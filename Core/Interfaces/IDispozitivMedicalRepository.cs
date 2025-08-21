using Shared.DTOs;

namespace Core.Interfaces
{
    public interface IDispozitivMedicalRepository
    {
        Task<IEnumerable<DispozitivMedicalDTO>> GetAllAsync();
        Task<DispozitivMedicalDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateDispozitivMedicalDTO dto);
        Task<bool> UpdateAsync(UpdateDispozitivMedicalDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<DispozitivMedicalDTO>> GetPagedAsync(string? search, string? categorie, string? clasaRisc, int page, int pageSize, string? sort);
        Task<IEnumerable<DispozitivMedicalDTO>> GetAllGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort);
        Task<PagedResult<DispozitivMedicalDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort, int page, int pageSize);
    }
}