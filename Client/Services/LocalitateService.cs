// Client/Services/LocalitateService.cs
using System.Net.Http.Json;
using Shared.DTOs;

namespace Client.Services
{
    public class LocalitateService : ILocalitateService
    {
        private readonly HttpClient _http;

        public LocalitateService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<LocalitateDto>> GetByJudetAsync(int idJudet)
        {
            return await _http.GetFromJsonAsync<List<LocalitateDto>>($"api/localitate/by-judet/{idJudet}") ?? new();
        }
    }
}
