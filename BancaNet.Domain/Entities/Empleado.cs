using System.Text.Json.Serialization;

namespace BancaNet.Domain.Entities;

public class Empleado
{
    public int IdEmpleado { get; set; }
    public string IdUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public int IdSucursal { get; set; }

    // Navigation properties
    [JsonIgnore]
    public ApplicationUser? Usuario { get; set; }

    [JsonIgnore]
    public Sucursal? Sucursal { get; set; }

    [JsonIgnore]
    public ICollection<Prestamo> PrestamosGestionados { get; set; } = new List<Prestamo>();
}
