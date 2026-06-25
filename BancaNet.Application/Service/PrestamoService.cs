using BancaNet.Application.Interface.Repository;
using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Service
{
    public class PrestamoService : IPrestamoService
    {
        private readonly IPrestamoRepository _prestamoRepository;

        public PrestamoService(IPrestamoRepository prestamoRepository)
        {
            _prestamoRepository = prestamoRepository;
        }

        public async Task<Prestamo?> ObtenerPorIdAsync(int id)
        {
            return await _prestamoRepository.ObtenerPorIdAsync(id);
        }

        public async Task<IEnumerable<Prestamo>> ObtenerTodosAsync()
        {
            return await _prestamoRepository.ObtenerTodosAsync();
        }

        public async Task CrearAsync(Prestamo prestamo)
        {
            await _prestamoRepository.CrearAsync(prestamo);
        }

        public async Task ActualizarAsync(Prestamo prestamo)
        {
            await _prestamoRepository.ActualizarAsync(prestamo);
        }

        public async Task EliminarAsync(int id)
        {
            await _prestamoRepository.EliminarAsync(id);
        }
    }
}
