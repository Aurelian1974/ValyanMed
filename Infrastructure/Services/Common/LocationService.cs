using Application.Services.Common;
using Shared.DTOs.Common;
using System.Data;
using Dapper;

namespace Infrastructure.Services.Common;

public class LocationService : ILocationService
{
    private readonly IDbConnection _connection;
    
    public LocationService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<JudetDto>> GetJudeteAsync()
    {
        try
        {
            var result = await _connection.QueryAsync<JudetDto>(
                "GetAllJudete", 
                commandType: CommandType.StoredProcedure);
            
            return result.ToList();
        }
        catch (Exception)
        {
            return new List<JudetDto>();
        }
    }

    public async Task<List<LocalitateDto>> GetLocalitatiAsync()
    {
        try
        {
            // Get all counties first to populate JudetNume
            var judete = await GetJudeteAsync();
            var judeteDictionary = judete.ToDictionary(j => j.IdJudet, j => j.Nume);
            
            // Get all localities by iterating through all counties
            var allLocalitati = new List<LocalitateDto>();
            
            foreach (var judet in judete)
            {
                var localitatiForJudet = await GetLocalitatiByJudetIdAsync(judet.IdJudet);
                allLocalitati.AddRange(localitatiForJudet);
            }
            
            return allLocalitati;
        }
        catch (Exception)
        {
            return new List<LocalitateDto>();
        }
    }

    public async Task<List<LocalitateDto>> GetLocalitatiByJudetAsync(string judetNume)
    {
        try
        {
            // First find the county by name to get the ID
            var judete = await GetJudeteAsync();
            var judet = judete.FirstOrDefault(j => string.Equals(j.Nume, judetNume, StringComparison.OrdinalIgnoreCase));
            
            if (judet == null)
            {
                return new List<LocalitateDto>();
            }
            
            return await GetLocalitatiByJudetIdAsync(judet.IdJudet);
        }
        catch (Exception)
        {
            return new List<LocalitateDto>();
        }
    }

    public async Task<List<LocalitateDto>> GetLocalitatiByJudetIdAsync(int judetId)
    {
        try
        {
            var result = await _connection.QueryAsync<LocalitateDto>(
                "Localitate_GetByJudet",
                new { IdJudet = judetId },
                commandType: CommandType.StoredProcedure);
            
            // Get county name for UI compatibility
            var judete = await GetJudeteAsync();
            var judetNume = judete.FirstOrDefault(j => j.IdJudet == judetId)?.Nume ?? string.Empty;
            
            var localitati = result.ToList();
            foreach (var localitate in localitati)
            {
                localitate.JudetNume = judetNume;
            }
            
            return localitati;
        }
        catch (Exception)
        {
            return new List<LocalitateDto>();
        }
    }

    public async Task<List<JudetWithLocalitatiDto>> GetJudeteWithLocalitatiAsync()
    {
        try
        {
            var judete = await GetJudeteAsync();
            var result = new List<JudetWithLocalitatiDto>();
            
            foreach (var judet in judete)
            {
                var localitati = await GetLocalitatiByJudetIdAsync(judet.IdJudet);
                
                result.Add(new JudetWithLocalitatiDto
                {
                    IdJudet = judet.IdJudet,
                    JudetGuid = judet.JudetGuid,
                    CodJudet = judet.CodJudet,
                    Nume = judet.Nume,
                    Siruta = judet.Siruta,
                    CodAuto = judet.CodAuto,
                    Ordine = judet.Ordine,
                    Localitati = localitati
                });
            }
            
            return result;
        }
        catch (Exception)
        {
            return new List<JudetWithLocalitatiDto>();
        }
    }
}