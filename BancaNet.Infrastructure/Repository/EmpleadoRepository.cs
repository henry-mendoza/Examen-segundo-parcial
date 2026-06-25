using BancaNet.Application.Interface.Repository;
using BancaNet.Domain.Entities;
using BancaNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancaNet.Infrastructure.Repository
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly ApplicationDbContext _context;

        public EmpleadoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Empleado?> ObtenerPorIdAsync(int id)
        {
            return await _context.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == id);
        }

        public async Task<IEnumerable<Empleado>> ObtenerTodosAsync()
        {
            return await _context.Empleados.ToListAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)    
        {
            var nombreNormalizado = nombre.Trim().ToLower();
            return await _context.Empleados.AnyAsync(e => e.Nombre.Trim().ToLower() == nombreNormalizado);
        }

        public async Task CrearAsync(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Empleado empleado)
        {
            _context.Empleados.Update(empleado);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            await _context.Empleados.Where(e => e.IdEmpleado == id).ExecuteDeleteAsync();
        }
    }
}
