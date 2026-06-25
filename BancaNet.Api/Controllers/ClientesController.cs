using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using BancaNet.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ApplicationDbContext _context;

        public ClientesController(IClienteService clienteService, ApplicationDbContext context)
        {
            _clienteService = clienteService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult<IEnumerable<Cliente>>> ObtenerTodos()
        {
            var clientes = await _clienteService.ObtenerTodosAsync();
            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Cliente>> ObtenerPorId(int id)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(id);
            if (cliente == null)
                return NotFound($"Cliente con ID {id} no encontrado.");

            // El empleado puede ver a cualquiera. El cliente solo se puede ver a sí mismo.
            if (User.IsInRole("Cliente"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                             ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return StatusCode(403, "No se pudo resolver el ID de usuario en los claims del token.");

                if (cliente.IdUsuario != userId)
                {
                    return StatusCode(403, "No tienes autorización para ver la información de este cliente.");
                }
            }

            return Ok(cliente);
        }

        [HttpPost]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult> Crear([FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _clienteService.ExisteNombreAsync(cliente.Nombre))
                return BadRequest($"El cliente con el nombre '{cliente.Nombre}' ya existe.");

            await _clienteService.CrearAsync(cliente);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.IdCliente }, cliente);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Actualizar(int id, [FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != cliente.IdCliente)
                return BadRequest("El ID del cliente no coincide.");

            var existente = await _clienteService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Cliente con ID {id} no encontrado.");

            // El empleado puede modificar a cualquiera. El cliente solo se puede modificar a sí mismo.
            if (User.IsInRole("Cliente"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                             ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId) || existente.IdUsuario != userId)
                {
                    return StatusCode(403, "No tienes autorización para modificar la información de este cliente.");
                }
            }

            await _clienteService.ActualizarAsync(cliente);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var existente = await _clienteService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Cliente con ID {id} no encontrado.");

            await _clienteService.EliminarAsync(id);
            return NoContent();
        }
    }
}
