using Core.Interfaces;
using Shared.DTOs;

namespace Application.Services
{
    public interface IMaterialSanitarService
    {
        Task<IEnumerable<MaterialSanitarDTO>> GetAllAsync();
        Task<PagedResult<MaterialSanitarDTO>> GetPagedAsync(string? search, string? categorie, int page, int pageSize, string? sort);
        Task<MaterialSanitarDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateMaterialSanitarDTO dto);
        Task<bool> UpdateAsync(UpdateMaterialSanitarDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<MaterialSanitarDTO>> GetAllGroupedAsync(string? search, string? categorie, string? groupBy, string? sort);
        Task<PagedResult<MaterialSanitarDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? groupBy, string? sort, int page, int pageSize);
    }

    public class MaterialSanitarService : IMaterialSanitarService
    {
        private readonly IMaterialSanitarRepository _repository;

        public MaterialSanitarService(IMaterialSanitarRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MaterialSanitarDTO>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PagedResult<MaterialSanitarDTO>> GetPagedAsync(string? search, string? categorie, int page, int pageSize, string? sort)
        {
            return await _repository.GetPagedAsync(search, categorie, page, pageSize, sort);
        }

        public async Task<MaterialSanitarDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(CreateMaterialSanitarDTO dto)
        {
            return await _repository.CreateAsync(dto);
        }

        public async Task<bool> UpdateAsync(UpdateMaterialSanitarDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<MaterialSanitarDTO>> GetAllGroupedAsync(string? search, string? categorie, string? groupBy, string? sort)
        {
            return await _repository.GetAllGroupedAsync(search, categorie, groupBy, sort);
        }

        public async Task<PagedResult<MaterialSanitarDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? groupBy, string? sort, int page, int pageSize)
        {
            return await _repository.GetPagedGroupedAsync(search, categorie, groupBy, sort, page, pageSize);
        }
    }
}