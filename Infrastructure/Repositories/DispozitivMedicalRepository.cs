using System.Data;
using Dapper;
using Shared.DTOs;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class DispozitivMedicalRepository : IDispozitivMedicalRepository
    {
        private readonly IDbConnection _db;

        public DispozitivMedicalRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<DispozitivMedicalDTO>> GetAllAsync()
        {
            return await _db.QueryAsync<DispozitivMedicalDTO>("dbo.sp_DispozitiveMedicale_GetAll", commandType: CommandType.StoredProcedure);
        }

        public async Task<DispozitivMedicalDTO?> GetByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<DispozitivMedicalDTO>(
                "dbo.sp_DispozitiveMedicale_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(CreateDispozitivMedicalDTO dto)
        {
            var parameters = new DynamicParameters(dto);
            return await _db.ExecuteScalarAsync<int>("dbo.sp_DispozitiveMedicale_Create", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAsync(UpdateDispozitivMedicalDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters(dto);
                var result = await _db.QueryFirstOrDefaultAsync<int?>("dbo.sp_DispozitiveMedicale_Update", parameters, commandType: CommandType.StoredProcedure);
                return result.HasValue && result.Value > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DispozitivMedicalRepository.UpdateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _db.QueryFirstOrDefaultAsync<int?>("dbo.sp_DispozitiveMedicale_Delete", new { Id = id }, commandType: CommandType.StoredProcedure);
            return result.HasValue && result.Value > 0;
        }

        public async Task<PagedResult<DispozitivMedicalDTO>> GetPagedAsync(string? search, string? categorie, string? clasaRisc, int page, int pageSize, string? sort)
        {
            var result = await _db.QueryMultipleAsync(
                "dbo.sp_DispozitiveMedicale_GetPaged",
                new { Search = search, Categorie = categorie, ClasaRisc = clasaRisc, Page = page, PageSize = pageSize, Sort = sort },
                commandType: CommandType.StoredProcedure);
            
            var items = (await result.ReadAsync<DispozitivMedicalDTO>()).AsList();
            var total = await result.ReadFirstAsync<int>();
            
            return new PagedResult<DispozitivMedicalDTO> 
            { 
                Items = items, 
                TotalCount = total, 
                Page = page, 
                PageSize = pageSize 
            };
        }

        public async Task<IEnumerable<DispozitivMedicalDTO>> GetAllGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort)
        {
            return await _db.QueryAsync<DispozitivMedicalDTO>(
                "dbo.sp_DispozitiveMedicale_GetAllGrouped",
                new { Search = search, Categorie = categorie, ClasaRisc = clasaRisc, GroupBy = groupBy, Sort = sort },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResult<DispozitivMedicalDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort, int page, int pageSize)
        {
            var result = await _db.QueryMultipleAsync(
                "dbo.sp_DispozitiveMedicale_GetPagedGrouped",
                new { Search = search, Categorie = categorie, ClasaRisc = clasaRisc, GroupBy = groupBy, Sort = sort, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure);
            
            var items = (await result.ReadAsync<DispozitivMedicalDTO>()).AsList();
            var total = await result.ReadFirstAsync<int>();
            
            return new PagedResult<DispozitivMedicalDTO> 
            { 
                Items = items, 
                TotalCount = total, 
                Page = page, 
                PageSize = pageSize 
            };
        }
    }
}