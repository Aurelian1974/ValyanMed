using Shared.DTOs;

namespace Core.Interfaces
{
    public interface IPartenerRepository
    {
        Task<IEnumerable<PartenerDTO>> GetAllAsync();
        Task<PartenerDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreatePartenerDTO dto);
        Task<bool> UpdateAsync(UpdatePartenerDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<PartenerDTO>> GetPagedAsync(string? search, string? judet, int page, int pageSize, string? sort);
    }
}