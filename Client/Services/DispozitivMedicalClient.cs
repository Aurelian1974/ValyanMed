using Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Services
{
    public interface IDispozitivMedicalClient
    {
        Task<PagedResult<DispozitivMedicalDTO>> GetPagedAsync(string? search, string? categorie, string? clasaRisc, int page, int pageSize, string? sort);
        Task<DispozitivMedicalDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateDispozitivMedicalDTO dto);
        Task<bool> UpdateAsync(UpdateDispozitivMedicalDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<DispozitivMedicalDTO>> GetAllGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort);
        Task<PagedResult<DispozitivMedicalDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort, int page, int pageSize);
    }

    public class DispozitivMedicalClient : IDispozitivMedicalClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DispozitivMedicalClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<PagedResult<DispozitivMedicalDTO>> GetPagedAsync(string? search, string? categorie, string? clasaRisc, int page, int pageSize, string? sort)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(categorie) && categorie != "toate") queryParams.Add($"categorie={Uri.EscapeDataString(categorie)}");
                if (!string.IsNullOrEmpty(clasaRisc) && clasaRisc != "toate") queryParams.Add($"clasaRisc={Uri.EscapeDataString(clasaRisc)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");
                if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/dispozitiveMedicale/paged?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PagedResult<DispozitivMedicalDTO>>(_jsonOptions);
                return result ?? new PagedResult<DispozitivMedicalDTO>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea raspunsului: {ex.Message}", ex);
            }
        }

        public async Task<DispozitivMedicalDTO?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/dispozitiveMedicale/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<DispozitivMedicalDTO>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea raspunsului: {ex.Message}", ex);
            }
        }

        public async Task<int> CreateAsync(CreateDispozitivMedicalDTO dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dispozitiveMedicale", dto, _jsonOptions);
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
                throw new Exception($"Eroare la procesarea raspunsului: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(UpdateDispozitivMedicalDTO dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/dispozitiveMedicale/{dto.Id}", dto, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/dispozitiveMedicale/{id}");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<DispozitivMedicalDTO>> GetAllGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(categorie) && categorie != "toate") queryParams.Add($"categorie={Uri.EscapeDataString(categorie)}");
                if (!string.IsNullOrEmpty(clasaRisc) && clasaRisc != "toate") queryParams.Add($"clasaRisc={Uri.EscapeDataString(clasaRisc)}");
                if (!string.IsNullOrEmpty(groupBy)) queryParams.Add($"groupBy={Uri.EscapeDataString(groupBy)}");
                if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/dispozitiveMedicale/grouped?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<IEnumerable<DispozitivMedicalDTO>>(_jsonOptions);
                return result ?? new List<DispozitivMedicalDTO>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea raspunsului: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<DispozitivMedicalDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? clasaRisc, string? groupBy, string? sort, int page, int pageSize)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(categorie) && categorie != "toate") queryParams.Add($"categorie={Uri.EscapeDataString(categorie)}");
                if (!string.IsNullOrEmpty(clasaRisc) && clasaRisc != "toate") queryParams.Add($"clasaRisc={Uri.EscapeDataString(clasaRisc)}");
                if (!string.IsNullOrEmpty(groupBy)) queryParams.Add($"groupBy={Uri.EscapeDataString(groupBy)}");
                if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/dispozitiveMedicale/paged-grouped?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PagedResult<DispozitivMedicalDTO>>(_jsonOptions);
                return result ?? new PagedResult<DispozitivMedicalDTO>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Eroare la procesarea raspunsului: {ex.Message}", ex);
            }
        }
    }
}