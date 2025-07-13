using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Repositories
{
    public interface IPersoanaRepository
    {
        Task<IEnumerable<PersoanaDTO>> GetAllAsync();
        Task<PersoanaDTO> GetByIdAsync(int id);
        Task<int> CreateAsync(CreatePersoanaDTO persoana);
        Task<bool> UpdateAsync(UpdatePersoanaDTO persoana);
        Task<bool> DeleteAsync(int id);
    }
}