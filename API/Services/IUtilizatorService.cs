using Shared.DTOs;
using API.Models; // use AuthResult from API.Models
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IUtilizatorService
    {
        Task<IEnumerable<UtilizatorDTO>> GetAllUtilizatoriAsync();
        Task<UtilizatorDTO> GetUtilizatorByIdAsync(int id);
        Task<int> CreateUtilizatorAsync(CreateUtilizatorDTO utilizatorDto);
        Task<bool> UpdateUtilizatorAsync(UpdateUtilizatorDTO utilizatorDto);
        Task<bool> DeleteUtilizatorAsync(int id);
        Task<AuthResult> AuthenticateAsync(string usernameOrEmail, string password);
    }
}
