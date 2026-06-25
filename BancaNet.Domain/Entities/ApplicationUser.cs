

using Microsoft.AspNetCore.Identity;

namespace BancaNet.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = null!;

        [System.Text.Json.Serialization.JsonIgnore]
        public Cliente? Cliente { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Empleado? Empleado { get; set; }

    }
}
