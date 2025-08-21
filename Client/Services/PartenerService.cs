using Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Services
{
    public interface IPartenerService
    {
        Task<List<PartenerDTO>> GetAllParteneriAsync();
        Task<PagedResult<PartenerDTO>> GetPagedParteneriAsync(string? search = null, string? judet = null, int page = 1, int pageSize = 25, string? sort = null);
        Task<PartenerDTO?> GetPartenerByIdAsync(int id);
        Task<int> CreatePartenerAsync(CreatePartenerDTO createDto);
        Task<bool> UpdatePartenerAsync(UpdatePartenerDTO updateDto);
        Task<(bool Success, string ErrorMessage)> DeletePartenerAsync(int id);
    }

    public class PartenerService : IPartenerService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public PartenerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<List<PartenerDTO>> GetAllParteneriAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/parteneri");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<List<PartenerDTO>>(_jsonOptions);
                return result ?? new List<PartenerDTO>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea r?spunsului: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<PartenerDTO>> GetPagedParteneriAsync(string? search = null, string? judet = null, int page = 1, int pageSize = 25, string? sort = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(judet)) queryParams.Add($"judet={Uri.EscapeDataString(judet)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");
                if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/parteneri/paged?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PagedResult<PartenerDTO>>(_jsonOptions);
                return result ?? new PagedResult<PartenerDTO>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea r?spunsului: {ex.Message}", ex);
            }
        }

        public async Task<PartenerDTO?> GetPartenerByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/parteneri/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PartenerDTO>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea r?spunsului: {ex.Message}", ex);
            }
        }

        public async Task<int> CreatePartenerAsync(CreatePartenerDTO createDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/parteneri", createDto, _jsonOptions);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return Convert.ToInt32(result?.GetProperty("id").GetInt32() ?? 0);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea r?spunsului: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePartenerAsync(UpdatePartenerDTO updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/parteneri/{updateDto.PartenerId}", updateDto, _jsonOptions);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return !string.IsNullOrEmpty(result);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeletePartenerAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/parteneri/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Eroare la ?tergere: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Eroare la comunicarea cu serverul: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Eroare nea?teptat?: {ex.Message}");
            }
        }
    }
}