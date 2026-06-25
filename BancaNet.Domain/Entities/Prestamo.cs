using System.Text.Json.Serialization;

namespace BancaNet.Domain.Entities;

public class Prestamo
{
    public int IdPrestamo { get; set; }
    public decimal Monto { get; set; }
    public decimal TasaInteres { get; set; }
    public int IdCliente { get; set; }
    public int IdEmpleado { get; set; }
    public int IdDetalle { get; set; }
    public DateTime FechaVencimientoActual { get; set; }
    public decimal TasaMoraDiaria { get; set; }
    public int DiasGracia { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Cliente? Cliente { get; set; }

    [JsonIgnore]
    public Empleado? Empleado { get; set; }

    [JsonIgnore]
    public DetallePrestamo? DetallePrestamo { get; set; }
}
