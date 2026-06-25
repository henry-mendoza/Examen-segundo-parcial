using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere token JWT válido
    public class SucursalesController : ControllerBase
    {
        private readonly ISucursalService _sucursalService;

        public SucursalesController(ISucursalService sucursalService)
        {
            _sucursalService = sucursalService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sucursal>>> ObtenerTodos()
        {
            var sucursales = await _sucursalService.ObtenerTodosAsync();
            return Ok(sucursales);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Sucursal>> ObtenerPorId(int id)
        {
            var sucursal = await _sucursalService.ObtenerPorIdAsync(id);
            if (sucursal == null)
                return NotFound($"Sucursal con ID {id} no encontrado.");
            return Ok(sucursal);
        }

        [HttpPost]
        [Authorize(Roles = "Empleado")] // Solo empleados
        public async Task<ActionResult> Crear([FromBody] Sucursal sucursal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _sucursalService.ExisteNombreAsync(sucursal.Nombre))
                return BadRequest($"La sucursal con el nombre '{sucursal.Nombre}' ya existe.");

            await _sucursalService.CrearAsync(sucursal);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = sucursal.IdSucursal }, sucursal);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Empleado")] // Solo empleados
        public async Task<ActionResult> Actualizar(int id, [FromBody] Sucursal sucursal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != sucursal.IdSucursal)
                return BadRequest("El ID de la sucursal no coincide.");

            var existente = await _sucursalService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Sucursal con ID {id} no encontrado.");

            await _sucursalService.ActualizarAsync(sucursal);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Empleado")] // Solo empleados
        public async Task<ActionResult> Eliminar(int id)
        {
            var existente = await _sucursalService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Sucursal con ID {id} no encontrado.");

            await _sucursalService.EliminarAsync(id);
            return NoContent();
        }
    }
}
