using Shared.DTOs;

namespace Client.Services
{
    public interface IMedicamentClient
    {
        Task<List<MedicamentDTO>> GetAllAsync();
        Task<MedicamentDTO> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateMedicamentDTO dto);
        Task<bool> UpdateAsync(UpdateMedicamentDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<MedicamentDTO>> GetPagedAsync(string? search, string? status, int page, int pageSize, string? sort);
    }
}
