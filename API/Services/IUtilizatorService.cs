using Shared.DTOs;
using API.Models;
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

    public class AuthResult
    {
        public UtilizatorDb User { get; set; }
        public string NumeComplet { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
