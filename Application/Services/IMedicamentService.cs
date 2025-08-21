using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

namespace Application.Services
{
    public interface IMedicamentService
    {
        Task<IEnumerable<MedicamentDTO>> GetAllAsync();
        Task<MedicamentDTO> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateMedicamentDTO dto);
        Task<bool> UpdateAsync(UpdateMedicamentDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<MedicamentDTO>> GetPagedAsync(string? search, string? status, int page, int pageSize, string? sort, string? groupBy);
        Task<IEnumerable<MedicamentDTO>> GetAllGroupedAsync(string? search, string? status, string? groupBy, string? sort);
    }
}
