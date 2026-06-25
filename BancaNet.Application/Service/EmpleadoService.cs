using BancaNet.Application.Interface.Repository;
using BancaNet.Application.Interface.Service;
using BancaNet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BancaNet.Application.Service
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IEmpleadoRepository _empleadoRepository;

        public EmpleadoService(IEmpleadoRepository empleadoRepository)
        {
            _empleadoRepository = empleadoRepository;
        }

        public async Task<Empleado?> ObtenerPorIdAsync(int id)
        {
            return await _empleadoRepository.ObtenerPorIdAsync(id);
        }

        public async Task<IEnumerable<Empleado>> ObtenerTodosAsync()
        {
            return await _empleadoRepository.ObtenerTodosAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            return await _empleadoRepository.ExisteNombreAsync(nombre);
        }

        public async Task CrearAsync(Empleado empleado)
        {
            await _empleadoRepository.CrearAsync(empleado);
        }

        public async Task ActualizarAsync(Empleado empleado)
        {
            await _empleadoRepository.ActualizarAsync(empleado);
        }

        public async Task EliminarAsync(int id)
        {
            await _empleadoRepository.EliminarAsync(id);
        }
    }
}
