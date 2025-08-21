using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;
using Core.Interfaces;

namespace Application.Services
{
    public class MedicamentService : IMedicamentService
    {
        private readonly IMedicamentRepository _repo;
        public MedicamentService(IMedicamentRepository repo) => _repo = repo;

        public Task<IEnumerable<MedicamentDTO>> GetAllAsync() => _repo.GetAllAsync();
        public Task<MedicamentDTO> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<int> CreateAsync(CreateMedicamentDTO dto) => _repo.CreateAsync(dto);
        public Task<bool> UpdateAsync(UpdateMedicamentDTO dto) => _repo.UpdateAsync(dto);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
        public Task<PagedResult<MedicamentDTO>> GetPagedAsync(string? search, string? status, int page, int pageSize, string? sort, string? groupBy) => _repo.GetPagedAsync(search, status, page, pageSize, sort, groupBy);
        public Task<IEnumerable<MedicamentDTO>> GetAllGroupedAsync(string? search, string? status, string? groupBy, string? sort) => _repo.GetAllGroupedAsync(search, status, groupBy, sort);
    }
}
