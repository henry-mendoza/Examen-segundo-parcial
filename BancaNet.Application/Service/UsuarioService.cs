using BancaNet.Application.Interface.Repository;
using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Service
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ApplicationUser?> ObtenerPorIdAsync(string id)
        {
            return await _usuarioRepository.ObtenerPorIdAsync(id);
        }

        public async Task<IEnumerable<ApplicationUser>> ObtenerTodosAsync(int pagina, int tamano)
        {
            return await _usuarioRepository.ObtenerTodosAsync(pagina, tamano);
        }

        public async Task<int> ContarAsync()
        {
            return await _usuarioRepository.ContarAsync();
        }
    }
}
