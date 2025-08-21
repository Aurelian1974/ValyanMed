using Core.Interfaces;
using Shared.DTOs;

namespace Application.Services
{
    public interface IPartenerService
    {
        Task<IEnumerable<PartenerDTO>> GetAllAsync();
        Task<PagedResult<PartenerDTO>> GetPagedAsync(string? search, string? judet, int page, int pageSize, string? sort);
        Task<PartenerDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreatePartenerDTO dto);
        Task<bool> UpdateAsync(UpdatePartenerDTO dto);
        Task<bool> DeleteAsync(int id);
    }

    public class PartenerService : IPartenerService
    {
        private readonly IPartenerRepository _repository;

        public PartenerService(IPartenerRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PartenerDTO>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PagedResult<PartenerDTO>> GetPagedAsync(string? search, string? judet, int page, int pageSize, string? sort)
        {
            return await _repository.GetPagedAsync(search, judet, page, pageSize, sort);
        }

        public async Task<PartenerDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(CreatePartenerDTO dto)
        {
            return await _repository.CreateAsync(dto);
        }

        public async Task<bool> UpdateAsync(UpdatePartenerDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}