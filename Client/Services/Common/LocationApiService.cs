using System.Net.Http.Json;
using Shared.DTOs.Common;

namespace Client.Services.Common;

public interface ILocationApiService
{
    Task<List<JudetDto>> GetJudeteAsync();
    Task<List<LocalitateDto>> GetLocalitatiAsync();
    Task<List<LocalitateDto>> GetLocalitatiByJudetAsync(string judetNume);
    Task<List<LocalitateDto>> GetLocalitatiByJudetIdAsync(int judetId);
    Task<List<JudetWithLocalitatiDto>> GetJudeteWithLocalitatiAsync();
}

public class LocationApiService : ILocationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocationApiService> _logger;

    public LocationApiService(HttpClient httpClient, ILogger<LocationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<JudetDto>> GetJudeteAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Location/judete");
            
            if (response.IsSuccessStatusCode)
            {
                var judete = await response.Content.ReadFromJsonAsync<List<JudetDto>>();
                return judete ?? new List<JudetDto>();
            }
            
            _logger.LogWarning("Failed to get counties. Status: {StatusCode}", response.StatusCode);
            return new List<JudetDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving counties");
            return new List<JudetDto>();
        }
    }

    public async Task<List<LocalitateDto>> GetLocalitatiAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Location/localitati");
            
            if (response.IsSuccessStatusCode)
            {
                var localitati = await response.Content.ReadFromJsonAsync<List<LocalitateDto>>();
                return localitati ?? new List<LocalitateDto>();
            }
            
            _logger.LogWarning("Failed to get localities. Status: {StatusCode}", response.StatusCode);
            return new List<LocalitateDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localities");
            return new List<LocalitateDto>();
        }
    }

    public async Task<List<LocalitateDto>> GetLocalitatiByJudetAsync(string judetNume)
    {
        try
        {
            var encodedJudetNume = Uri.EscapeDataString(judetNume);
            var response = await _httpClient.GetAsync($"api/Location/localitati/judet/{encodedJudetNume}");
            
            if (response.IsSuccessStatusCode)
            {
                var localitati = await response.Content.ReadFromJsonAsync<List<LocalitateDto>>();
                return localitati ?? new List<LocalitateDto>();
            }
            
            _logger.LogWarning("Failed to get localities for county {County}. Status: {StatusCode}", judetNume, response.StatusCode);
            return new List<LocalitateDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localities for county {County}", judetNume);
            return new List<LocalitateDto>();
        }
    }

    public async Task<List<LocalitateDto>> GetLocalitatiByJudetIdAsync(int judetId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Location/localitati/judet-id/{judetId}");
            
            if (response.IsSuccessStatusCode)
            {
                var localitati = await response.Content.ReadFromJsonAsync<List<LocalitateDto>>();
                return localitati ?? new List<LocalitateDto>();
            }
            
            _logger.LogWarning("Failed to get localities for county ID {CountyId}. Status: {StatusCode}", judetId, response.StatusCode);
            return new List<LocalitateDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localities for county ID {CountyId}", judetId);
            return new List<LocalitateDto>();
        }
    }

    public async Task<List<JudetWithLocalitatiDto>> GetJudeteWithLocalitatiAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Location/judete-cu-localitati");
            
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<JudetWithLocalitatiDto>>();
                return data ?? new List<JudetWithLocalitatiDto>();
            }
            
            _logger.LogWarning("Failed to get counties with localities. Status: {StatusCode}", response.StatusCode);
            return new List<JudetWithLocalitatiDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving counties with localities");
            return new List<JudetWithLocalitatiDto>();
        }
    }
}