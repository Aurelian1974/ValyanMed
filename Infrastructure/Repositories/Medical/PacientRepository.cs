using Shared.DTOs.Medical;
using Shared.Common;
using System.Data;
using Dapper;
using Application.Services.Medical;

namespace Infrastructure.Repositories.Medical;

public class PacientRepository : IPacientRepository
{
    private readonly IDbConnection _connection;

    public PacientRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<PagedResult<PacientListDto>> GetPagedAsync(PacientiSearchQuery searchQuery)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Search", searchQuery.Search);
            parameters.Add("@Judet", searchQuery.Judet);
            parameters.Add("@Gen", searchQuery.Gen);
            parameters.Add("@Page", searchQuery.Page);
            parameters.Add("@PageSize", searchQuery.PageSize);
            parameters.Add("@Sort", searchQuery.Sort);

            using var multi = await _connection.QueryMultipleAsync(
                "sp_Pacienti_GetPaged",
                parameters,
                commandType: CommandType.StoredProcedure);

            var items = await multi.ReadAsync<PacientListDto>();
            var totalCount = await multi.ReadFirstAsync<int>();

            var pagedResult = new PagedResult<PacientListDto>(
                items.ToList(),
                totalCount,
                searchQuery.Page,
                searchQuery.PageSize);

            return pagedResult;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<PacientListDto?> GetByIdAsync(Guid pacientId)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PacientID", pacientId);

            var patient = await _connection.QueryFirstOrDefaultAsync<PacientListDto>(
                @"SELECT 
                    p.PacientID,
                    p.Nume,
                    p.Prenume,
                    CONCAT(p.Nume, ' ', p.Prenume) AS NumeComplet,
                    p.CNP,
                    p.DataNasterii,
                    p.Gen,
                    p.Telefon,
                    p.Email,
                    p.Oras,
                    p.Judet,
                    p.DataCreare,
                    CASE 
                        WHEN p.DataNasterii IS NOT NULL 
                        THEN DATEDIFF(YEAR, p.DataNasterii, GETDATE()) - 
                             CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.DataNasterii, GETDATE()), p.DataNasterii) > GETDATE() 
                                  THEN 1 ELSE 0 END
                        ELSE 0 
                    END AS Varsta
                  FROM dbo.Pacienti p
                  WHERE p.PacientID = @PacientID",
                parameters);

            return patient;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<PacientListDto?> GetByCNPAsync(string cnp)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@CNP", cnp);

            var patient = await _connection.QueryFirstOrDefaultAsync<PacientListDto>(
                @"SELECT 
                    p.PacientID,
                    p.Nume,
                    p.Prenume,
                    CONCAT(p.Nume, ' ', p.Prenume) AS NumeComplet,
                    p.CNP,
                    p.DataNasterii,
                    p.Gen,
                    p.Telefon,
                    p.Email,
                    p.Oras,
                    p.Judet,
                    p.DataCreare,
                    CASE 
                        WHEN p.DataNasterii IS NOT NULL 
                        THEN DATEDIFF(YEAR, p.DataNasterii, GETDATE()) - 
                             CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.DataNasterii, GETDATE()), p.DataNasterii) > GETDATE() 
                                  THEN 1 ELSE 0 END
                        ELSE 0 
                    END AS Varsta
                  FROM dbo.Pacienti p
                  WHERE p.CNP = @CNP",
                parameters);

            return patient;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<PacientListDto?> GetByEmailAsync(string email)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);

            var patient = await _connection.QueryFirstOrDefaultAsync<PacientListDto>(
                @"SELECT 
                    p.PacientID,
                    p.Nume,
                    p.Prenume,
                    CONCAT(p.Nume, ' ', p.Prenume) AS NumeComplet,
                    p.CNP,
                    p.DataNasterii,
                    p.Gen,
                    p.Telefon,
                    p.Email,
                    p.Oras,
                    p.Judet,
                    p.DataCreare,
                    CASE 
                        WHEN p.DataNasterii IS NOT NULL 
                        THEN DATEDIFF(YEAR, p.DataNasterii, GETDATE()) - 
                             CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.DataNasterii, GETDATE()), p.DataNasterii) > GETDATE() 
                                  THEN 1 ELSE 0 END
                        ELSE 0 
                    END AS Varsta
                  FROM dbo.Pacienti p
                  WHERE p.Email = @Email",
                parameters);

            return patient;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<Guid> CreateAsync(CreatePacientRequest request)
    {
        try
        {
            var pacientId = Guid.NewGuid();

            var parameters = new DynamicParameters();
            parameters.Add("@PacientID", pacientId);
            parameters.Add("@Nume", request.Nume);
            parameters.Add("@Prenume", request.Prenume);
            parameters.Add("@CNP", request.CNP);
            parameters.Add("@DataNasterii", request.DataNasterii);
            parameters.Add("@Gen", request.Gen);
            parameters.Add("@Telefon", request.Telefon);
            parameters.Add("@Email", request.Email);
            parameters.Add("@Adresa", request.Adresa);
            parameters.Add("@Oras", request.Oras);
            parameters.Add("@Judet", request.Judet);
            parameters.Add("@CodPostal", request.CodPostal);
            parameters.Add("@NumeContactUrgenta", request.NumeContactUrgenta);
            parameters.Add("@TelefonContactUrgenta", request.TelefonContactUrgenta);
            parameters.Add("@FurnizorAsigurare", request.FurnizorAsigurare);
            parameters.Add("@NumarAsigurare", request.NumarAsigurare);
            parameters.Add("@DataCreare", DateTime.UtcNow);
            parameters.Add("@DataUltimeiModificari", DateTime.UtcNow);
            parameters.Add("@EsteActiv", true);

            await _connection.ExecuteAsync(
                @"INSERT INTO dbo.Pacienti 
                  (PacientID, Nume, Prenume, CNP, DataNasterii, Gen, 
                   Telefon, Email, Adresa, Oras, Judet, CodPostal, NumeContactUrgenta, TelefonContactUrgenta,
                   FurnizorAsigurare, NumarAsigurare, DataCreare, DataUltimeiModificari, EsteActiv)
                  VALUES 
                  (@PacientID, @Nume, @Prenume, @CNP, @DataNasterii, @Gen,
                   @Telefon, @Email, @Adresa, @Oras, @Judet, @CodPostal, @NumeContactUrgenta, @TelefonContactUrgenta,
                   @FurnizorAsigurare, @NumarAsigurare, @DataCreare, @DataUltimeiModificari, @EsteActiv)",
                parameters);

            return pacientId;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<bool> UpdateAsync(UpdatePacientRequest request)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PacientID", request.PacientID);
            parameters.Add("@Nume", request.Nume);
            parameters.Add("@Prenume", request.Prenume);
            parameters.Add("@CNP", request.CNP);
            parameters.Add("@DataNasterii", request.DataNasterii);
            parameters.Add("@Gen", request.Gen);
            parameters.Add("@Telefon", request.Telefon);
            parameters.Add("@Email", request.Email);
            parameters.Add("@Adresa", request.Adresa);
            parameters.Add("@Oras", request.Oras);
            parameters.Add("@Judet", request.Judet);
            parameters.Add("@CodPostal", request.CodPostal);
            parameters.Add("@NumeContactUrgenta", request.NumeContactUrgenta);
            parameters.Add("@TelefonContactUrgenta", request.TelefonContactUrgenta);
            parameters.Add("@FurnizorAsigurare", request.FurnizorAsigurare);
            parameters.Add("@NumarAsigurare", request.NumarAsigurare);
            parameters.Add("@DataUltimeiModificari", DateTime.UtcNow);

            var affectedRows = await _connection.ExecuteAsync(
                @"UPDATE dbo.Pacienti 
                  SET Nume = @Nume, 
                      Prenume = @Prenume, 
                      CNP = @CNP, 
                      DataNasterii = @DataNasterii, 
                      Gen = @Gen,
                      Telefon = @Telefon, 
                      Email = @Email, 
                      Adresa = @Adresa, 
                      Oras = @Oras, 
                      Judet = @Judet, 
                      CodPostal = @CodPostal,
                      NumeContactUrgenta = @NumeContactUrgenta,
                      TelefonContactUrgenta = @TelefonContactUrgenta,
                      FurnizorAsigurare = @FurnizorAsigurare,
                      NumarAsigurare = @NumarAsigurare,
                      DataUltimeiModificari = @DataUltimeiModificari
                  WHERE PacientID = @PacientID",
                parameters);

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid pacientId)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PacientID", pacientId);

            var affectedRows = await _connection.ExecuteAsync(
                "DELETE FROM dbo.Pacienti WHERE PacientID = @PacientID",
                parameters);

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}