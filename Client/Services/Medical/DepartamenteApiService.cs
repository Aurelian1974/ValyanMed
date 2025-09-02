using Shared.DTOs.Medical;
using Shared.Common;
using System.Net.Http.Json; // AD?UGAT pentru ReadFromJsonAsync

namespace Client.Services.Medical;

public interface IDepartamenteApiService
{
    Task<Result<IEnumerable<DepartamentOptionDto>>> GetCategoriiAsync();
    Task<Result<IEnumerable<DepartamentOptionDto>>> GetSpecializariByCategorieAsync(Guid categorieId); // CORECTAT numele metodei
    Task<Result<IEnumerable<DepartamentOptionDto>>> GetSubspecializariBySpecializareAsync(Guid specializareId);
    Task<Result<DepartamentIerarhicDto?>> GetDepartamentIerarhicAsync(Guid departamentId);
    Task<Result<IEnumerable<DepartamentIerarhicDto>>> GetIerarhieCompletaAsync();
}

public class DepartamenteApiService : IDepartamenteApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<DepartamenteApiService> _logger;

    public DepartamenteApiService(HttpClient http, ILogger<DepartamenteApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<DepartamentOptionDto>>> GetCategoriiAsync()
    {
        try
        {
            _logger.LogDebug("Fetching categorii departamente from API");
            
            var response = await _http.GetAsync("api/departamente/categorii");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<IEnumerable<DepartamentOptionDto>>>();
                return result ?? Result<IEnumerable<DepartamentOptionDto>>.Failure("R?spuns null de la server");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error fetching categorii: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<IEnumerable<DepartamentOptionDto>>.Failure($"Eroare server: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching categorii departamente");
            return Result<IEnumerable<DepartamentOptionDto>>.Failure($"Eroare la înc?rcarea categoriilor: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<DepartamentOptionDto>>> GetSpecializariByCategorieAsync(Guid categorieId) // CORECTAT numele metodei
    {
        try
        {
            _logger.LogDebug("Fetching specializari for categorie {CategorieId}", categorieId);
            
            var response = await _http.GetAsync($"api/departamente/specializari/{categorieId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<IEnumerable<DepartamentOptionDto>>>();
                return result ?? Result<IEnumerable<DepartamentOptionDto>>.Failure("R?spuns null de la server");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error fetching specializari: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<IEnumerable<DepartamentOptionDto>>.Failure($"Eroare server: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching specializari for categorie {CategorieId}", categorieId);
            return Result<IEnumerable<DepartamentOptionDto>>.Failure($"Eroare la înc?rcarea specializ?rilor: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<DepartamentOptionDto>>> GetSubspecializariBySpecializareAsync(Guid specializareId)
    {
        try
        {
            _logger.LogDebug("Fetching subspecializari for specializare {SpecializareId}", specializareId);
            
            var response = await _http.GetAsync($"api/departamente/subspecializari/{specializareId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<IEnumerable<DepartamentOptionDto>>>();
                return result ?? Result<IEnumerable<DepartamentOptionDto>>.Failure("R?spuns null de la server");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error fetching subspecializari: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<IEnumerable<DepartamentOptionDto>>.Failure($"Eroare server: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching subspecializari for specializare {SpecializareId}", specializareId);
            return Result<IEnumerable<DepartamentOptionDto>>.Failure($"Eroare la înc?rcarea subspecializ?rilor: {ex.Message}");
        }
    }

    public async Task<Result<DepartamentIerarhicDto?>> GetDepartamentIerarhicAsync(Guid departamentId)
    {
        try
        {
            _logger.LogDebug("Fetching departament ierarhic {DepartamentId}", departamentId);
            
            var response = await _http.GetAsync($"api/departamente/{departamentId}/ierarhic");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<DepartamentIerarhicDto?>>();
                return result ?? Result<DepartamentIerarhicDto?>.Failure("R?spuns null de la server");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Result<DepartamentIerarhicDto?>.Success(null);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error fetching departament ierarhic: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<DepartamentIerarhicDto?>.Failure($"Eroare server: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching departament ierarhic {DepartamentId}", departamentId);
            return Result<DepartamentIerarhicDto?>.Failure($"Eroare la înc?rcarea departamentului: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<DepartamentIerarhicDto>>> GetIerarhieCompletaAsync()
    {
        try
        {
            _logger.LogDebug("Fetching ierarhie completa departamente");
            
            var response = await _http.GetAsync("api/departamente/ierarhie");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<IEnumerable<DepartamentIerarhicDto>>>();
                return result ?? Result<IEnumerable<DepartamentIerarhicDto>>.Failure("R?spuns null de la server");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error fetching ierarhie completa: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return Result<IEnumerable<DepartamentIerarhicDto>>.Failure($"Eroare server: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching ierarhie completa departamente");
            return Result<IEnumerable<DepartamentIerarhicDto>>.Failure($"Eroare la înc?rcarea ierarhiei: {ex.Message}");
        }
    }
}