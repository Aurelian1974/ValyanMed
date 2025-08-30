using Dapper;
using Shared.Common;
using Shared.DTOs.Medical;
using Application.Services.Medical;
using Infrastructure.Data;

namespace Infrastructure.Repositories.Medical;

public class PersonalMedicalRepository : IPersonalMedicalRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public PersonalMedicalRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<PagedResult<PersonalMedicalListDto>>> GetPagedAsync(PersonalMedicalSearchQuery query)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Build ORDER BY clause cu suport pentru grupare
            var orderBy = BuildOrderByClause(query.Sort);
            
            Console.WriteLine($"Repository: GetPagedAsync called");
            Console.WriteLine($"Sort parameter: {query.Sort}");
            Console.WriteLine($"Generated ORDER BY: {orderBy}");

            var offset = (query.Page - 1) * query.PageSize;

            var sql = $@"
SELECT PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare
FROM PersonalMedical WITH (NOLOCK)
WHERE 1=1
  AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv)
  AND (@Departament IS NULL OR @Departament = '' OR @Departament = 'toate' OR UPPER(Departament) LIKE '%' + UPPER(@Departament) + '%')
  AND (@Pozitie IS NULL OR @Pozitie = '' OR @Pozitie = 'toate' OR UPPER(Pozitie) LIKE '%' + UPPER(@Pozitie) + '%')
  AND (@Nume IS NULL OR @Nume = '' OR UPPER(Nume) LIKE '%' + UPPER(@Nume) + '%')
  AND (@Prenume IS NULL OR @Prenume = '' OR UPPER(Prenume) LIKE '%' + UPPER(@Prenume) + '%')
  AND (@Specializare IS NULL OR @Specializare = '' OR UPPER(Specializare) LIKE '%' + UPPER(@Specializare) + '%')
  AND (@NumarLicenta IS NULL OR @NumarLicenta = '' OR UPPER(NumarLicenta) LIKE '%' + UPPER(@NumarLicenta) + '%')
  AND (@Telefon IS NULL OR @Telefon = '' OR Telefon LIKE '%' + @Telefon + '%')
  AND (@Email IS NULL OR @Email = '' OR UPPER(Email) LIKE '%' + UPPER(@Email) + '%')
  AND ( @Search IS NULL OR @Search = ''
        OR UPPER(Nume) LIKE '%' + UPPER(@Search) + '%'
        OR UPPER(Prenume) LIKE '%' + UPPER(@Search) + '%'
        OR UPPER(CONCAT(Nume, ' ', Prenume)) LIKE '%' + UPPER(@Search) + '%'
        OR (Specializare IS NOT NULL AND UPPER(Specializare) LIKE '%' + UPPER(@Search) + '%')
        OR (NumarLicenta IS NOT NULL AND UPPER(NumarLicenta) LIKE '%' + UPPER(@Search) + '%')
        OR (Departament IS NOT NULL AND UPPER(Departament) LIKE '%' + UPPER(@Search) + '%')
        OR (Pozitie IS NOT NULL AND UPPER(Pozitie) LIKE '%' + UPPER(@Search) + '%')
        OR (Telefon IS NOT NULL AND Telefon LIKE '%' + @Search + '%')
        OR (Email IS NOT NULL AND UPPER(Email) LIKE '%' + UPPER(@Search) + '%') )
ORDER BY {orderBy}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

SELECT COUNT(*)
FROM PersonalMedical WITH (NOLOCK)
WHERE 1=1
  AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv)
  AND (@Departament IS NULL OR @Departament = '' OR @Departament = 'toate' OR UPPER(Departament) LIKE '%' + UPPER(@Departament) + '%')
  AND (@Pozitie IS NULL OR @Pozitie = '' OR @Pozitie = 'toate' OR UPPER(Pozitie) LIKE '%' + UPPER(@Pozitie) + '%')
  AND (@Nume IS NULL OR @Nume = '' OR UPPER(Nume) LIKE '%' + UPPER(@Nume) + '%')
  AND (@Prenume IS NULL OR @Prenume = '' OR UPPER(Prenume) LIKE '%' + UPPER(@Prenume) + '%')
  AND (@Specializare IS NULL OR @Specializare = '' OR UPPER(Specializare) LIKE '%' + UPPER(@Specializare) + '%')
  AND (@NumarLicenta IS NULL OR @NumarLicenta = '' OR UPPER(NumarLicenta) LIKE '%' + UPPER(@NumarLicenta) + '%')
  AND (@Telefon IS NULL OR @Telefon = '' OR Telefon LIKE '%' + @Telefon + '%')
  AND (@Email IS NULL OR @Email = '' OR UPPER(Email) LIKE '%' + UPPER(@Email) + '%')
  AND ( @Search IS NULL OR @Search = ''
        OR UPPER(Nume) LIKE '%' + UPPER(@Search) + '%'
        OR UPPER(Prenume) LIKE '%' + UPPER(@Search) + '%'
        OR UPPER(CONCAT(Nume, ' ', Prenume)) LIKE '%' + UPPER(@Search) + '%'
        OR (Specializare IS NOT NULL AND UPPER(Specializare) LIKE '%' + UPPER(@Search) + '%')
        OR (NumarLicenta IS NOT NULL AND UPPER(NumarLicenta) LIKE '%' + UPPER(@Search) + '%')
        OR (Departament IS NOT NULL AND UPPER(Departament) LIKE '%' + UPPER(@Search) + '%')
        OR (Pozitie IS NOT NULL AND UPPER(Pozitie) LIKE '%' + UPPER(@Search) + '%')
        OR (Telefon IS NOT NULL AND Telefon LIKE '%' + @Search + '%')
        OR (Email IS NOT NULL AND UPPER(Email) LIKE '%' + UPPER(@Email) + '%') );";

            var parameters = new DynamicParameters();
            BuildSearchParameters(parameters, query);
            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", query.PageSize);

            using var multi = await connection.QueryMultipleAsync(sql, parameters);
            
            var items = await multi.ReadAsync<PersonalMedicalDto>();
            var totalCount = await multi.ReadSingleAsync<int>();

            var listItems = items.Select(MapToListDto).ToList();
            
            Console.WriteLine($"Repository: Retrieved {listItems.Count} items, total: {totalCount}");

            return Result<PagedResult<PersonalMedicalListDto>>.Success(
                new PagedResult<PersonalMedicalListDto>
                {
                    Items = listItems,
                    TotalCount = totalCount
                });
        }
        catch (Exception ex)
        {
            return Result<PagedResult<PersonalMedicalListDto>>.Failure($"Eroare la incarcarea datelor: {ex.Message}");
        }
    }

    private string BuildOrderByClause(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return "Nume ASC, Prenume ASC";

        var orderByParts = new List<string>();
        var sortParts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in sortParts)
        {
            var trimmedPart = part.Trim();
            var colonIndex = trimmedPart.IndexOf(':');
            
            string column, direction;
            if (colonIndex > 0)
            {
                column = trimmedPart.Substring(0, colonIndex).Trim();
                direction = trimmedPart.Substring(colonIndex + 1).Trim().ToUpper();
                direction = direction == "DESC" ? "DESC" : "ASC";
            }
            else
            {
                column = trimmedPart;
                direction = "ASC";
            }

            var validColumn = GetValidColumnName(column);
            if (!string.IsNullOrEmpty(validColumn))
            {
                orderByParts.Add($"{validColumn} {direction}");
            }
        }

        if (!orderByParts.Any())
            return "Nume ASC, Prenume ASC";

        var result = string.Join(", ", orderByParts);
        Console.WriteLine($"BuildOrderByClause: '{sort}' -> '{result}'");
        return result;
    }

    private string? GetValidColumnName(string column)
    {
        return column?.ToLower() switch
        {
            "nume" => "Nume",
            "prenume" => "Prenume", 
            "numecomplet" => "Nume", 
            "pozitie" => "Pozitie",
            "departament" => "Departament",
            "specializare" => "Specializare",
            "numarlicenta" => "NumarLicenta",
            "telefon" => "Telefon",
            "email" => "Email",
            "esteactiv" => "EsteActiv",
            "datacreare" => "DataCreare",
            _ => null
        };
    }

    private void BuildSearchParameters(DynamicParameters parameters, PersonalMedicalSearchQuery query)
    {
        parameters.Add("@Search", query.Search);
        parameters.Add("@Departament", query.Departament);
        parameters.Add("@Pozitie", query.Pozitie);
        parameters.Add("@EsteActiv", query.EsteActiv);
        parameters.Add("@Nume", query.Nume);
        parameters.Add("@Prenume", query.Prenume);
        parameters.Add("@Specializare", query.Specializare);
        parameters.Add("@NumarLicenta", query.NumarLicenta);
        parameters.Add("@Telefon", query.Telefon);
        parameters.Add("@Email", query.Email);
    }

    private PersonalMedicalListDto MapToListDto(PersonalMedicalDto dto)
    {
        return new PersonalMedicalListDto
        {
            PersonalID = dto.PersonalID,
            Nume = dto.Nume,
            Prenume = dto.Prenume,
            Specializare = dto.Specializare,
            NumarLicenta = dto.NumarLicenta,
            Telefon = dto.Telefon,
            Email = dto.Email,
            Departament = dto.Departament,
            Pozitie = dto.Pozitie,
            EsteActiv = dto.EsteActiv,
            DataCreare = dto.DataCreare
        };
    }

    // Implement?ri pentru metodele din interfa??
    public async Task<Result<PersonalMedicalDataGridResult>> GetDataGridAsync(PersonalMedicalDataGridRequest request)
    {
        try
        {
            // For now, redirect to GetPagedAsync since DataGrid functionality is not fully implemented
            var searchQuery = new PersonalMedicalSearchQuery
            {
                Search = request.Search,
                Departament = request.Departament,
                Pozitie = request.Pozitie,
                EsteActiv = request.EsteActiv,
                Nume = request.Nume,
                Prenume = request.Prenume,
                Specializare = request.Specializare,
                NumarLicenta = request.NumarLicenta,
                Telefon = request.Telefon,
                Email = request.Email,
                Page = (request.Skip / request.Take) + 1,
                PageSize = request.Take,
                Sort = request.Sort
            };

            var pagedResult = await GetPagedAsync(searchQuery);
            
            if (!pagedResult.IsSuccess)
            {
                return Result<PersonalMedicalDataGridResult>.Failure(pagedResult.Errors);
            }

            var gridResult = new PersonalMedicalDataGridResult
            {
                Data = pagedResult.Value!.Items,
                Count = pagedResult.Value.Items.Count(),
                TotalItemsOverall = pagedResult.Value.TotalCount,
                TotalItems = pagedResult.Value.TotalCount,
                IsGrouped = request.Groups?.Any() == true,
                Groups = new List<DataGridGroupResult<PersonalMedicalListDto>>(),
                CurrentPage = searchQuery.Page,
                TotalPages = (int)Math.Ceiling((double)pagedResult.Value.TotalCount / request.Take),
                TotalGroups = 0
            };

            return Result<PersonalMedicalDataGridResult>.Success(gridResult);
        }
        catch (Exception ex)
        {
            return Result<PersonalMedicalDataGridResult>.Failure($"Eroare la incarcarea datelor pentru DataGrid: {ex.Message}");
        }
    }

    public async Task<Result<PersonalMedicalDetailDto?>> GetByIdAsync(Guid personalId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
SELECT PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare
FROM PersonalMedical WITH (NOLOCK)
WHERE PersonalID = @PersonalID";

            var dto = await connection.QuerySingleOrDefaultAsync<PersonalMedicalDto>(sql, new { PersonalID = personalId });
            
            if (dto == null)
            {
                return Result<PersonalMedicalDetailDto?>.Success(null);
            }

            var detailDto = new PersonalMedicalDetailDto
            {
                PersonalID = dto.PersonalID,
                Nume = dto.Nume,
                Prenume = dto.Prenume,
                Specializare = dto.Specializare,
                NumarLicenta = dto.NumarLicenta,
                Telefon = dto.Telefon,
                Email = dto.Email,
                Departament = dto.Departament,
                Pozitie = dto.Pozitie,
                EsteActiv = dto.EsteActiv,
                DataCreare = dto.DataCreare,
                ProgramariRecente = new List<ProgramareListDto>(),
                ConsultatiiRecente = new List<ConsultatieListDto>(),
                TotalProgramari = 0,
                TotalConsultatii = 0
            };

            return Result<PersonalMedicalDetailDto?>.Success(detailDto);
        }
        catch (Exception ex)
        {
            return Result<PersonalMedicalDetailDto?>.Failure($"Eroare la gasirea personalului medical: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> CreateAsync(CreatePersonalMedicalRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var personalId = Guid.NewGuid();
            
            var sql = @"
INSERT INTO PersonalMedical (PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare)
VALUES (@PersonalID, @Nume, @Prenume, @Specializare, @NumarLicenta, @Telefon, @Email, @Departament, @Pozitie, @EsteActiv, @DataCreare)";

            await connection.ExecuteAsync(sql, new
            {
                PersonalID = personalId,
                Nume = request.Nume,
                Prenume = request.Prenume,
                Specializare = request.Specializare,
                NumarLicenta = request.NumarLicenta,
                Telefon = request.Telefon,
                Email = request.Email,
                Departament = request.Departament,
                Pozitie = request.Pozitie,
                EsteActiv = request.EsteActiv,
                DataCreare = DateTime.UtcNow
            });

            return Result<Guid>.Success(personalId);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Eroare la crearea personalului medical: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdatePersonalMedicalRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
UPDATE PersonalMedical 
SET Nume = @Nume, 
    Prenume = @Prenume, 
    Specializare = @Specializare, 
    NumarLicenta = @NumarLicenta, 
    Telefon = @Telefon, 
    Email = @Email, 
    Departament = @Departament, 
    Pozitie = @Pozitie, 
    EsteActiv = @EsteActiv
WHERE PersonalID = @PersonalID";

            var rowsAffected = await connection.ExecuteAsync(sql, request);
            
            if (rowsAffected == 0)
            {
                return Result.Failure("Personal medical nu a fost gasit pentru actualizare");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la actualizarea personalului medical: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid personalId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Soft delete - set EsteActiv = false
            var sql = @"
UPDATE PersonalMedical 
SET EsteActiv = 0
WHERE PersonalID = @PersonalID";

            var rowsAffected = await connection.ExecuteAsync(sql, new { PersonalID = personalId });
            
            if (rowsAffected == 0)
            {
                return Result.Failure("Personal medical nu a fost gasit pentru stergere");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la stergerea personalului medical: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PersonalMedicalListDto>>> GetActiveDoctorsAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
SELECT PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare
FROM PersonalMedical WITH (NOLOCK)
WHERE EsteActiv = 1 
  AND (Pozitie LIKE '%Doctor%' OR Pozitie LIKE '%Medic%')
ORDER BY Nume, Prenume";

            var items = await connection.QueryAsync<PersonalMedicalDto>(sql);
            var listItems = items.Select(MapToListDto).ToList();

            return Result<IEnumerable<PersonalMedicalListDto>>.Success(listItems);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PersonalMedicalListDto>>.Failure($"Eroare la gasirea doctorilor activi: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PersonalMedicalGroupAggregateDto>>> GetGroupAggregatesAsync(PersonalMedicalGroupAggregateQuery query)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var groupByColumn = GetValidColumnName(query.GroupBy) ?? "Departament";
            
            var sql = $@"
SELECT 
    ISNULL({groupByColumn}, 'Nu este specificat') as [Key],
    COUNT(*) as Count,
    MAX(DataCreare) as LastDataCreare
FROM PersonalMedical WITH (NOLOCK)
WHERE 1=1
  AND (@EsteActiv IS NULL OR EsteActiv = @EsteActiv)
  AND (@Departament IS NULL OR @Departament = '' OR UPPER(Departament) LIKE '%' + UPPER(@Departament) + '%')
  AND (@Pozitie IS NULL OR @Pozitie = '' OR UPPER(Pozitie) LIKE '%' + UPPER(@Pozitie) + '%')
  AND (@Search IS NULL OR @Search = '' OR 
       UPPER(Nume) LIKE '%' + UPPER(@Search) + '%' OR 
       UPPER(Prenume) LIKE '%' + UPPER(@Search) + '%')
GROUP BY {groupByColumn}
ORDER BY Count DESC";

            var parameters = new DynamicParameters();
            parameters.Add("@Search", query.Search);
            parameters.Add("@Departament", query.Departament);
            parameters.Add("@Pozitie", query.Pozitie);
            parameters.Add("@EsteActiv", query.EsteActiv);

            var items = await connection.QueryAsync<PersonalMedicalGroupAggregateDto>(sql, parameters);

            return Result<IEnumerable<PersonalMedicalGroupAggregateDto>>.Success(items);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PersonalMedicalGroupAggregateDto>>.Failure($"Eroare la agregarea grupurilor: {ex.Message}");
        }
    }
}

internal class PersonalMedicalDto
{
    public Guid PersonalID { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Departament { get; set; }
    public string Pozitie { get; set; } = string.Empty;
    public bool EsteActiv { get; set; }
    public DateTime DataCreare { get; set; }
}