using System.Data;
using Dapper;
using Shared.DTOs;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class PartenerRepository : IPartenerRepository
    {
        private readonly IDbConnection _db;
        private bool? _spPagedAvailable;

        public PartenerRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<PartenerDTO>> GetAllAsync()
        {
            return await _db.QueryAsync<PartenerDTO>("dbo.sp_Partener_GetAll", commandType: CommandType.StoredProcedure);
        }

        public async Task<PartenerDTO?> GetByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<PartenerDTO>(
                "dbo.sp_Partener_GetById",
                new { PartenerId = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(CreatePartenerDTO dto)
        {
            var parameters = new DynamicParameters(dto);
            return await _db.ExecuteScalarAsync<int>("dbo.sp_Partener_Create", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAsync(UpdatePartenerDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters(dto);
                var result = await _db.QueryFirstOrDefaultAsync<int?>("dbo.sp_Partener_Update", parameters, commandType: CommandType.StoredProcedure);
                return result.HasValue && result.Value > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PartenerRepository.UpdateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _db.QueryFirstOrDefaultAsync<int?>("dbo.sp_Partener_Delete", new { PartenerId = id }, commandType: CommandType.StoredProcedure);
            return result.HasValue && result.Value > 0;
        }

        public async Task<PagedResult<PartenerDTO>> GetPagedAsync(string? search, string? judet, int page, int pageSize, string? sort)
        {
            // Check if stored procedure exists
            if (!_spPagedAvailable.HasValue)
            {
                const string checkSql = "SELECT CASE WHEN OBJECT_ID('dbo.sp_Partener_GetPaged','P') IS NULL THEN 0 ELSE 1 END";
                _spPagedAvailable = await _db.ExecuteScalarAsync<int>(checkSql) == 1;
            }

            if (_spPagedAvailable == true)
            {
                // Use stored procedure
                var result = await _db.QueryMultipleAsync(
                    "dbo.sp_Partener_GetPaged",
                    new { Search = search, Judet = judet, Page = page, PageSize = pageSize, Sort = sort },
                    commandType: CommandType.StoredProcedure);
                
                var items = (await result.ReadAsync<PartenerDTO>()).AsList();
                var total = await result.ReadFirstAsync<int>();
                
                return new PagedResult<PartenerDTO> 
                { 
                    Items = items, 
                    TotalCount = total, 
                    Page = page, 
                    PageSize = pageSize 
                };
            }
            else
            {
                // Fallback to direct SQL
                var offset = (page - 1) * pageSize;
                var orderBy = BuildOrderBy(sort);

                var sql = $@"
                    SELECT PartenerId, PartenerGuid, CodIntern, Denumire, CodFiscal, Judet, Localitate, Adresa, 
                           DataCreare, DataActualizare, UtilizatorCreare, UtilizatorActualizare, Activ
                    FROM dbo.Partener WITH (NOLOCK)
                    WHERE Activ = 1
                      AND (@Search IS NULL OR @Search = '' OR 
                           UPPER(CodIntern) LIKE '%' + UPPER(@Search) + '%' OR 
                           UPPER(Denumire) LIKE '%' + UPPER(@Search) + '%' OR 
                           UPPER(CodFiscal) LIKE '%' + UPPER(@Search) + '%' OR
                           UPPER(Localitate) LIKE '%' + UPPER(@Search) + '%')
                      AND (@Judet IS NULL OR @Judet = '' OR @Judet = 'toate' OR Judet = @Judet)
                    ORDER BY {orderBy}
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    SELECT COUNT(1)
                    FROM dbo.Partener WITH (NOLOCK)
                    WHERE Activ = 1
                      AND (@Search IS NULL OR @Search = '' OR 
                           UPPER(CodIntern) LIKE '%' + UPPER(@Search) + '%' OR 
                           UPPER(Denumire) LIKE '%' + UPPER(@Search) + '%' OR 
                           UPPER(CodFiscal) LIKE '%' + UPPER(@Search) + '%' OR
                           UPPER(Localitate) LIKE '%' + UPPER(@Search) + '%')
                      AND (@Judet IS NULL OR @Judet = '' OR @Judet = 'toate' OR Judet = @Judet);";

                using var multi = await _db.QueryMultipleAsync(sql, new { Search = search, Judet = judet, Offset = offset, PageSize = pageSize });
                var list = (await multi.ReadAsync<PartenerDTO>()).AsList();
                var total = await multi.ReadFirstAsync<int>();
                
                return new PagedResult<PartenerDTO> 
                { 
                    Items = list, 
                    TotalCount = total, 
                    Page = page, 
                    PageSize = pageSize 
                };
            }
        }

        private static string BuildOrderBy(string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return "Denumire ASC";

            var orderClauses = new List<string>();
            var parts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            foreach (var part in parts)
            {
                var kv = part.Split(':', StringSplitOptions.TrimEntries);
                var column = MapColumn(kv[0]);
                var direction = kv.Length > 1 && kv[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
                
                if (!string.IsNullOrEmpty(column))
                    orderClauses.Add($"{column} {direction}");
            }

            return orderClauses.Count > 0 ? string.Join(", ", orderClauses) : "Denumire ASC";
        }

        private static string MapColumn(string name) => name switch
        {
            "PartenerId" => "PartenerId",
            "CodIntern" => "CodIntern",
            "Denumire" => "Denumire",
            "CodFiscal" => "CodFiscal",
            "Judet" => "Judet",
            "Localitate" => "Localitate",
            "DataCreare" => "DataCreare",
            "DataActualizare" => "DataActualizare",
            _ => string.Empty
        };
    }
}