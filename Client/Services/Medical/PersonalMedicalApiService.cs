using System.Net.Http.Json;
using System.Text.Json;
using Shared.Common;
using Shared.DTOs.Medical;
using System.Linq;
using System.Text.Json.Serialization;

namespace Client.Services.Medical;

public interface IPersonalMedicalApiService
{
    Task<Result<PagedResult<PersonalMedicalListDto>>> GetPagedAsync(PersonalMedicalSearchQuery query);
    Task<Result<PersonalMedicalDataGridResult>> GetDataGridAsync(PersonalMedicalDataGridRequest request);
    Task<Result<IEnumerable<PersonalMedicalGroupAggregateDto>>> GetGroupAggregatesAsync(PersonalMedicalGroupAggregateQuery query);
    Task<Result<PersonalMedicalDetailDto?>> GetByIdAsync(Guid personalId);
    Task<Result<Guid>> CreateAsync(CreatePersonalMedicalRequest request);
    Task<Result> UpdateAsync(Guid personalId, UpdatePersonalMedicalRequest request);
    Task<Result> DeleteAsync(Guid personalId);
    Task<Result> ToggleStatusAsync(Guid personalId);
    Task<Result<IEnumerable<PersonalMedicalListDto>>> GetActiveDoctorsAsync();
}

public class PersonalMedicalApiService : IPersonalMedicalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PersonalMedicalApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PersonalMedicalApiService(HttpClient httpClient, ILogger<PersonalMedicalApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<Result<PersonalMedicalDataGridResult>> GetDataGridAsync(PersonalMedicalDataGridRequest request)
    {
        try
        {
            _logger.LogInformation("Calling DataGrid API with grouping: {HasGroups}", request.Groups?.Any() == true);

            var response = await _httpClient.PostAsJsonAsync("api/PersonalMedical/datagrid", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PersonalMedicalDataGridResult>(_jsonOptions);
                return Result<PersonalMedicalDataGridResult>.Success(result!);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for GetDataGrid: {StatusCode} - {Content}", response.StatusCode, errorContent);
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result<PersonalMedicalDataGridResult>.Failure(errorResponse?.Errors ?? new[] { "Eroare necunoscuta la obtinerea datelor" });
            }
            catch
            {
                return Result<PersonalMedicalDataGridResult>.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error in GetDataGridAsync");
            return Result<PersonalMedicalDataGridResult>.Failure("Eroare de conectare la server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetDataGridAsync");
            return Result<PersonalMedicalDataGridResult>.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<PersonalMedicalListDto>>> GetPagedAsync(PersonalMedicalSearchQuery query)
    {
        try
        {
            var queryParams = new List<string>();
            
            // Global search
            if (!string.IsNullOrWhiteSpace(query.Search))
                queryParams.Add($"search={Uri.EscapeDataString(query.Search)}");
            
            // High-level dropdown filters
            if (!string.IsNullOrWhiteSpace(query.Departament))
                queryParams.Add($"departament={Uri.EscapeDataString(query.Departament)}");
            
            if (!string.IsNullOrWhiteSpace(query.Pozitie))
                queryParams.Add($"pozitie={Uri.EscapeDataString(query.Pozitie)}");
            
            if (query.EsteActiv.HasValue)
                queryParams.Add($"esteActiv={query.EsteActiv.Value}");

            // Per-column filters (advanced)
            if (!string.IsNullOrWhiteSpace(query.Nume))
                queryParams.Add($"nume={Uri.EscapeDataString(query.Nume)}");
            if (!string.IsNullOrWhiteSpace(query.Prenume))
                queryParams.Add($"prenume={Uri.EscapeDataString(query.Prenume)}");
            if (!string.IsNullOrWhiteSpace(query.Specializare))
                queryParams.Add($"specializare={Uri.EscapeDataString(query.Specializare)}");
            if (!string.IsNullOrWhiteSpace(query.NumarLicenta))
                queryParams.Add($"numarLicenta={Uri.EscapeDataString(query.NumarLicenta)}");
            if (!string.IsNullOrWhiteSpace(query.Telefon))
                queryParams.Add($"telefon={Uri.EscapeDataString(query.Telefon)}");
            if (!string.IsNullOrWhiteSpace(query.Email))
                queryParams.Add($"email={Uri.EscapeDataString(query.Email)}");

            // Multi-select lists: repeat the parameter for model binding to IEnumerable<string>
            if (query.Specializari != null)
            {
                foreach (var v in query.Specializari.Where(s => !string.IsNullOrWhiteSpace(s)))
                    queryParams.Add($"specializari={Uri.EscapeDataString(v)}");
            }
            if (query.Departamente != null)
            {
                foreach (var v in query.Departamente.Where(s => !string.IsNullOrWhiteSpace(s)))
                    queryParams.Add($"departamente={Uri.EscapeDataString(v)}");
            }
            if (query.Pozitii != null)
            {
                foreach (var v in query.Pozitii.Where(s => !string.IsNullOrWhiteSpace(s)))
                    queryParams.Add($"pozitii={Uri.EscapeDataString(v)}");
            }

            // Paging
            queryParams.Add($"page={query.Page}");
            queryParams.Add($"pageSize={query.PageSize}");
            
            // Sort normalization: Radzen provides "Column asc" or "Column desc"; normalize to "Column:asc|desc"
            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                var sort = query.Sort.Trim();
                var firstPart = sort.Split(',')[0].Trim();
                string orderCol;
                string orderDir = "asc";

                var parts = firstPart.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length >= 1)
                {
                    orderCol = parts[0];
                    if (parts.Length >= 2)
                    {
                        var dir = parts[1].ToLowerInvariant();
                        orderDir = dir == "desc" ? "desc" : "asc";
                    }
                    queryParams.Add($"sort={Uri.EscapeDataString($"{orderCol}:{orderDir}")}");
                }
                else
                {
                    // fallback to raw
                    queryParams.Add($"sort={Uri.EscapeDataString(sort)}");
                }
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            var url = $"api/PersonalMedical{queryString}";

            _logger.LogInformation("Calling API: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<PersonalMedicalListDto>>(_jsonOptions);
                return Result<PagedResult<PersonalMedicalListDto>>.Success(pagedResult!);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for GetPaged: {StatusCode} - {Content}", response.StatusCode, errorContent);
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result<PagedResult<PersonalMedicalListDto>>.Failure(errorResponse?.Errors ?? new[] { "Eroare necunoscuta la obtinerea datelor" });
            }
            catch
            {
                return Result<PagedResult<PersonalMedicalListDto>>.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error in GetPagedAsync");
            return Result<PagedResult<PersonalMedicalListDto>>.Failure("Eroare de conectare la server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPagedAsync");
            return Result<PagedResult<PersonalMedicalListDto>>.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PersonalMedicalGroupAggregateDto>>> GetGroupAggregatesAsync(PersonalMedicalGroupAggregateQuery query)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(query.GroupBy))
                queryParams.Add($"groupBy={Uri.EscapeDataString(query.GroupBy)}");
            if (!string.IsNullOrWhiteSpace(query.Search)) queryParams.Add($"search={Uri.EscapeDataString(query.Search)}");
            if (!string.IsNullOrWhiteSpace(query.Departament)) queryParams.Add($"departament={Uri.EscapeDataString(query.Departament)}");
            if (!string.IsNullOrWhiteSpace(query.Pozitie)) queryParams.Add($"pozitie={Uri.EscapeDataString(query.Pozitie)}");
            if (query.EsteActiv.HasValue) queryParams.Add($"esteActiv={query.EsteActiv.Value}");
            if (!string.IsNullOrWhiteSpace(query.Nume)) queryParams.Add($"nume={Uri.EscapeDataString(query.Nume)}");
            if (!string.IsNullOrWhiteSpace(query.Prenume)) queryParams.Add($"prenume={Uri.EscapeDataString(query.Prenume)}");
            if (!string.IsNullOrWhiteSpace(query.Specializare)) queryParams.Add($"specializare={Uri.EscapeDataString(query.Specializare)}");
            if (!string.IsNullOrWhiteSpace(query.NumarLicenta)) queryParams.Add($"numarLicenta={Uri.EscapeDataString(query.NumarLicenta)}");
            if (!string.IsNullOrWhiteSpace(query.Telefon)) queryParams.Add($"telefon={Uri.EscapeDataString(query.Telefon)}");
            if (!string.IsNullOrWhiteSpace(query.Email)) queryParams.Add($"email={Uri.EscapeDataString(query.Email)}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            var url = $"api/PersonalMedical/group-aggregates{queryString}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<Result<IEnumerable<PersonalMedicalGroupAggregateDto>>>(_jsonOptions);
                return data!;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for GetGroupAggregates: {StatusCode} - {Content}", response.StatusCode, error);
            return Result<IEnumerable<PersonalMedicalGroupAggregateDto>>.Failure("Eroare la agregarea grupurilor");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGroupAggregatesAsync");
            return Result<IEnumerable<PersonalMedicalGroupAggregateDto>>.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result<PersonalMedicalDetailDto?>> GetByIdAsync(Guid personalId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/PersonalMedical/{personalId}");

            if (response.IsSuccessStatusCode)
            {
                var personalDetail = await response.Content.ReadFromJsonAsync<PersonalMedicalDetailDto>(_jsonOptions);
                return Result<PersonalMedicalDetailDto?>.Success(personalDetail);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Result<PersonalMedicalDetailDto?>.Success(null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for GetById {PersonalId}: {StatusCode} - {Content}", personalId, response.StatusCode, errorContent);
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result<PersonalMedicalDetailDto?>.Failure(errorResponse?.Errors ?? new[] { "Eroare la obtinerea detaliilor" });
            }
            catch
            {
                return Result<PersonalMedicalDetailDto?>.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetByIdAsync for {PersonalId}", personalId);
            return Result<PersonalMedicalDetailDto?>.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> CreateAsync(CreatePersonalMedicalRequest request)
    {
        try
        {
            Console.WriteLine($"[PersonalMedicalApiService] Calling CreateAsync API with request: {System.Text.Json.JsonSerializer.Serialize(request)}");
            Console.WriteLine($"[PersonalMedicalApiService] API URL: {_httpClient.BaseAddress}");
            
            var response = await _httpClient.PostAsJsonAsync("api/PersonalMedical", request, _jsonOptions);

            Console.WriteLine($"[PersonalMedicalApiService] API Response: StatusCode={response.StatusCode}, IsSuccess={response.IsSuccessStatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[PersonalMedicalApiService] Response content: {responseContent}");
                
                var createResult = await response.Content.ReadFromJsonAsync<Result<Guid>>(_jsonOptions);
                Console.WriteLine($"[PersonalMedicalApiService] Parsed result: IsSuccess={createResult?.IsSuccess}, Value={createResult?.Value}, Errors={createResult?.Errors?.Count ?? 0}");
                
                return createResult!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[PersonalMedicalApiService] API returned error for Create: {response.StatusCode} - {errorContent}");
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result<Guid>.Failure(errorResponse?.Errors ?? new[] { "Eroare la crearea personalului medical" });
            }
            catch
            {
                return Result<Guid>.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PersonalMedicalApiService] Error in CreateAsync: {ex.Message}");
            Console.WriteLine($"[PersonalMedicalApiService] Stack trace: {ex.StackTrace}");
            return Result<Guid>.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Guid personalId, UpdatePersonalMedicalRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/PersonalMedical/{personalId}", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var updateResult = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return updateResult!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for Update {PersonalId}: {StatusCode} - {Content}", personalId, response.StatusCode, errorContent);
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result.Failure(errorResponse?.Errors ?? new[] { "Eroare la actualizarea personalului medical" });
            }
            catch
            {
                return Result.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateAsync for {PersonalId}", personalId);
            return Result.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid personalId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/PersonalMedical/{personalId}");

            if (response.IsSuccessStatusCode)
            {
                var deleteResult = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return deleteResult!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for Delete {PersonalId}: {StatusCode} - {Content}", personalId, response.StatusCode, errorContent);
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result.Failure(errorResponse?.Errors ?? new[] { "Eroare la stergerea personalului medical" });
            }
            catch
            {
                return Result.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteAsync for {PersonalId}", personalId);
            return Result.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result> ToggleStatusAsync(Guid personalId)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"api/PersonalMedical/{personalId}/toggle-status", null);

            if (response.IsSuccessStatusCode)
            {
                var toggleResult = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return toggleResult!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for ToggleStatus {PersonalId}: {StatusCode} - {Content}", personalId, response.StatusCode, errorContent);
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result.Failure(errorResponse?.Errors ?? new[] { "Eroare la schimbarea statusului" });
            }
            catch
            {
                return Result.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ToggleStatusAsync for {PersonalId}", personalId);
            return Result.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PersonalMedicalListDto>>> GetActiveDoctorsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/PersonalMedical/active-doctors");

            if (response.IsSuccessStatusCode)
            {
                var doctorsResult = await response.Content.ReadFromJsonAsync<Result<IEnumerable<PersonalMedicalListDto>>>(_jsonOptions);
                return doctorsResult!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned error for GetActiveDoctors: {StatusCode} - {Content}", response.StatusCode, errorContent);
            
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                return Result<IEnumerable<PersonalMedicalListDto>>.Failure(errorResponse?.Errors ?? new[] { "Eroare la obtinerea listei de doctori" });
            }
            catch
            {
                return Result<IEnumerable<PersonalMedicalListDto>>.Failure($"Eroare API: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetActiveDoctorsAsync");
            return Result<IEnumerable<PersonalMedicalListDto>>.Failure($"Eroare neasteptata: {ex.Message}");
        }
    }

    private class ErrorResponse
    {
        public string[]? Errors { get; set; }
        public string? Message { get; set; }
    }
}