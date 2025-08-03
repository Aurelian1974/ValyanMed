using Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

namespace Client.Services
{
    public interface IPersoanaService
    {
        Task<List<PersoanaModel>> GetAllPersonalAsync();
        Task<PersoanaModel> GetPersoanaByIdAsync(int id);
        Task<PersoanaDTO> CreatePersoanaAsync(CreatePersoanaDTO persoana);
        Task<bool> UpdatePersoanaAsync(PersoanaModel persoana);
        Task<PersoanaService.DeleteResult> DeletePersoanaAsync(int id);
    }
}