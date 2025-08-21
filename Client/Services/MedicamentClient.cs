using System.Net.Http.Json;
using System.Text;
using Shared.DTOs;

namespace Client.Services
{
    public class MedicamentClient : IMedicamentClient
    {
        private readonly HttpClient _http;
        public MedicamentClient(HttpClient http) => _http = http;

        public async Task<List<MedicamentDTO>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<MedicamentDTO>>("api/medicamente") ?? new();

        public async Task<MedicamentDTO> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<MedicamentDTO>($"api/medicamente/{id}");

        public async Task<int> CreateAsync(CreateMedicamentDTO dto)
        {
            var resp = await _http.PostAsJsonAsync("api/medicamente", dto);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> UpdateAsync(UpdateMedicamentDTO dto)
        {
            try
            {
                var medicamentId = dto.MedicamentID;
                var url = $"api/medicamente/{medicamentId}";
                
                Console.WriteLine($"MedicamentClient.UpdateAsync: About to PUT to {url}");
                Console.WriteLine($"DTO: ID={dto.MedicamentID}, Nume={dto.Nume}");
                Console.WriteLine($"Base address: {_http.BaseAddress}");
                Console.WriteLine($"Full URL: {_http.BaseAddress}{url}");
                
                var resp = await _http.PutAsJsonAsync(url, dto);
                
                Console.WriteLine($"Response status: {resp.StatusCode} ({(int)resp.StatusCode})");
                
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response content: {errorContent}");
                    
                    // If it's a service-level failure (404 with success:false), it might still be a logical success
                    if (resp.StatusCode == System.Net.HttpStatusCode.NotFound && errorContent.Contains("\"success\":false"))
                    {
                        Console.WriteLine("Treating service-level NotFound as update failure");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Success response received");
                }
                
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/medicamente/{id}");
            return resp.IsSuccessStatusCode;
        }

        public async Task<PagedResult<MedicamentDTO>> GetPagedAsync(string? search, string? status, int page, int pageSize, string? sort, string? groupBy = null)
        {
            var sb = new StringBuilder("api/medicamente/paged?");
            if (!string.IsNullOrWhiteSpace(search)) sb.Append("search=").Append(Uri.EscapeDataString(search)).Append('&');
            if (!string.IsNullOrWhiteSpace(status)) sb.Append("status=").Append(Uri.EscapeDataString(status)).Append('&');
            sb.Append("page=").Append(page).Append('&');
            sb.Append("pageSize=").Append(pageSize).Append('&');
            if (!string.IsNullOrWhiteSpace(sort)) sb.Append("sort=").Append(Uri.EscapeDataString(sort)).Append('&');
            if (!string.IsNullOrWhiteSpace(groupBy)) sb.Append("groupBy=").Append(Uri.EscapeDataString(groupBy)).Append('&');
            var url = sb.ToString().TrimEnd('&', '?');

            return await _http.GetFromJsonAsync<PagedResult<MedicamentDTO>>(url) ?? new PagedResult<MedicamentDTO>();
        }

        public async Task<IEnumerable<MedicamentDTO>> GetAllGroupedAsync(string? search, string? status, string? groupBy, string? sort)
        {
            var sb = new StringBuilder("api/medicamente/grouped?");
            if (!string.IsNullOrWhiteSpace(search)) sb.Append("search=").Append(Uri.EscapeDataString(search)).Append('&');
            if (!string.IsNullOrWhiteSpace(status)) sb.Append("status=").Append(Uri.EscapeDataString(status)).Append('&');
            if (!string.IsNullOrWhiteSpace(groupBy)) sb.Append("groupBy=").Append(Uri.EscapeDataString(groupBy)).Append('&');
            if (!string.IsNullOrWhiteSpace(sort)) sb.Append("sort=").Append(Uri.EscapeDataString(sort)).Append('&');
            var url = sb.ToString().TrimEnd('&', '?');

            return await _http.GetFromJsonAsync<IEnumerable<MedicamentDTO>>(url) ?? new List<MedicamentDTO>();
        }
    }
}
