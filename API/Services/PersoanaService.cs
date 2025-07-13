using Shared.DTOs;
using API.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class PersoanaService : IPersoanaService
    {
        private readonly IPersoanaRepository _persoanaRepository;

        public PersoanaService(IPersoanaRepository persoanaRepository)
        {
            _persoanaRepository = persoanaRepository;
        }

        public async Task<IEnumerable<PersoanaDTO>> GetAllPersonalAsync()
        {
            return await _persoanaRepository.GetAllAsync();
        }

        public async Task<PersoanaDTO> GetPersonaByIdAsync(int id)
        {
            return await _persoanaRepository.GetByIdAsync(id);
        }

        public async Task<int> CreatePersonaAsync(CreatePersoanaDTO personaDto)
        {
            // Explicitly cast or ensure both use Shared.DTOs.CreatePersoanaDTO
            return await _persoanaRepository.CreateAsync(personaDto);
        }

        public async Task<bool> UpdatePersonaAsync(UpdatePersoanaDTO personaDto)
        {
            // Explicitly cast or ensure both use Shared.DTOs.UpdatePersoanaDTO
            return await _persoanaRepository.UpdateAsync(personaDto);
        }

        public async Task<bool> DeletePersonaAsync(int id)
        {
            return await _persoanaRepository.DeleteAsync(id);
        }
    }
}