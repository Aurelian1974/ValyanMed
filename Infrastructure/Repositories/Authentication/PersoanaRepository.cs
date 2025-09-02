using Dapper;
using Shared.Common;
using Shared.Enums;
using Shared.Models.Authentication;
using Shared.DTOs.Authentication;
using System.Data;
using Application.Services.Authentication;

namespace Infrastructure.Repositories.Authentication;

public class PersoanaRepository : IPersoanaRepository
{
    private readonly IDbConnection _connection;

    public PersoanaRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<int>> CreateAsync(Persoana persoana)
    {
        try
        {
            persoana.Guid = Guid.NewGuid();
            
            var parameters = new DynamicParameters();
            parameters.Add("@Guid", persoana.Guid);
            parameters.Add("@Nume", persoana.Nume);
            parameters.Add("@Prenume", persoana.Prenume);
            parameters.Add("@Judet", persoana.Judet);
            parameters.Add("@Localitate", persoana.Localitate);
            parameters.Add("@Strada", persoana.Strada);
            parameters.Add("@NumarStrada", persoana.NumarStrada);
            parameters.Add("@CodPostal", persoana.CodPostal);
            parameters.Add("@PozitieOrganizatie", persoana.PozitieOrganizatie);
            parameters.Add("@DataNasterii", persoana.DataNasterii);
            parameters.Add("@CNP", persoana.CNP);
            parameters.Add("@TipActIdentitate", persoana.TipActIdentitate?.ToString());
            parameters.Add("@SerieActIdentitate", persoana.SerieActIdentitate);
            parameters.Add("@NumarActIdentitate", persoana.NumarActIdentitate);
            parameters.Add("@StareCivila", persoana.StareCivila?.ToString());
            parameters.Add("@Gen", persoana.Gen?.ToString());
            parameters.Add("@Telefon", persoana.Telefon);
            parameters.Add("@Email", persoana.Email);
            parameters.Add("@EsteActiva", persoana.EsteActiva);

            var result = await _connection.QueryFirstAsync<int>("sp_CreatePersoana", parameters, commandType: CommandType.StoredProcedure);
            
            return Result<int>.Success(result, "Persoana a fost creata cu succes");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Eroare la crearea persoanei: {ex.Message}");
        }
    }

    public async Task<Result<Persoana?>> GetByIdAsync(int id)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var persoana = await _connection.QuerySingleOrDefaultAsync<dynamic>("sp_GetPersoanaById", parameters, commandType: CommandType.StoredProcedure);
            
            if (persoana == null)
            {
                return Result<Persoana?>.Success(null);
            }

            var result = MapDynamicToPersoana(persoana);
            return Result<Persoana?>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<Persoana?>.Failure($"Eroare la ob?inerea persoanei: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Persoana>>> GetAllAsync()
    {
        try
        {
            var persoane = await _connection.QueryAsync<dynamic>("sp_GetAllPersoane", commandType: CommandType.StoredProcedure);
            var result = persoane.Select(MapDynamicToPersoana);
            
            return Result<IEnumerable<Persoana>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Persoana>>.Failure($"Eroare la obtinerea tuturor persoanelor: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Persoana persoana)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", persoana.Id);
            parameters.Add("@Nume", persoana.Nume);
            parameters.Add("@Prenume", persoana.Prenume);
            parameters.Add("@Judet", persoana.Judet);
            parameters.Add("@Localitate", persoana.Localitate);
            parameters.Add("@Strada", persoana.Strada);
            parameters.Add("@NumarStrada", persoana.NumarStrada);
            parameters.Add("@CodPostal", persoana.CodPostal);
            parameters.Add("@PozitieOrganizatie", persoana.PozitieOrganizatie);
            parameters.Add("@DataNasterii", persoana.DataNasterii);
            parameters.Add("@CNP", persoana.CNP);
            parameters.Add("@TipActIdentitate", persoana.TipActIdentitate?.ToString());
            parameters.Add("@SerieActIdentitate", persoana.SerieActIdentitate);
            parameters.Add("@NumarActIdentitate", persoana.NumarActIdentitate);
            parameters.Add("@StareCivila", persoana.StareCivila?.ToString());
            parameters.Add("@Gen", persoana.Gen?.ToString());
            parameters.Add("@Telefon", persoana.Telefon);
            parameters.Add("@Email", persoana.Email);
            parameters.Add("@EsteActiva", persoana.EsteActiva);

            await _connection.ExecuteAsync("sp_UpdatePersoana", parameters, commandType: CommandType.StoredProcedure);
            
            return Result.Success("Persoana a fost actualizata cu succes");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la actualizarea persoanei: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await _connection.ExecuteAsync("sp_DeletePersoana", parameters, commandType: CommandType.StoredProcedure);
            
            return Result.Success("Persoana a fost stearsa cu succes");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la stergerea persoanei: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ExistsByCNPAsync(string cnp, int? excludeId = null)
    {
        try
        {
            // Don't check for empty or null CNP
            if (string.IsNullOrWhiteSpace(cnp))
            {
                return Result<bool>.Success(false);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@CNP", cnp);
            parameters.Add("@ExcludeId", excludeId);

            var result = await _connection.QueryFirstAsync<dynamic>("sp_CheckCNPExists", parameters, commandType: CommandType.StoredProcedure);
            var exists = (int)result.ExistsCNP == 1;
            
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Eroare la verificarea CNP-ului: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<PersoanaListDto>>> GetPagedAsync(PersoanaSearchQuery query)
    {
        try
        {
            // Use simple SQL query like PersonalMedical instead of complex stored procedure
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            // Build WHERE conditions
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                whereClause += " AND (Nume LIKE @Search OR Prenume LIKE @Search OR CONCAT(Nume, ' ', Prenume) LIKE @Search OR Email LIKE @Search OR Telefon LIKE @Search)";
                parameters.Add("@Search", $"%{query.Search}%");
            }

            if (!string.IsNullOrWhiteSpace(query.Judet))
            {
                whereClause += " AND Judet = @Judet";
                parameters.Add("@Judet", query.Judet);
            }

            if (!string.IsNullOrWhiteSpace(query.Localitate))
            {
                whereClause += " AND Localitate = @Localitate";
                parameters.Add("@Localitate", query.Localitate);
            }

            if (query.EsteActiv.HasValue)
            {
                whereClause += " AND EsteActiva = @EsteActiva";
                parameters.Add("@EsteActiva", query.EsteActiv.Value);
            }

            // Build ORDER BY with column mapping
            var orderBy = "ORDER BY Nume, Prenume";
            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                var mappedSort = MapSortColumns(query.Sort);
                orderBy = $"ORDER BY {mappedSort}";
            }

            // Calculate pagination
            var offset = (query.Page - 1) * query.PageSize;
            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", query.PageSize);

            // Count query
            var countSql = $@"
                SELECT COUNT(*) 
                FROM Persoane 
                {whereClause}";

            // Data query
            var dataSql = $@"
                SELECT 
                    Id,
                    Nume,
                    Prenume,
                    CONCAT(Nume, ' ', Prenume) as NumeComplet,
                    CNP,
                    DataNasterii,
                    CASE 
                        WHEN DataNasterii IS NOT NULL 
                        THEN DATEDIFF(YEAR, DataNasterii, GETDATE()) - 
                             CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, DataNasterii, GETDATE()), DataNasterii) > GETDATE() 
                                  THEN 1 ELSE 0 END
                        ELSE 0 
                    END as Varsta,
                    Gen,
                    Telefon,
                    Email,
                    Judet,
                    Localitate,
                    CASE 
                        WHEN Strada IS NOT NULL OR NumarStrada IS NOT NULL OR Localitate IS NOT NULL OR Judet IS NOT NULL
                        THEN CONCAT(
                            ISNULL(Strada + ' ', ''),
                            CASE WHEN NumarStrada IS NOT NULL THEN 'nr. ' + NumarStrada + ' ' ELSE '' END,
                            ISNULL(Localitate + ' ', ''),
                            CASE WHEN Judet IS NOT NULL THEN 'jud. ' + Judet ELSE '' END
                        )
                        ELSE NULL 
                    END as Adresa,
                    EsteActiva,
                    DataCreare
                FROM Persoane 
                {whereClause}
                {orderBy}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            // Execute queries
            var totalCount = await _connection.QueryFirstAsync<int>(countSql, parameters);
            var items = await _connection.QueryAsync<PersoanaListDto>(dataSql, parameters);

            var result = new PagedResult<PersoanaListDto>(
                items,
                totalCount,
                query.Page,
                query.PageSize);

            return Result<PagedResult<PersoanaListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<PersoanaListDto>>.Failure($"Eroare la obtinerea persoanelor paginate: {ex.Message}");
        }
    }

    private static Persoana MapDynamicToPersoana(dynamic item)
    {
        return new Persoana
        {
            Id = item.Id ?? 0,
            Guid = item.Guid ?? Guid.Empty,
            Nume = item.Nume ?? string.Empty,
            Prenume = item.Prenume ?? string.Empty,
            Judet = item.Judet,
            Localitate = item.Localitate,
            Strada = item.Strada,
            NumarStrada = item.NumarStrada,
            CodPostal = item.CodPostal,
            PozitieOrganizatie = item.PozitieOrganizatie,
            DataNasterii = item.DataNasterii,
            DataCreare = item.DataCreare,
            DataModificare = item.DataModificare,
            CNP = item.CNP,
            TipActIdentitate = ParseEnum<TipActIdentitate>(item.TipActIdentitate),
            SerieActIdentitate = item.SerieActIdentitate,
            NumarActIdentitate = item.NumarActIdentitate,
            StareCivila = ParseEnum<StareCivila>(item.StareCivila),
            Gen = ParseEnum<Gen>(item.Gen),
            Telefon = item.Telefon,
            Email = item.Email,
            EsteActiva = item.EsteActiva ?? true
        };
    }

    private static PersoanaListDto MapDynamicToPersoanaListDto(dynamic item)
    {
        var varsta = 0;
        if (item.DataNasterii != null)
        {
            var dataNasterii = (DateTime)item.DataNasterii;
            var today = DateTime.Today;
            varsta = today.Year - dataNasterii.Year;
            if (dataNasterii.Date > today.AddYears(-varsta))
                varsta--;
        }

        return new PersoanaListDto
        {
            Id = item.Id ?? 0,
            Nume = item.Nume ?? string.Empty,
            Prenume = item.Prenume ?? string.Empty,
            NumeComplet = $"{item.Nume ?? ""} {item.Prenume ?? ""}".Trim(),
            CNP = item.CNP,
            DataNasterii = item.DataNasterii,
            Varsta = varsta,
            Gen = item.Gen,
            Telefon = item.Telefon,
            Email = item.Email,
            Judet = item.Judet,
            Localitate = item.Localitate,
            Adresa = item.Adresa,
            EsteActiva = item.EsteActiva ?? true,
            DataCreare = item.DataCreare
        };
    }

    /// <summary>
    /// Safe enum parsing that handles legacy values and unknown enum values
    /// Updated to handle both short and long enum values for maximum compatibility
    /// </summary>
    private static T? ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (Enum.TryParse<T>(value, true, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Maps client column names to database column names for sorting
    /// Supports all columns except 'Actiuni' which is UI-only
    /// Based on the columns from GestionarePersoane.razor DataGrid
    /// </summary>
    private static string MapSortColumns(string sortExpression)
    {
        if (string.IsNullOrWhiteSpace(sortExpression))
            return "Nume, Prenume";

        // Split by comma for multiple sort columns
        var sortParts = sortExpression.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var mappedParts = new List<string>();

        foreach (var part in sortParts)
        {
            var trimmedPart = part.Trim();
            var sortPart = trimmedPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (sortPart.Length == 0) continue;

            var columnName = sortPart[0].ToLower();
            var direction = sortPart.Length > 1 ? sortPart[1].ToUpper() : "ASC";

            // Validate direction
            if (direction != "ASC" && direction != "DESC")
                direction = "ASC";

            // Map column names based on actual DataGrid columns from GestionarePersoane.razor:
            // NumeComplet, CNP, DataNasterii, Varsta, Gen, Telefon, Email, Judet, Localitate, EsteActiva, DataCreare, Actiuni
            var mappedColumn = columnName switch
            {
                // Basic name columns
                "nume" => "Nume",
                "prenume" => "Prenume", 
                "numecomplet" => "CONCAT(Nume, ' ', Prenume)",
                
                // Identity and personal info columns
                "cnp" => "CNP",
                "datanasterii" => "DataNasterii",
                "varsta" => "DATEDIFF(YEAR, DataNasterii, GETDATE())",
                "gen" => "Gen",
                
                // Contact columns
                "telefon" => "Telefon",
                "email" => "Email",
                
                // Location columns
                "judet" => "Judet",
                "localitate" => "Localitate",
                "adresa" => "CONCAT(ISNULL(Strada + ' ', ''), CASE WHEN NumarStrada IS NOT NULL THEN 'nr. ' + NumarStrada + ' ' ELSE '' END, ISNULL(Localitate + ' ', ''), CASE WHEN Judet IS NOT NULL THEN 'jud. ' + Judet ELSE '' END)",
                
                // Status columns - supporting multiple variations
                "esteactiva" => "EsteActiva",
                "status" => "EsteActiva",
                "activa" => "EsteActiva",
                "active" => "EsteActiva",
                
                // Date columns - supporting multiple variations
                "datacrearii" => "DataCreare",
                "datacreari" => "DataCreare",
                "datacreare" => "DataCreare",
                "creationdate" => "DataCreare",
                
                // UI-only columns that should be ignored (not sortable)
                "actiuni" => null,
                "actions" => null,
                
                // Default fallback for unknown columns
                _ => "Nume"
            };

            // Skip UI-only columns that return null
            if (mappedColumn != null)
            {
                mappedParts.Add($"{mappedColumn} {direction}");
            }
        }

        return mappedParts.Any() ? string.Join(", ", mappedParts) : "Nume, Prenume";
    }
}