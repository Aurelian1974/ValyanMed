using Dapper;
using Shared.Common;
using Shared.Models.Authentication;
using System.Data;
using Application.Services.Authentication;

namespace Infrastructure.Repositories.Authentication;

public class UtilizatorRepository : IUtilizatorRepository
{
    private readonly IDbConnection _connection;

    public UtilizatorRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<int>> CreateAsync(Utilizator utilizator)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@NumeUtilizator", utilizator.NumeUtilizator);
            parameters.Add("@ParolaHash", utilizator.ParolaHash);
            parameters.Add("@Email", utilizator.Email);
            parameters.Add("@Telefon", utilizator.Telefon);
            parameters.Add("@PersoanaId", utilizator.PersoanaId);

            var id = await _connection.QuerySingleAsync<int>("usp_Utilizator_Insert", parameters, commandType: CommandType.StoredProcedure);
            
            return Result<int>.Success(id, "Utilizatorul a fost creat cu succes");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Eroare la crearea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result<Utilizator?>> GetByIdAsync(int id)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var utilizator = await _connection.QuerySingleOrDefaultAsync<dynamic>("usp_Utilizator_GetById", parameters, commandType: CommandType.StoredProcedure);
            
            if (utilizator == null)
            {
                return Result<Utilizator?>.Success(null);
            }

            var result = MapDynamicToUtilizator(utilizator);
            return Result<Utilizator?>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<Utilizator?>.Failure($"Eroare la ob?inerea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result<Utilizator?>> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@NumeUtilizatorSauEmail", usernameOrEmail);

            var utilizator = await _connection.QuerySingleOrDefaultAsync<dynamic>("usp_Utilizator_Authenticate", parameters, commandType: CommandType.StoredProcedure);
            
            if (utilizator == null)
            {
                return Result<Utilizator?>.Success(null);
            }

            var result = MapDynamicToUtilizatorWithPersoana(utilizator);
            return Result<Utilizator?>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<Utilizator?>.Failure($"Eroare la ob?inerea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Utilizator>>> GetAllAsync()
    {
        try
        {
            var utilizatori = await _connection.QueryAsync<dynamic>("usp_Utilizator_GetAll", commandType: CommandType.StoredProcedure);
            
            var result = utilizatori.Select(MapDynamicToUtilizatorWithPersoana);
            return Result<IEnumerable<Utilizator>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Utilizator>>.Failure($"Eroare la ob?inerea utilizatorilor: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Utilizator utilizator)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", utilizator.Id);
            parameters.Add("@NumeUtilizator", utilizator.NumeUtilizator);
            parameters.Add("@Email", utilizator.Email);
            parameters.Add("@Telefon", utilizator.Telefon);
            parameters.Add("@PersoanaId", utilizator.PersoanaId);
            parameters.Add("@ParolaHash", string.IsNullOrEmpty(utilizator.ParolaHash) ? null : utilizator.ParolaHash);

            await _connection.ExecuteAsync("usp_Utilizator_Update", parameters, commandType: CommandType.StoredProcedure);
            
            return Result.Success("Utilizatorul a fost actualizat cu succes");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la actualizarea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await _connection.ExecuteAsync("usp_Utilizator_Delete", parameters, commandType: CommandType.StoredProcedure);
            
            return Result.Success("Utilizatorul a fost ?ters cu succes");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la ?tergerea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ExistsByUsernameAsync(string username, int? excludeId = null)
    {
        try
        {
            var query = "SELECT COUNT(1) FROM Utilizator WHERE NumeUtilizator = @Username";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);
            if (excludeId.HasValue)
            {
                parameters.Add("@ExcludeId", excludeId.Value);
            }

            var count = await _connection.QuerySingleAsync<int>(query, parameters);
            
            return Result<bool>.Success(count > 0);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Eroare la verificarea numelui de utilizator: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        try
        {
            var query = "SELECT COUNT(1) FROM Utilizator WHERE Email = @Email";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);
            if (excludeId.HasValue)
            {
                parameters.Add("@ExcludeId", excludeId.Value);
            }

            var count = await _connection.QuerySingleAsync<int>(query, parameters);
            
            return Result<bool>.Success(count > 0);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Eroare la verificarea email-ului: {ex.Message}");
        }
    }

    private static Utilizator MapDynamicToUtilizator(dynamic item)
    {
        return new Utilizator
        {
            Id = item.Id,
            Guid = item.Guid,
            PersoanaId = item.PersoanaId,
            NumeUtilizator = item.NumeUtilizator ?? string.Empty,
            ParolaHash = item.ParolaHash ?? string.Empty,
            Email = item.Email ?? string.Empty,
            Telefon = item.Telefon,
            DataCreare = item.DataCreare,
            DataModificare = item.DataModificare
        };
    }

    private static Utilizator MapDynamicToUtilizatorWithPersoana(dynamic item)
    {
        var utilizator = MapDynamicToUtilizator(item);
        
        // Verific?m dac? avem informa?ii despre persoan?
        if (item.Nume != null && item.Prenume != null)
        {
            utilizator.Persoana = new Persoana
            {
                Id = utilizator.PersoanaId,
                Nume = item.Nume,
                Prenume = item.Prenume
            };
        }

        return utilizator;
    }
}