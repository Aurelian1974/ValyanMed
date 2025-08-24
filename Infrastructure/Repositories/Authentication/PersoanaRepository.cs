using Dapper;
using Shared.Common;
using Shared.Enums;
using Shared.Models.Authentication;
using System.Data;

namespace Infrastructure.Repositories.Authentication;

public interface IPersoanaRepository
{
    Task<Result<int>> CreateAsync(Persoana persoana);
    Task<Result<Persoana?>> GetByIdAsync(int id);
    Task<Result<IEnumerable<Persoana>>> GetAllAsync();
    Task<Result> UpdateAsync(Persoana persoana);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> ExistsByCNPAsync(string cnp, int? excludeId = null);
}

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

            var id = await _connection.QuerySingleAsync<int>("sp_CreatePersoana", parameters, commandType: CommandType.StoredProcedure);
            
            return Result<int>.Success(id, "Persoana a fost creat? cu succes");
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
            var persoane = await _connection.QueryAsync<dynamic>("sp_GetAllPersonal", commandType: CommandType.StoredProcedure);
            
            var result = persoane.Select(MapDynamicToPersoana);
            return Result<IEnumerable<Persoana>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Persoana>>.Failure($"Eroare la ob?inerea persoanelor: {ex.Message}");
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

            await _connection.ExecuteAsync("sp_UpdatePersoana", parameters, commandType: CommandType.StoredProcedure);
            
            return Result.Success("Persoana a fost actualizat? cu succes");
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
            
            return Result.Success("Persoana a fost ?tears? cu succes");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la ?tergerea persoanei: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ExistsByCNPAsync(string cnp, int? excludeId = null)
    {
        try
        {
            var query = "SELECT COUNT(1) FROM Persoana WHERE CNP = @CNP";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }

            var parameters = new DynamicParameters();
            parameters.Add("@CNP", cnp);
            if (excludeId.HasValue)
            {
                parameters.Add("@ExcludeId", excludeId.Value);
            }

            var count = await _connection.QuerySingleAsync<int>(query, parameters);
            
            return Result<bool>.Success(count > 0);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Eroare la verificarea CNP-ului: {ex.Message}");
        }
    }

    private static Persoana MapDynamicToPersoana(dynamic item)
    {
        return new Persoana
        {
            Id = item.Id,
            Guid = item.Guid,
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
            TipActIdentitate = SafeParseEnum<TipActIdentitate>(item.TipActIdentitate),
            SerieActIdentitate = item.SerieActIdentitate,
            NumarActIdentitate = item.NumarActIdentitate,
            StareCivila = SafeParseEnum<StareCivila>(item.StareCivila),
            Gen = SafeParseEnum<Gen>(item.Gen)
        };
    }

    /// <summary>
    /// Safe enum parsing that handles legacy values and unknown enum values
    /// Updated to handle both short and long enum values for maximum compatibility
    /// </summary>
    private static T? SafeParseEnum<T>(dynamic value) where T : struct, Enum
    {
        if (value == null)
            return null;

        var stringValue = value.ToString();
        if (string.IsNullOrEmpty(stringValue))
            return null;

        // Handle legacy mappings for specific enums
        if (typeof(T) == typeof(TipActIdentitate))
        {
            return stringValue.ToUpper() switch
            {
                // Short values (for DB compatibility)
                "CI" => (T)(object)TipActIdentitate.CI,
                "PASAPORT" => (T)(object)TipActIdentitate.Pasaport,
                "PERMIS" => (T)(object)TipActIdentitate.Permis,
                "CERTIFICAT" => (T)(object)TipActIdentitate.Certificat,
                "ALTUL" => (T)(object)TipActIdentitate.Altul,
                
                // Long values (for completeness)
                "CARTEIDENTITATE" => (T)(object)TipActIdentitate.CarteIdentitate,
                "CARTE IDENTITATE" => (T)(object)TipActIdentitate.CarteIdentitate,
                "CARTE DE IDENTITATE" => (T)(object)TipActIdentitate.CarteIdentitate,
                "PERMISCONDUCERE" => (T)(object)TipActIdentitate.PermisConducere,
                "PERMIS CONDUCERE" => (T)(object)TipActIdentitate.PermisConducere,
                "PERMIS DE CONDUCERE" => (T)(object)TipActIdentitate.PermisConducere,
                "CERTIFICATNASTERE" => (T)(object)TipActIdentitate.CertificatNastere,
                "CERTIFICAT NASTERE" => (T)(object)TipActIdentitate.CertificatNastere,
                "CERTIFICAT DE NASTERE" => (T)(object)TipActIdentitate.CertificatNastere,
                
                // Truncated values (from previous errors)
                "CARTE" => (T)(object)TipActIdentitate.CI,
                "PASAP" => (T)(object)TipActIdentitate.Pasaport,
                "PERMI" => (T)(object)TipActIdentitate.Permis,
                "CERTI" => (T)(object)TipActIdentitate.Certificat,
                
                _ => Enum.TryParse<T>(stringValue, true, out T result) ? result : null
            };
        }

        if (typeof(T) == typeof(StareCivila))
        {
            return stringValue.ToUpper() switch
            {
                // Short values for compatibility
                "CELIBATAR" => (T)(object)StareCivila.Necasatorit,
                "CASATORIT" => (T)(object)StareCivila.Casatorit,
                "DIVORTIT" => (T)(object)StareCivila.Divortit,
                "VADUV" => (T)(object)StareCivila.Vaduv,
                "PARTENER" => (T)(object)StareCivila.Concubinaj,
                
                // Standard long values
                "NECASATORIT" => (T)(object)StareCivila.Necasatorit,
                "NEMARITAT" => (T)(object)StareCivila.Necasatorit,
                "MARIAJ" => (T)(object)StareCivila.Casatorit,
                "MARITAT" => (T)(object)StareCivila.Casatorit,
                "DIVORTAT" => (T)(object)StareCivila.Divortit,
                "VADOVA" => (T)(object)StareCivila.Vaduv,
                "VADUVE" => (T)(object)StareCivila.Vaduv,
                "CONCUBINAJ" => (T)(object)StareCivila.Concubinaj,
                "PARTENERIAT" => (T)(object)StareCivila.Concubinaj,
                
                _ => Enum.TryParse<T>(stringValue, true, out T result) ? result : null
            };
        }

        if (typeof(T) == typeof(Gen))
        {
            return stringValue.ToUpper() switch
            {
                // Short values for compatibility
                "M" => (T)(object)Gen.Masculin,
                "F" => (T)(object)Gen.Feminin,
                "N" => (T)(object)Gen.Neprecizat,
                
                // Standard long values
                "MASCULIN" => (T)(object)Gen.Masculin,
                "BARBAT" => (T)(object)Gen.Masculin,
                "MALE" => (T)(object)Gen.Masculin,
                "FEMININ" => (T)(object)Gen.Feminin,
                "FEMEIE" => (T)(object)Gen.Feminin,
                "FEMALE" => (T)(object)Gen.Feminin,
                "NEPRECIZAT" => (T)(object)Gen.Neprecizat,
                
                _ => Enum.TryParse<T>(stringValue, true, out T result) ? result : null
            };
        }

        // Default fallback for standard enum parsing
        return Enum.TryParse<T>(stringValue, true, out T defaultResult) ? defaultResult : null;
    }
}