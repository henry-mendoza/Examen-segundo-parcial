using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Interface.Repository
{
    public interface IEmpleadoRepository
    {
        Task<Empleado?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Empleado>> ObtenerTodosAsync();
        Task<bool> ExisteNombreAsync(string nombre);
        Task CrearAsync(Empleado empleado);
        Task ActualizarAsync(Empleado empleado);
        Task EliminarAsync(int id);
    }
}
