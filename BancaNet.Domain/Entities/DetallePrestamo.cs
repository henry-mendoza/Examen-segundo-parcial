using System.Text.Json.Serialization;

namespace BancaNet.Domain.Entities;

public class DetallePrestamo
{
    public int IdDetalle { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal Interes { get; set; }
    public decimal Abono { get; set; }
    public DateTime FechaAbono { get; set; }
    public decimal Mora { get; set; }
    public decimal Total { get; set; }

    // Navigation property
    [JsonIgnore]
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
