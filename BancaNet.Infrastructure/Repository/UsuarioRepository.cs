using BancaNet.Application.Interface.Repository;
using BancaNet.Domain.Entities;
using BancaNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancaNet.Infrastructure.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context) 
        {
            _context = context;
        }
         
        public async Task<int> ContarAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<ApplicationUser?> ObtenerPorIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<ApplicationUser>> ObtenerTodosAsync(int pagina, int tamano)
        {
            return await _context.Users
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }
    }
}
