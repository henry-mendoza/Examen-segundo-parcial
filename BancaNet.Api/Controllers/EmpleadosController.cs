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
    [Authorize(Roles = "Empleado")]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoService _empleadoService;

        public EmpleadosController(IEmpleadoService empleadoService)
        {
            _empleadoService = empleadoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empleado>>> ObtenerTodos()
        {
            var empleados = await _empleadoService.ObtenerTodosAsync();
            return Ok(empleados);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Empleado>> ObtenerPorId(int id)
        {
            var empleado = await _empleadoService.ObtenerPorIdAsync(id);
            if (empleado == null)
                return NotFound($"Empleado con ID {id} no encontrado.");
            return Ok(empleado);
        }

        [HttpPost]
        public async Task<ActionResult> Crear([FromBody] Empleado empleado)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _empleadoService.ExisteNombreAsync(empleado.Nombre))
                return BadRequest($"El empleado con el nombre '{empleado.Nombre}' ya existe.");

            await _empleadoService.CrearAsync(empleado);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = empleado.IdEmpleado }, empleado);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Actualizar(int id, [FromBody] Empleado empleado)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != empleado.IdEmpleado)
                return BadRequest("El ID del empleado no coincide.");

            var existente = await _empleadoService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Empleado con ID {id} no encontrado.");

            await _empleadoService.ActualizarAsync(empleado);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var existente = await _empleadoService.ObtenerPorIdAsync(id);
            if (existente == null)
                return NotFound($"Empleado con ID {id} no encontrado.");

            await _empleadoService.EliminarAsync(id);
            return NoContent();
        }
    }
}
