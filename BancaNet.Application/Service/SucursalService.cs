using BancaNet.Application.Interface.Repository;
using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Service
{
    public class SucursalService : ISucursalService
    {
        private readonly ISucursalRepository _sucursalRepository;

        public SucursalService(ISucursalRepository sucursalRepository)
        {
            _sucursalRepository = sucursalRepository;
        }

        public async Task<Sucursal?> ObtenerPorIdAsync(int id)
        {
            return await _sucursalRepository.ObtenerPorIdAsync(id);
        }

        public async Task<IEnumerable<Sucursal>> ObtenerTodosAsync()
        {
            return await _sucursalRepository.ObtenerTodosAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            return await _sucursalRepository.ExisteNombreAsync(nombre);
        }

        public async Task CrearAsync(Sucursal sucursal)
        {
            await _sucursalRepository.CrearAsync(sucursal);
        }

        public async Task ActualizarAsync(Sucursal sucursal)
        {
            await _sucursalRepository.ActualizarAsync(sucursal);
        }

        public async Task EliminarAsync(int id)
        {
            await _sucursalRepository.EliminarAsync(id);
        }
    }
}
