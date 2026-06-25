using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Interface.Service
{
    public interface IClienteService
    {
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Cliente>> ObtenerTodosAsync();
        Task<bool> ExisteNombreAsync(string nombre);
        Task CrearAsync(Cliente cliente);
        Task ActualizarAsync(Cliente cliente);
        Task EliminarAsync(int id);
    }
}
