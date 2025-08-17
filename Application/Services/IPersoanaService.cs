using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IPersoanaService
    {
        Task<IEnumerable<PersoanaDTO>> GetAllPersonalAsync();
        Task<PersoanaDTO> GetPersonaByIdAsync(int id);
        Task<int> CreatePersonaAsync(CreatePersoanaDTO personaDto);
        Task<bool> UpdatePersonaAsync(UpdatePersoanaDTO personaDto);
        Task<bool> DeletePersonaAsync(int id);
    }
}
