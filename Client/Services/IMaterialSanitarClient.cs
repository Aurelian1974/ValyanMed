using Shared.DTOs;

namespace Client.Services
{
    public interface IMaterialSanitarClient
    {
        Task<List<MaterialSanitarDTO>> GetAllAsync();
        Task<MaterialSanitarDTO> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateMaterialSanitarDTO dto);
        Task<bool> UpdateAsync(UpdateMaterialSanitarDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<MaterialSanitarDTO>> GetPagedAsync(string? search, string? categorie, int page, int pageSize, string? sort);
        Task<IEnumerable<MaterialSanitarDTO>> GetAllGroupedAsync(string? search, string? categorie, string? groupBy, string? sort);
        Task<PagedResult<MaterialSanitarDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? groupBy, string? sort, int page, int pageSize);
    }
}