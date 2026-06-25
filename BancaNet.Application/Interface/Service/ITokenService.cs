using BancaNet.Domain.Entities;
using System.Threading.Tasks;

namespace BancaNet.Application.Interface.Service
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    }
}
