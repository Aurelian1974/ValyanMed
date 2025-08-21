using Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace Client.Services
{
    public class MaterialSanitarClient : IMaterialSanitarClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public MaterialSanitarClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<List<MaterialSanitarDTO>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/materialeSanitare");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<List<MaterialSanitarDTO>>(_jsonOptions);
                return result ?? new List<MaterialSanitarDTO>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<MaterialSanitarDTO>> GetPagedAsync(string? search, string? categorie, int page, int pageSize, string? sort)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(categorie) && categorie != "toate") queryParams.Add($"categorie={Uri.EscapeDataString(categorie)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");
                if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/materialeSanitare/paged?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PagedResult<MaterialSanitarDTO>>(_jsonOptions);
                return result ?? new PagedResult<MaterialSanitarDTO>();
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

        public async Task<MaterialSanitarDTO> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/materialeSanitare/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MaterialSanitarDTO>(_jsonOptions);
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

        public async Task<int> CreateAsync(CreateMaterialSanitarDTO dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/materialeSanitare", dto, _jsonOptions);
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

        public async Task<bool> UpdateAsync(UpdateMaterialSanitarDTO dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/materialeSanitare/{dto.Id}", dto, _jsonOptions);
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
                var response = await _httpClient.DeleteAsync($"api/materialeSanitare/{id}");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare la comunicarea cu serverul: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<MaterialSanitarDTO>> GetAllGroupedAsync(string? search, string? categorie, string? groupBy, string? sort)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(categorie) && categorie != "toate") queryParams.Add($"categorie={Uri.EscapeDataString(categorie)}");
                if (!string.IsNullOrEmpty(groupBy)) queryParams.Add($"groupBy={Uri.EscapeDataString(groupBy)}");
                if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/materialeSanitare/grouped?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<IEnumerable<MaterialSanitarDTO>>(_jsonOptions);
                return result ?? new List<MaterialSanitarDTO>();
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

        public async Task<PagedResult<MaterialSanitarDTO>> GetPagedGroupedAsync(string? search, string? categorie, string? groupBy, string? sort, int page, int pageSize)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(categorie) && categorie != "toate") queryParams.Add($"categorie={Uri.EscapeDataString(categorie)}");
                if (!string.IsNullOrEmpty(groupBy)) queryParams.Add($"groupBy={Uri.EscapeDataString(groupBy)}");
                if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={Uri.EscapeDataString(sort)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/materialeSanitare/paged-grouped?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PagedResult<MaterialSanitarDTO>>(_jsonOptions);
                return result ?? new PagedResult<MaterialSanitarDTO>();
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