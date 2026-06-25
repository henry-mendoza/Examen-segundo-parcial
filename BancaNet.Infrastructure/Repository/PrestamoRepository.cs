using BancaNet.Application.Interface.Repository;
using BancaNet.Domain.Entities;
using BancaNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancaNet.Infrastructure.Repository
{
    public class PrestamoRepository : IPrestamoRepository
    {
        private readonly ApplicationDbContext _context;

        public PrestamoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Prestamo?> ObtenerPorIdAsync(int id)
        {
            return await _context.Prestamos.FirstOrDefaultAsync(p => p.IdPrestamo == id);
        }

        public async Task<IEnumerable<Prestamo>> ObtenerTodosAsync()
        {
            return await _context.Prestamos.ToListAsync();
        }

        public async Task CrearAsync(Prestamo prestamo)
        {
            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Prestamo prestamo)
        {
            _context.Prestamos.Update(prestamo);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            await _context.Prestamos.Where(p => p.IdPrestamo == id).ExecuteDeleteAsync();
        }
    }
}
