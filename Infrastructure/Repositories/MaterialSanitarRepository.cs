using System.Data;
using Dapper;
using Shared.DTOs;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class MaterialSanitarRepository : IMaterialSanitarRepository
    {
        private readonly IDbConnection _db;

        public MaterialSanitarRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<MaterialSanitarDTO>> GetAllAsync()
        {
            return await _db.QueryAsync<MaterialSanitarDTO>("dbo.sp_MaterialeSanitare_GetAll", commandType: CommandType.StoredProcedure);
        }

        public async Task<MaterialSanitarDTO?> GetByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<MaterialSanitarDTO>(
                "dbo.sp_MaterialeSanitare_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(CreateMaterialSanitarDTO dto)
        {
            var parameters = new DynamicParameters(dto);
            return await _db.ExecuteScalarAsync<int>("dbo.sp_MaterialeSanitare_Create", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAsync(UpdateMaterialSanitarDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters(dto);
                var result = await _db.QueryFirstOrDefaultAsync<int?>("dbo.sp_MaterialeSanitare_Update", parameters, commandType: CommandType.StoredProcedure);
                return result.HasValue && result.Value > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MaterialSanitarRepository.UpdateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _db.QueryFirstOrDefaultAsync<int?>("dbo.sp_MaterialeSanitare_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            return result.HasValue && result.Value > 0;
        }

        public async Task<PagedResult<MaterialSanitarDTO>> GetPagedAsync(string? search, string? categorie, int page, int pageSize, string? sort)
        {
            var result = await _db.QueryMultipleAsync(
                "dbo.sp_MaterialeSanitare_GetPaged",
                new { Search = search, Categorie = categorie, Page = page, PageSize = pageSize, Sort = sort },
                commandType: CommandType.StoredProcedure);
            
            var items = (await result.ReadAsync<MaterialSanitarDTO>()).AsList();
            var total = await result.ReadFirstAsync<int>();
            
            return new PagedResult<MaterialSanitarDTO> 
            { 
                Items = items, 
                TotalCount = total, 
                Page = page, 
                PageSize = pageSize 
            };
        }

        public async Task<IEnumerable<MaterialSanitarDTO>> GetAllGroupedAsync(string? search, string? categorie, string? groupBy, string? sort)
        {
            return await _db.QueryAsync<MaterialSanitarDTO>(
                "dbo.sp_MaterialeSanitare_GetAllGrouped",
                new { Search = search, Categorie = categorie, GroupBy = groupBy, Sort = sort },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResult<MaterialSanitarDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? groupBy, string? sort, int page, int pageSize)
        {
            var result = await _db.QueryMultipleAsync(
                "dbo.sp_MaterialeSanitare_GetPagedGrouped",
                new { Search = search, Categorie = categorie, GroupBy = groupBy, Sort = sort, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure);
            
            var items = (await result.ReadAsync<MaterialSanitarDTO>()).AsList();
            var total = await result.ReadFirstAsync<int>();
            
            return new PagedResult<MaterialSanitarDTO> 
            { 
                Items = items, 
                TotalCount = total, 
                Page = page, 
                PageSize = pageSize 
            };
        }
    }
}