using BancaNet.Infrastructure.Data;
using BancaNet.Domain.Entities;
using BancaNet.Application.Interface.Service;
using BancaNet.Infrastructure.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Load environment variables from .env file
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}
else
{
    DotNetEnv.Env.Load();
}

var host = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("PORT") ?? "5432";
var database = Environment.GetEnvironmentVariable("DATABASE") ?? "BancaNet";
var user = Environment.GetEnvironmentVariable("USER") ?? "postgres";
var password = Environment.GetEnvironmentVariable("PASSWORD") ?? "admin123";

// Construir la cadena de conexión a la base de datos
var connectionString =
    $"Host={host};" +
    $"Port={port};" +
    $"Database={database};" +
    $"Username={user};" +
    $"Password={password};" +
    $"SSL Mode=Require;" +
    $"Trust Server Certificate=true;";

// Configure ApplicationDbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authorization = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorization))
            {
                if (authorization.StartsWith("Token ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authorization.Substring("Token ".Length).Trim();
                }
                else if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authorization.Substring("Bearer ".Length).Trim();
                }
                else
                {
                    // Si no tiene prefijo, asumir que es el token crudo
                    context.Token = authorization.Trim();
                }
            }
            return Task.CompletedTask;
        }
    };
});

// Register Repositories and Services
builder.Services.AddScoped<BancaNet.Application.Interface.Repository.IClienteRepository, BancaNet.Infrastructure.Repository.ClienteRepository>();
builder.Services.AddScoped<BancaNet.Application.Interface.Repository.IEmpleadoRepository, BancaNet.Infrastructure.Repository.EmpleadoRepository>();
builder.Services.AddScoped<BancaNet.Application.Interface.Repository.IPrestamoRepository, BancaNet.Infrastructure.Repository.PrestamoRepository>();
builder.Services.AddScoped<BancaNet.Application.Interface.Repository.ISucursalRepository, BancaNet.Infrastructure.Repository.SucursalRepository>();
builder.Services.AddScoped<BancaNet.Application.Interface.Repository.IUsuarioRepository, BancaNet.Infrastructure.Repository.UsuarioRepository>();

builder.Services.AddScoped<BancaNet.Application.Interface.Service.IUsuarioService, BancaNet.Application.Service.UsuarioService>();
builder.Services.AddScoped<BancaNet.Application.Interface.Service.IClienteService, BancaNet.Application.Service.ClienteService>();
builder.Services.AddScoped<BancaNet.Application.Interface.Service.IEmpleadoService, BancaNet.Application.Service.EmpleadoService>();
builder.Services.AddScoped<BancaNet.Application.Interface.Service.IPrestamoService, BancaNet.Application.Service.PrestamoService>();
builder.Services.AddScoped<BancaNet.Application.Interface.Service.ISucursalService, BancaNet.Application.Service.SucursalService>();
builder.Services.AddScoped<ITokenService, TokenService>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Configure native OpenAPI with JWT Security Scheme
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "BancaNet API";
        document.Info.Version = "v1";

        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = "Token",
            BearerFormat = "JWT",
            Description = "Ingresa 'Token' seguido de un espacio y tu token JWT. Ejemplo: 'Token eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Token", scheme);

        var requirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Token", document),
                new List<string>()
            }
        };
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirement);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "BancaNet API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Inicializar datos por defecto (Seeder) para permitir pruebas sin controladores ocultos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        // 1. Sembrar Roles
        string[] roles = new string[] { "Empleado", "Cliente" };
        foreach (var role in roles)
        {
            if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }
        }

        // 2. Sembrar Sucursal
        if (!context.Sucursales.Any())
        {
            context.Sucursales.Add(new BancaNet.Domain.Entities.Sucursal
            {
                Nombre = "Sucursal Central Principal",
                Direccion = "Centro Financiero Central, Local 10"
            });
            context.SaveChanges();
        }

        // 3. Sembrar Usuario Empleado Admin
        string adminEmail = "admin@bancanet.com";
        var existingUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (existingUser == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                NombreCompleto = "Administrador Principal",
                EmailConfirmed = true
            };

            var result = userManager.CreateAsync(adminUser, "Password123!").GetAwaiter().GetResult();
            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, "Empleado").GetAwaiter().GetResult();

                // Sembrar perfil de Empleado
                var empleadoPerfil = new Empleado
                {
                    IdUsuario = adminUser.Id, // Enlazado al ID de Identity (string)
                    Nombre = "Administrador Principal",
                    Cargo = "Administrador",
                    IdSucursal = 1 // Sucursal sembrada arriba
                };
                context.Empleados.Add(empleadoPerfil);
                context.SaveChanges();
            }
        }
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine($"Error al inicializar datos por defecto: {ex.Message}");
    }
}

app.Run();
