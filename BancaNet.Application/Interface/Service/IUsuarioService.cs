using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Interface.Service
{
    public interface IUsuarioService
    {
        Task<ApplicationUser?> ObtenerPorIdAsync(string id);
        Task<IEnumerable<ApplicationUser>> ObtenerTodosAsync(int pagina, int tamano);
        Task<int> ContarAsync();
    }
}
