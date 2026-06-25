using BancaNet.Application.Interface.Repository;
using BancaNet.Domain.Entities;
using BancaNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancaNet.Infrastructure.Repository
{
    public class SucursalRepository : ISucursalRepository
    {
        private readonly ApplicationDbContext _context;

        public SucursalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Sucursal?> ObtenerPorIdAsync(int id)
        {
            return await _context.Sucursales.FirstOrDefaultAsync(s => s.IdSucursal == id);
        }

        public async Task<IEnumerable<Sucursal>> ObtenerTodosAsync()
        {
            return await _context.Sucursales.ToListAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            var nombreNormalizado = nombre.Trim().ToLower();
            return await _context.Sucursales.AnyAsync(s => s.Nombre.Trim().ToLower() == nombreNormalizado);
        }

        public async Task CrearAsync(Sucursal sucursal)
        {
            _context.Sucursales.Add(sucursal);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Sucursal sucursal)
        {
            _context.Sucursales.Update(sucursal);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            await _context.Sucursales.Where(s => s.IdSucursal == id).ExecuteDeleteAsync();
        }
    }
}
