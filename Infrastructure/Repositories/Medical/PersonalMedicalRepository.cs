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
            
            Console.WriteLine($"Repository: GetPagedAsync called");
            Console.WriteLine($"Sort parameter: {query.Sort}");

            // Folosim stored procedure actualizat? cu suport pentru ierarhie
            var parameters = new DynamicParameters();
            parameters.Add("@Search", query.Search);
            parameters.Add("@CategorieID", query.CategorieID);
            parameters.Add("@SpecializareID", query.SpecializareID);
            parameters.Add("@SubspecializareID", query.SubspecializareID);
            parameters.Add("@Departament", query.Departament); // Pentru compatibilitate cu datele vechi
            parameters.Add("@Pozitie", query.Pozitie);
            parameters.Add("@EsteActiv", query.EsteActiv);
            parameters.Add("@Nume", query.Nume);
            parameters.Add("@Prenume", query.Prenume);
            parameters.Add("@Specializare", query.Specializare); // Pentru compatibilitate cu datele vechi
            parameters.Add("@NumarLicenta", query.NumarLicenta);
            parameters.Add("@Telefon", query.Telefon);
            parameters.Add("@Email", query.Email);
            parameters.Add("@Page", query.Page);
            parameters.Add("@PageSize", query.PageSize);
            parameters.Add("@Sort", query.Sort);

            using var multi = await connection.QueryMultipleAsync(
                "sp_PersonalMedical_GetPaged", 
                parameters, 
                commandType: System.Data.CommandType.StoredProcedure);
            
            var items = await multi.ReadAsync<PersonalMedicalListDtoRaw>();
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

    private PersonalMedicalListDto MapToListDto(PersonalMedicalListDtoRaw raw)
    {
        return new PersonalMedicalListDto
        {
            PersonalID = raw.PersonalID,
            Nume = raw.Nume,
            Prenume = raw.Prenume,
            Specializare = raw.Specializare,
            NumarLicenta = raw.NumarLicenta,
            Telefon = raw.Telefon,
            Email = raw.Email,
            Departament = raw.Departament,
            Pozitie = raw.Pozitie,
            EsteActiv = raw.EsteActiv,
            DataCreare = raw.DataCreare,
            // Noi propriet??i ierarhice
            CategorieID = raw.CategorieID,
            SpecializareID = raw.SpecializareID,
            SubspecializareID = raw.SubspecializareID,
            CategorieNume = raw.CategorieNume,
            SpecializareNume = raw.SpecializareNume,
            SubspecializareNume = raw.SubspecializareNume
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
                CategorieID = request.CategorieID,
                SpecializareID = request.SpecializareID,
                SubspecializareID = request.SubspecializareID,
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
            
            var rawData = await connection.QuerySingleOrDefaultAsync<PersonalMedicalDetailDtoRaw>(
                "sp_PersonalMedical_GetById", 
                new { PersonalID = personalId },
                commandType: System.Data.CommandType.StoredProcedure);
            
            if (rawData == null)
            {
                return Result<PersonalMedicalDetailDto?>.Success(null);
            }

            var detailDto = new PersonalMedicalDetailDto
            {
                PersonalID = rawData.PersonalID,
                Nume = rawData.Nume,
                Prenume = rawData.Prenume,
                Specializare = rawData.Specializare,
                NumarLicenta = rawData.NumarLicenta,
                Telefon = rawData.Telefon,
                Email = rawData.Email,
                Departament = rawData.Departament,
                Pozitie = rawData.Pozitie,
                EsteActiv = rawData.EsteActiv,
                DataCreare = rawData.DataCreare,
                // Noi propriet??i ierarhice
                CategorieID = rawData.CategorieID,
                SpecializareID = rawData.SpecializareID,
                SubspecializareID = rawData.SubspecializareID,
                CategorieNume = rawData.CategorieNume,
                SpecializareNume = rawData.SpecializareNume,
                SubspecializareNume = rawData.SubspecializareNume,
                // Date adi?ionale pentru pagina de detalii
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
            
            Console.WriteLine($"[PersonalMedicalRepository] Creating personal with data: {request.Nume} {request.Prenume}");
            
            var parameters = new DynamicParameters();
            parameters.Add("@Nume", request.Nume);
            parameters.Add("@Prenume", request.Prenume);
            parameters.Add("@CategorieID", request.CategorieID);
            parameters.Add("@SpecializareID", request.SpecializareID);
            parameters.Add("@SubspecializareID", request.SubspecializareID);
            parameters.Add("@NumarLicenta", request.NumarLicenta);
            parameters.Add("@Telefon", request.Telefon);
            parameters.Add("@Email", request.Email);
            parameters.Add("@Pozitie", request.Pozitie);
            parameters.Add("@EsteActiv", request.EsteActiv);
            // Pentru compatibilitate cu datele vechi
            parameters.Add("@Departament", request.Departament);
            parameters.Add("@Specializare", request.Specializare);

            var result = await connection.QuerySingleAsync<PersonalCreateResult>(
                "sp_PersonalMedical_Create",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            Console.WriteLine($"[PersonalMedicalRepository] Success! Created personal with ID: {result.PersonalID}");
            return Result<Guid>.Success(result.PersonalID);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PersonalMedicalRepository] Exception: {ex.Message}");
            return Result<Guid>.Failure($"Eroare la crearea personalului medical: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(UpdatePersonalMedicalRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var parameters = new DynamicParameters();
            parameters.Add("@PersonalID", request.PersonalID);
            parameters.Add("@Nume", request.Nume);
            parameters.Add("@Prenume", request.Prenume);
            parameters.Add("@CategorieID", request.CategorieID);
            parameters.Add("@SpecializareID", request.SpecializareID);
            parameters.Add("@SubspecializareID", request.SubspecializareID);
            parameters.Add("@NumarLicenta", request.NumarLicenta);
            parameters.Add("@Telefon", request.Telefon);
            parameters.Add("@Email", request.Email);
            parameters.Add("@Pozitie", request.Pozitie);
            parameters.Add("@EsteActiv", request.EsteActiv);
            // Pentru compatibilitate cu datele vechi
            parameters.Add("@Departament", request.Departament);
            parameters.Add("@Specializare", request.Specializare);

            await connection.ExecuteAsync(
                "sp_PersonalMedical_Update",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

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
SELECT PersonalID, Nume, Prenume, Specializare, NumarLicenta, Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare,
       CategorieID, SpecializareID, SubspecializareID, CategorieNume, SpecializareNume, SubspecializareNume
FROM PersonalMedical pm WITH (NOLOCK)
LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = 'Categorie'
LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = 'Specialitate'
LEFT JOIN Departamente sub ON pm.SubspecializareID = sub.DepartamentID AND sub.Tip = 'Subspecialitate'
WHERE pm.EsteActiv = 1 
  AND (pm.Pozitie LIKE '%Doctor%' OR pm.Pozitie LIKE '%Medic%')
ORDER BY pm.Nume, pm.Prenume";

            var items = await connection.QueryAsync<PersonalMedicalListDtoRaw>(sql);
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
            
            var groupByColumn = GetValidColumnName(query.GroupBy) ?? "CategorieNume";
            
            var sql = $@"
SELECT 
    ISNULL({groupByColumn}, 'Nu este specificat') as [Key],
    COUNT(*) as Count,
    MAX(pm.DataCreare) as LastDataCreare
FROM PersonalMedical pm WITH (NOLOCK)
LEFT JOIN Departamente cat ON pm.CategorieID = cat.DepartamentID AND cat.Tip = 'Categorie'
LEFT JOIN Departamente spec ON pm.SpecializareID = spec.DepartamentID AND spec.Tip = 'Specialitate'
WHERE 1=1
  AND (@EsteActiv IS NULL OR pm.EsteActiv = @EsteActiv)
  AND (@CategorieID IS NULL OR pm.CategorieID = @CategorieID)
  AND (@SpecializareID IS NULL OR pm.SpecializareID = @SpecializareID)
  AND (@Search IS NULL OR @Search = '' OR 
       UPPER(pm.Nume) LIKE '%' + UPPER(@Search) + '%' OR 
       UPPER(pm.Prenume) LIKE '%' + UPPER(@Search) + '%')
GROUP BY {groupByColumn}
ORDER BY Count DESC";

            var parameters = new DynamicParameters();
            parameters.Add("@Search", query.Search);
            parameters.Add("@CategorieID", query.CategorieID);
            parameters.Add("@SpecializareID", query.SpecializareID);
            parameters.Add("@EsteActiv", query.EsteActiv);

            var items = await connection.QueryAsync<PersonalMedicalGroupAggregateDto>(sql, parameters);

            return Result<IEnumerable<PersonalMedicalGroupAggregateDto>>.Success(items);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PersonalMedicalGroupAggregateDto>>.Failure($"Eroare la agregarea grupurilor: {ex.Message}");
        }
    }

    private string? GetValidColumnName(string column)
    {
        return column?.ToLower() switch
        {
            "nume" => "pm.Nume",
            "prenume" => "pm.Prenume", 
            "numecomplet" => "pm.Nume", 
            "pozitie" => "pm.Pozitie",
            "departament" => "cat.Nume",
            "categorie" => "cat.Nume",
            "specializare" => "spec.Nume",
            "numarlicenta" => "pm.NumarLicenta",
            "telefon" => "pm.Telefon",
            "email" => "pm.Email",
            "esteactiv" => "pm.EsteActiv",
            "datacreare" => "pm.DataCreare",
            _ => null
        };
    }
}

// Raw DTOs pentru maparea din database cu toate coloanele noi
internal class PersonalMedicalListDtoRaw
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
    // Noi propriet??i ierarhice
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    public string? CategorieNume { get; set; }
    public string? SpecializareNume { get; set; }
    public string? SubspecializareNume { get; set; }
}

internal class PersonalMedicalDetailDtoRaw : PersonalMedicalListDtoRaw
{
    // Inherit all properties from PersonalMedicalListDtoRaw
}

internal class PersonalCreateResult
{
    public Guid PersonalID { get; set; }
}