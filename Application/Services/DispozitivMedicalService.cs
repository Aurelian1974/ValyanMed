using Core.Interfaces;
using Shared.DTOs;

namespace Application.Services
{
    public interface IDispozitivMedicalService
    {
        Task<IEnumerable<DispozitivMedicalDTO>> GetAllAsync();
        Task<PagedResult<DispozitivMedicalDTO>> GetPagedAsync(string? search, string? categorie, string? clasaRisc, int page, int pageSize, string? sort);
        Task<DispozitivMedicalDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateDispozitivMedicalDTO dto);
        Task<bool> UpdateAsync(UpdateDispozitivMedicalDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<DispozitivMedicalDTO>> GetAllGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort);
        Task<PagedResult<DispozitivMedicalDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort, int page, int pageSize);
    }

    public class DispozitivMedicalService : IDispozitivMedicalService
    {
        private readonly IDispozitivMedicalRepository _repository;

        public DispozitivMedicalService(IDispozitivMedicalRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DispozitivMedicalDTO>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PagedResult<DispozitivMedicalDTO>> GetPagedAsync(string? search, string? categorie, string? clasaRisc, int page, int pageSize, string? sort)
        {
            return await _repository.GetPagedAsync(search, categorie, clasaRisc, page, pageSize, sort);
        }

        public async Task<DispozitivMedicalDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(CreateDispozitivMedicalDTO dto)
        {
            return await _repository.CreateAsync(dto);
        }

        public async Task<bool> UpdateAsync(UpdateDispozitivMedicalDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<DispozitivMedicalDTO>> GetAllGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort)
        {
            return await _repository.GetAllGroupedAsync(search, categorie, clasaRisc, groupBy, sort);
        }

        public async Task<PagedResult<DispozitivMedicalDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort, int page, int pageSize)
        {
            return await _repository.GetPagedGroupedAsync(search, categorie, clasaRisc, groupBy, sort, page, pageSize);
        }
    }
}