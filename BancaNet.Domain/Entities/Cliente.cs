using System.Text.Json.Serialization;

namespace BancaNet.Domain.Entities;

public class Cliente
{
    public int IdCliente { get; set; }
    public string IdUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    // Navigation properties
    [JsonIgnore]
    public ApplicationUser? Usuario { get; set; }

    [JsonIgnore]
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
