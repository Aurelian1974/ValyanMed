using Dapper;
using Shared.Common;
using Shared.Models.Authentication;
using System.Data;
using Application.Services.Authentication;

namespace Infrastructure.Repositories.Authentication;

public class UtilizatoriSistemRepository : IUtilizatorRepository
{
    private readonly IDbConnection _connection;

    public UtilizatoriSistemRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<int>> CreateAsync(Utilizator utilizator)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@NumeUtilizator", utilizator.NumeUtilizator);
            parameters.Add("@HashParola", utilizator.ParolaHash);
            parameters.Add("@Email", utilizator.Email);
            parameters.Add("@PersonalID", utilizator.PersoanaId);

            // Inserare in UtilizatoriSistem
            var query = @"
                INSERT INTO UtilizatoriSistem (UtilizatorID, NumeUtilizator, HashParola, Email, PersonalID, EsteActiv, DataCreare)
                OUTPUT INSERTED.UtilizatorID
                VALUES (NEWID(), @NumeUtilizator, @HashParola, @Email, @PersonalID, 1, GETDATE())";

            var id = await _connection.QuerySingleAsync<Guid>(query, parameters);
            
            return Result<int>.Success(0, "Utilizatorul a fost creat cu succes"); // Return 0 since we use GUID
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
            // Pentru compatibilitate cu interfa?a, vom c?uta dup? PersonalID convertit
            // Aten?ie: aceasta este o solu?ie temporar?
            var query = @"
                SELECT us.*, pm.Nume, pm.Prenume, pm.Specializare, pm.Pozitie
                FROM UtilizatoriSistem us
                LEFT JOIN PersonalMedical pm ON us.PersonalID = pm.PersonalID
                WHERE CAST(us.PersonalID AS VARCHAR(36)) = @Id AND us.EsteActiv = 1";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id.ToString());

            var utilizator = await _connection.QuerySingleOrDefaultAsync<dynamic>(query, parameters);
            
            if (utilizator == null)
            {
                return Result<Utilizator?>.Success(null);
            }

            var result = MapDynamicToUtilizator(utilizator);
            return Result<Utilizator?>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<Utilizator?>.Failure($"Eroare la obtinerea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result<Utilizator?>> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        try
        {
            var query = @"
                SELECT us.*, pm.Nume, pm.Prenume, pm.Specializare, pm.Pozitie, pm.NumarLicenta, pm.Telefon as TelefonPersonal
                FROM UtilizatoriSistem us
                LEFT JOIN PersonalMedical pm ON us.PersonalID = pm.PersonalID
                WHERE (us.NumeUtilizator = @UsernameOrEmail OR us.Email = @UsernameOrEmail) 
                AND us.EsteActiv = 1";

            var parameters = new DynamicParameters();
            parameters.Add("@UsernameOrEmail", usernameOrEmail);

            var utilizator = await _connection.QuerySingleOrDefaultAsync<dynamic>(query, parameters);
            
            if (utilizator == null)
            {
                return Result<Utilizator?>.Success(null);
            }

            var result = MapDynamicToUtilizatorWithPersonal(utilizator);
            return Result<Utilizator?>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<Utilizator?>.Failure($"Eroare la obtinerea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Utilizator>>> GetAllAsync()
    {
        try
        {
            var query = @"
                SELECT us.*, pm.Nume, pm.Prenume, pm.Specializare, pm.Pozitie, pm.NumarLicenta
                FROM UtilizatoriSistem us
                LEFT JOIN PersonalMedical pm ON us.PersonalID = pm.PersonalID
                WHERE us.EsteActiv = 1
                ORDER BY pm.Nume, pm.Prenume";

            var utilizatori = await _connection.QueryAsync<dynamic>(query);
            
            var result = utilizatori.Select(MapDynamicToUtilizatorWithPersonal);
            return Result<IEnumerable<Utilizator>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Utilizator>>.Failure($"Eroare la obtinerea utilizatorilor: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Utilizator utilizator)
    {
        try
        {
            var query = @"
                UPDATE UtilizatoriSistem 
                SET NumeUtilizator = @NumeUtilizator,
                    Email = @Email,
                    HashParola = CASE WHEN @HashParola IS NOT NULL THEN @HashParola ELSE HashParola END
                WHERE PersonalID = @PersonalID";

            var parameters = new DynamicParameters();
            parameters.Add("@NumeUtilizator", utilizator.NumeUtilizator);
            parameters.Add("@Email", utilizator.Email);
            parameters.Add("@HashParola", string.IsNullOrEmpty(utilizator.ParolaHash) ? null : utilizator.ParolaHash);
            parameters.Add("@PersonalID", utilizator.PersoanaId);

            await _connection.ExecuteAsync(query, parameters);
            
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
            // Soft delete - setam EsteActiv = 0
            var query = @"
                UPDATE UtilizatoriSistem 
                SET EsteActiv = 0
                WHERE PersonalID = @Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await _connection.ExecuteAsync(query, parameters);
            
            return Result.Success("Utilizatorul a fost sters cu succes");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la stergerea utilizatorului: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ExistsByUsernameAsync(string username, int? excludeId = null)
    {
        try
        {
            var query = "SELECT COUNT(1) FROM UtilizatoriSistem WHERE NumeUtilizator = @Username AND EsteActiv = 1";
            if (excludeId.HasValue)
            {
                query += " AND PersonalID != @ExcludeId";
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
            var query = "SELECT COUNT(1) FROM UtilizatoriSistem WHERE Email = @Email AND EsteActiv = 1";
            if (excludeId.HasValue)
            {
                query += " AND PersonalID != @ExcludeId";
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
            Id = 0, // Nu folosim ID numeric pentru UtilizatoriSistem
            Guid = (Guid)item.UtilizatorID,
            PersoanaId = GetPersonalIdAsInt(item.PersonalID), // Convertim GUID la int pentru compatibilitate
            NumeUtilizator = item.NumeUtilizator?.ToString() ?? string.Empty,
            ParolaHash = item.HashParola?.ToString() ?? string.Empty,
            Email = item.Email?.ToString() ?? string.Empty,
            Telefon = item.TelefonPersonal?.ToString(),
            DataCreare = item.DataCreare != null ? (DateTime?)item.DataCreare : null,
            DataModificare = null, // UtilizatoriSistem nu are coloana DataModificare
            EsteActiv = item.EsteActiv != null ? (bool)item.EsteActiv : true
        };
    }

    private static Utilizator MapDynamicToUtilizatorWithPersonal(dynamic item)
    {
        var utilizator = MapDynamicToUtilizator(item);
        
        // Mapam informatiile despre personalul medical
        if (item.Nume != null && item.Prenume != null)
        {
            utilizator.Persoana = new Persoana
            {
                Id = utilizator.PersoanaId, // Folosim ID-ul convertit
                Guid = (Guid)item.PersonalID, // P?str?m GUID-ul original
                Nume = item.Nume?.ToString() ?? string.Empty,
                Prenume = item.Prenume?.ToString() ?? string.Empty,
                // Adaugam informatii specifice personalului medical
                PozitieOrganizatie = item.Pozitie?.ToString()
            };
            
            if (!string.IsNullOrEmpty(item.Specializare?.ToString()))
            {
                utilizator.Specializare = item.Specializare.ToString();
            }
            
            // Folosim NumarLicenta in loc de CodPersonal
            if (!string.IsNullOrEmpty(item.NumarLicenta?.ToString()))
            {
                utilizator.CodPersonal = item.NumarLicenta.ToString();
            }
        }

        return utilizator;
    }

    // Helper method pentru conversia GUID la int
    private static int GetPersonalIdAsInt(object personalIdValue)
    {
        if (personalIdValue is Guid guid)
        {
            // Convertim GUID la un hash int pentru compatibilitate
            return Math.Abs(guid.GetHashCode());
        }
        return 0;
    }
}