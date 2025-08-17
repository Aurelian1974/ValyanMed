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
            var resp = await _http.PutAsJsonAsync("api/medicamente", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/medicamente/{id}");
            return resp.IsSuccessStatusCode;
        }

        public async Task<PagedResult<MedicamentDTO>> GetPagedAsync(string? search, string? status, int page, int pageSize, string? sort)
        {
            var sb = new StringBuilder("api/medicamente/paged?");
            if (!string.IsNullOrWhiteSpace(search)) sb.Append("search=").Append(Uri.EscapeDataString(search)).Append('&');
            if (!string.IsNullOrWhiteSpace(status)) sb.Append("status=").Append(Uri.EscapeDataString(status)).Append('&');
            sb.Append("page=").Append(page).Append('&');
            sb.Append("pageSize=").Append(pageSize).Append('&');
            if (!string.IsNullOrWhiteSpace(sort)) sb.Append("sort=").Append(Uri.EscapeDataString(sort)).Append('&');
            var url = sb.ToString().TrimEnd('&', '?');

            return await _http.GetFromJsonAsync<PagedResult<MedicamentDTO>>(url) ?? new PagedResult<MedicamentDTO>();
        }
    }
}
