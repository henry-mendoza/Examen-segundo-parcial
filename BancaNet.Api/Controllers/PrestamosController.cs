using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using BancaNet.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BancaNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere token JWT válido
    public class PrestamosController : ControllerBase
    {
        private readonly IPrestamoService _prestamoService;
        private readonly ApplicationDbContext _context;

        public PrestamosController(IPrestamoService prestamoService, ApplicationDbContext context)
        {
            _prestamoService = prestamoService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prestamo>>> ObtenerTodos()
        {
            if (User.IsInRole("Empleado"))
            {
                var prestamos = await _prestamoService.ObtenerTodosAsync();
                return Ok(prestamos);
            }
            else if (User.IsInRole("Cliente"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                             ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return StatusCode(403, "No se pudo resolver el ID de usuario en el token.");

                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdUsuario == userId);
                if (cliente == null)
                    return NotFound("No se encontró el perfil de cliente correspondiente.");

                // Filtrar préstamos por el cliente
                var prestamosCliente = await _context.Prestamos
                    .Where(p => p.IdCliente == cliente.IdCliente)
                    .ToListAsync();

                return Ok(prestamosCliente);
            }
            return StatusCode(403, "Rol no autorizado.");
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Prestamo>> ObtenerPorId(int id)
        {
            var prestamo = await _prestamoService.ObtenerPorIdAsync(id);
            if (prestamo == null)
                return NotFound($"Prestamo con ID {id} no encontrado.");

            if (User.IsInRole("Cliente"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                             ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdUsuario == userId);
                if (cliente == null || prestamo.IdCliente != cliente.IdCliente)
                {
                    return StatusCode(403, "No tienes autorización para ver este préstamo.");
                }
            }

            return Ok(prestamo);
        }

        [HttpPost]
        [Authorize(Roles = "Empleado")] // Solo empleados pueden crear/aprobar préstamos
        public async Task<ActionResult> Crear([FromBody] Prestamo prestamo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar si existe el cliente
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == prestamo.IdCliente);
            if (!clienteExiste)
                return BadRequest($"El cliente con ID {prestamo.IdCliente} no existe.");

            // Validar si existe el empleado
            var empleadoExiste = await _context.Empleados.AnyAsync(e => e.IdEmpleado == prestamo.IdEmpleado);
            if (!empleadoExiste)
                return BadRequest($"El empleado con ID {prestamo.IdEmpleado} no existe.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Establecer valores iniciales por defecto para mora y días de gracia si no se proveen
                if (prestamo.TasaMoraDiaria <= 0)
                    prestamo.TasaMoraDiaria = 0.05m; // 0.05% diario por defecto
                
                if (prestamo.DiasGracia <= 0)
                    prestamo.DiasGracia = 3; // 3 días de gracia por defecto

                // La primera fecha de vencimiento es en 1 mes
                prestamo.FechaVencimientoActual = DateTime.UtcNow.Date.AddMonths(1);

                // 1. Crear el primer detalle del préstamo (inicial)
                var detalleInicial = new DetallePrestamo
                {
                    SaldoInicial = prestamo.Monto,
                    Interes = 0,
                    Mora = 0,
                    Abono = 0,
                    FechaAbono = DateTime.UtcNow.Date,
                    Total = prestamo.Monto
                };

                _context.DetallesPrestamos.Add(detalleInicial);
                await _context.SaveChangesAsync(); // Genera IdDetalle

                // 2. Asociar el detalle inicial al préstamo
                prestamo.IdDetalle = detalleInicial.IdDetalle;

                // 3. Crear el préstamo
                await _prestamoService.CrearAsync(prestamo);

                await transaction.CommitAsync();
                return CreatedAtAction(nameof(ObtenerPorId), new { id = prestamo.IdPrestamo }, prestamo);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Empleado")] // Solo empleados
        public async Task<ActionResult> Actualizar(int id, [FromBody] Prestamo prestamo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != prestamo.IdPrestamo)
                return BadRequest("El ID del préstamo no coincide.");

            var existente = await _prestamoService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Prestamo con ID {id} no encontrado.");

            await _prestamoService.ActualizarAsync(prestamo);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Empleado")] // Solo empleados
        public async Task<ActionResult> Eliminar(int id)
        {
            var existente = await _prestamoService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Prestamo con ID {id} no encontrado.");

            await _prestamoService.EliminarAsync(id);
            return NoContent();
        }

        // --- Endpoint de Abonos (Amortizaciones) ---

        [HttpPost("{id:int}/abonos")]
        [Authorize(Roles = "Empleado")] // Solo empleados registran abonos físicamente
        public async Task<ActionResult> RegistrarAbono(int id, [FromBody] RegistrarAbonoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var prestamo = await _context.Prestamos
                .Include(p => p.DetallePrestamo)
                .FirstOrDefaultAsync(p => p.IdPrestamo == id);

            if (prestamo == null)
                return NotFound($"Prestamo con ID {id} no encontrado.");

            if (prestamo.DetallePrestamo == null)
                return BadRequest("El préstamo no tiene un historial de detalle válido.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Obtener saldo inicial (es el total del último detalle)
                decimal saldoInicial = prestamo.DetallePrestamo.Total;

                if (saldoInicial <= 0)
                    return BadRequest("El préstamo ya se encuentra completamente cancelado.");

                if (dto.MontoAbono <= 0)
                    return BadRequest("El monto del abono debe ser mayor a cero.");

                // Usar fecha del DTO o la fecha actual UTC
                DateTime fechaPago = dto.FechaAbono?.Date ?? DateTime.UtcNow.Date;

                // Calcular interés ordinario en base a la tasa de interés del préstamo (anual / 12 para mensual)
                decimal tasaMensual = prestamo.TasaInteres / 100 / 12;
                decimal interesCalculado = Math.Round(saldoInicial * tasaMensual, 2);

                // Calcular mora (interés moratorio) por días de retraso excedidos
                decimal moraCalculada = 0;
                int diasRetraso = 0;
                DateTime limitePago = prestamo.FechaVencimientoActual.AddDays(prestamo.DiasGracia);

                if (fechaPago > limitePago)
                {
                    diasRetraso = (fechaPago - prestamo.FechaVencimientoActual).Days;
                    moraCalculada = Math.Round(saldoInicial * (prestamo.TasaMoraDiaria / 100) * diasRetraso, 2);
                }

                // Calcular total restante
                decimal totalRestante = Math.Round(saldoInicial + interesCalculado + moraCalculada - dto.MontoAbono, 2);
                if (totalRestante < 0)
                    totalRestante = 0;

                // Crear nuevo registro de DetallePrestamo
                var nuevoDetalle = new DetallePrestamo
                {
                    SaldoInicial = saldoInicial,
                    Interes = interesCalculado,
                    Mora = moraCalculada,
                    Abono = dto.MontoAbono,
                    FechaAbono = fechaPago,
                    Total = totalRestante
                };

                _context.DetallesPrestamos.Add(nuevoDetalle);
                await _context.SaveChangesAsync(); // Genera nuevo IdDetalle

                // Actualizar la referencia del detalle en el préstamo al más reciente y rodar la fecha de vencimiento
                prestamo.IdDetalle = nuevoDetalle.IdDetalle;
                prestamo.FechaVencimientoActual = prestamo.FechaVencimientoActual.AddMonths(1);
                
                _context.Prestamos.Update(prestamo);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Abono registrado exitosamente.",
                    SaldoAnterior = saldoInicial,
                    InteresGenerado = interesCalculado,
                    MoraAplicada = moraCalculada,
                    DiasDeRetraso = diasRetraso,
                    Abonado = dto.MontoAbono,
                    NuevoSaldoTotal = totalRestante,
                    SiguienteVencimiento = prestamo.FechaVencimientoActual.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { Error = ex.Message });
            }
        }
    }

    public class RegistrarAbonoDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto del abono debe ser mayor a cero.")]
        public decimal MontoAbono { get; set; }

        public DateTime? FechaAbono { get; set; }
    }
}
