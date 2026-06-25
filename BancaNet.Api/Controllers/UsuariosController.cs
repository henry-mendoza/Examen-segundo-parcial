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
    [Authorize(Roles = "Empleado")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<ActionResult> ObtenerTodos([FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
        {
            if (pagina < 1 || tamano < 1)
                return BadRequest("La página y el tamaño de la página deben ser mayores que 0.");

            var usuarios = await _usuarioService.ObtenerTodosAsync(pagina, tamano);
            var total = await _usuarioService.ContarAsync();

            return Ok(new
            {
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                Usuarios = usuarios
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> ObtenerPorId(string id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario == null)
                return NotFound($"Usuario con ID {id} no encontrado.");

            return Ok(usuario);
        }
    }
}
