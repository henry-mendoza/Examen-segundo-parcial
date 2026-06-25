using BancaNet.Application.Interface.Repository;
using BancaNet.Domain.Entities;
using BancaNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancaNet.Infrastructure.Repository
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ApplicationDbContext _context;

        public ClienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            var nombreNormalizado = nombre.Trim().ToLower();
            return await _context.Clientes.AnyAsync(c => c.Nombre.Trim().ToLower() == nombreNormalizado);
        }

        public async Task CrearAsync(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            await _context.Clientes.Where(c => c.IdCliente == id).ExecuteDeleteAsync();
        }
    }
}
