using Shared.DTOs;
using Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PersoanaService : IPersoanaService
    {
        private readonly IPersoanaRepository _persoanaRepository;

        public PersoanaService(IPersoanaRepository persoanaRepository)
        {
            _persoanaRepository = persoanaRepository;
        }

        public async Task<IEnumerable<PersoanaDTO>> GetAllPersonalAsync() => await _persoanaRepository.GetAllAsync();
        public async Task<PersoanaDTO> GetPersonaByIdAsync(int id) => await _persoanaRepository.GetByIdAsync(id);
        public async Task<int> CreatePersonaAsync(CreatePersoanaDTO personaDto) => await _persoanaRepository.CreateAsync(personaDto);
        public async Task<bool> UpdatePersonaAsync(UpdatePersoanaDTO personaDto) => await _persoanaRepository.UpdateAsync(personaDto);
        public async Task<bool> DeletePersonaAsync(int id) => await _persoanaRepository.DeleteAsync(id);
    }
}
