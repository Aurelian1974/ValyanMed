using Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Services
{
    public interface IPersoanaService
    {
        Task<List<PersoanaModel>> GetAllPersonalAsync();
        Task<PersoanaModel> GetPersoanaByIdAsync(int id);
        Task<int> CreatePersoanaAsync(PersoanaModel persoana);
        Task<bool> UpdatePersoanaAsync(PersoanaModel persoana);
        Task<bool> DeletePersoanaAsync(int id);
    }
}