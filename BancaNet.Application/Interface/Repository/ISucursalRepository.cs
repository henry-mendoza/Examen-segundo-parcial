using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Interface.Repository
{
    public interface ISucursalRepository
    {
        Task<Sucursal?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Sucursal>> ObtenerTodosAsync();
        Task<bool> ExisteNombreAsync(string nombre);
        Task CrearAsync(Sucursal sucursal);
        Task ActualizarAsync(Sucursal sucursal);
        Task EliminarAsync(int id);
    }
}
