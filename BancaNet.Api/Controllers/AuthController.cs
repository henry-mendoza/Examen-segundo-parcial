using BancaNet.Domain.Entities;
using BancaNet.Application.Interface.Service;
using BancaNet.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BancaNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar rol
            var rolNormalizado = dto.Rol.Trim();
            if (rolNormalizado != "Cliente" && rolNormalizado != "Empleado")
            {
                return BadRequest("El rol especificado debe ser 'Cliente' o 'Empleado'.");
            }

            // Iniciar transacción de base de datos
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Crear el usuario en ASP.NET Identity
                var identityUser = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    NombreCompleto = dto.NombreCompleto
                };

                var result = await _userManager.CreateAsync(identityUser, dto.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // Asegurar que el rol exista
                if (!await _roleManager.RoleExistsAsync(rolNormalizado))
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolNormalizado));
                }

                // Asignar el rol al usuario
                await _userManager.AddToRoleAsync(identityUser, rolNormalizado);

                // 2. Crear el perfil correspondiente (Cliente o Empleado)
                if (rolNormalizado == "Cliente")
                {
                    if (string.IsNullOrWhiteSpace(dto.Direccion) || string.IsNullOrWhiteSpace(dto.Telefono))
                    {
                        throw new ArgumentException("Los campos de dirección y teléfono son requeridos para un Cliente.");
                    }

                    var cliente = new Cliente
                    {
                        IdUsuario = identityUser.Id,
                        Nombre = dto.NombreCompleto,
                        Direccion = dto.Direccion,
                        Telefono = dto.Telefono
                    };
                    _context.Clientes.Add(cliente);
                }
                else // Empleado
                {
                    var cargo = dto.Cargo ?? "Cajero";
                    var sucursalId = dto.IdSucursal ?? 1;

                    // Validar si existe la sucursal
                    var sucursalExiste = await _context.Sucursales.AnyAsync(s => s.IdSucursal == sucursalId);
                    if (!sucursalExiste)
                    {
                        throw new ArgumentException($"La sucursal con ID {sucursalId} no existe.");
                    }

                    var empleado = new Empleado
                    {
                        IdUsuario = identityUser.Id,
                        Nombre = dto.NombreCompleto,
                        Cargo = cargo,
                        IdSucursal = sucursalId
                    };
                    _context.Empleados.Add(empleado);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Usuario registrado exitosamente.", Email = dto.Email, Rol = rolNormalizado, IdUsuario = identityUser.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized("Credenciales de acceso incorrectas.");
            }

            var token = await _tokenService.GenerateJwtTokenAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var rol = roles.Count > 0 ? roles[0] : "Sin Rol";

            return Ok(new
            {
                Token = token,
                Email = user.Email,
                Rol = rol,
                IdUsuario = user.Id
            });
        }
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(4)]
        public string Password { get; set; } = null!;

        [Required]
        public string NombreCompleto { get; set; } = null!;

        [Required]
        public string Rol { get; set; } = null!; // "Cliente" o "Empleado"

        // Campos específicos de Cliente
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }

        // Campos específicos de Empleado
        public string? Cargo { get; set; }
        public int? IdSucursal { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
