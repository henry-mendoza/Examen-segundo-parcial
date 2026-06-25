using BancaNet.Application.Interface.Repository;
using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Service
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return await _clienteRepository.ObtenerPorIdAsync(id);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync()
        {
            return await _clienteRepository.ObtenerTodosAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            return await _clienteRepository.ExisteNombreAsync(nombre);
        }

        public async Task CrearAsync(Cliente cliente)
        {
            await _clienteRepository.CrearAsync(cliente);
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            await _clienteRepository.ActualizarAsync(cliente);
        }

        public async Task EliminarAsync(int id)
        {
            await _clienteRepository.EliminarAsync(id);
        }
    }
}
