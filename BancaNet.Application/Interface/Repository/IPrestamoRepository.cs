using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Interface.Repository
{
    public interface IPrestamoRepository
    {
        Task<Prestamo?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Prestamo>> ObtenerTodosAsync();
        Task CrearAsync(Prestamo prestamo);
        Task ActualizarAsync(Prestamo prestamo);
        Task EliminarAsync(int id);
    }
}
