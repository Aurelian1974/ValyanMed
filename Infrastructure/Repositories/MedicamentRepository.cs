using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Shared.DTOs;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class MedicamentRepository : IMedicamentRepository
    {
        private readonly IDbConnection _db;
        private bool? _spPagedAvailable;
        public MedicamentRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<MedicamentDTO>> GetAllAsync()
        {
            return await _db.QueryAsync<MedicamentDTO>("dbo.sp_Medicament_GetAll", commandType: CommandType.StoredProcedure);
        }

        public async Task<MedicamentDTO> GetByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<MedicamentDTO>(
                "dbo.sp_Medicament_GetById",
                new { MedicamentID = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(CreateMedicamentDTO dto)
        {
            var p = new DynamicParameters(dto);
            return await _db.ExecuteScalarAsync<int>("dbo.sp_Medicament_Create", p, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAsync(UpdateMedicamentDTO dto)
        {
            var p = new DynamicParameters(dto);
            var rows = await _db.ExecuteAsync("dbo.sp_Medicament_Update", p, commandType: CommandType.StoredProcedure);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync("dbo.sp_Medicament_Delete", new { MedicamentID = id }, commandType: CommandType.StoredProcedure);
            return rows > 0;
        }

        public async Task<PagedResult<MedicamentDTO>> GetPagedAsync(string? search, string? status, int page, int pageSize, string? sort, string? groupBy)
        {
            // Decide once if stored procedure exists
            if (!_spPagedAvailable.HasValue)
            {
                const string checkSql = "SELECT CASE WHEN OBJECT_ID('dbo.sp_Medicament_GetPaged','P') IS NULL THEN 0 ELSE 1 END";
                _spPagedAvailable = await _db.ExecuteScalarAsync<int>(checkSql) == 1;
            }

            if (_spPagedAvailable == true)
            {
                var result = await _db.QueryMultipleAsync(
                    "dbo.sp_Medicament_GetPaged",
                    new { Search = search, Status = status, Page = page, PageSize = pageSize, Sort = sort, GroupBy = groupBy },
                    commandType: CommandType.StoredProcedure);
                var items = (await result.ReadAsync<MedicamentDTO>()).AsList();
                var total = await result.ReadFirstAsync<int>();
                return new PagedResult<MedicamentDTO> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
            }
            else
            {
                var orderBy = BuildOrderBy(sort, groupBy);
                var offset = (page - 1) * pageSize;

                var sql = new StringBuilder();
                sql.AppendLine("SELECT MedicamentID, MedicamentGUID, Nume, DenumireComunaInternationala, Concentratie, FormaFarmaceutica, Producator, CodATC, Status, DataInregistrare, NumarAutorizatie, DataAutorizatie, DataExpirare, Ambalaj, Prospect, Contraindicatii, Interactiuni, Pret, PretProducator, TVA, Compensat, PrescriptieMedicala, Stoc, StocSiguranta, DataActualizare, UtilizatorActualizare, Observatii, Activ");
                sql.AppendLine("FROM dbo.Medicament WITH (NOLOCK)");
                sql.AppendLine("WHERE ( @Search IS NULL OR @Search = '' OR Nume LIKE '%' + @Search + '%' OR DenumireComunaInternationala LIKE '%' + @Search + '%' OR Producator LIKE '%' + @Search + '%' OR CodATC LIKE '%' + @Search + '%' )");
                sql.AppendLine("AND ( @Status IS NULL OR @Status = '' OR @Status = 'toate' OR Status = @Status )");
                sql.AppendLine($"ORDER BY {orderBy}");
                sql.AppendLine("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");
                sql.AppendLine("SELECT COUNT(1) FROM dbo.Medicament WITH (NOLOCK) WHERE ( @Search IS NULL OR @Search = '' OR Nume LIKE '%' + @Search + '%' OR DenumireComunaInternationala LIKE '%' + @Search + '%' OR Producator LIKE '%' + @Search + '%' OR CodATC LIKE '%' + @Search + '%' ) AND ( @Status IS NULL OR @Status = '' OR @Status = 'toate' OR Status = @Status );");

                using var multi = await _db.QueryMultipleAsync(sql.ToString(), new { Search = search, Status = status, Offset = offset, PageSize = pageSize });
                var list = (await multi.ReadAsync<MedicamentDTO>()).AsList();
                var total = await multi.ReadFirstAsync<int>();
                return new PagedResult<MedicamentDTO> { Items = list, TotalCount = total, Page = page, PageSize = pageSize };
            }
        }

        private static string BuildOrderBy(string? sort, string? groupBy)
        {
            var order = new List<string>();
            // handle grouping first
            if (!string.IsNullOrWhiteSpace(groupBy))
            {
                var groups = groupBy.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                foreach (var g in groups)
                {
                    var col = MapColumn(g);
                    if (!string.IsNullOrEmpty(col)) order.Add($"{col} ASC");
                }
            }
            // then sorts
            if (!string.IsNullOrWhiteSpace(sort))
            {
                var parts = sort.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                foreach (var p in parts)
                {
                    var kv = p.Split(':', System.StringSplitOptions.TrimEntries);
                    var col = MapColumn(kv[0]);
                    var dir = kv.Length > 1 && kv[1].Equals("desc", System.StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
                    if (!string.IsNullOrEmpty(col)) order.Add($"{col} {dir}");
                }
            }
            // Default to clustered key for fast paging if no explicit sort/group
            if (order.Count == 0) order.Add("MedicamentID ASC");
            return string.Join(", ", order);
        }

        private static string MapColumn(string name) => name switch
        {
            "MedicamentID" => "MedicamentID",
            "Nume" => "Nume",
            "DenumireComunaInternationala" => "DenumireComunaInternationala",
            "Concentratie" => "Concentratie",
            "FormaFarmaceutica" => "FormaFarmaceutica",
            "Producator" => "Producator",
            "CodATC" => "CodATC",
            "Status" => "Status",
            "Stoc" => "Stoc",
            "StocSiguranta" => "StocSiguranta",
            _ => string.Empty
        };
    }
}
