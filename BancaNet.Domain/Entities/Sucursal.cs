using System.Text.Json.Serialization;

namespace BancaNet.Domain.Entities;

public class Sucursal
{
    public int IdSucursal { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    // Navigation property
    [JsonIgnore]
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
