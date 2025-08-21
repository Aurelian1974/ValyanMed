using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Services
{
    public class UtilizatorService : IUtilizatorService
    {
        private readonly HttpClient _httpClient;

        public UtilizatorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UtilizatorDTO>> GetAllUtilizatoriAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<UtilizatorDTO>>("api/utilizator") ?? new List<UtilizatorDTO>();
        }

        public async Task<UtilizatorDTO> GetUtilizatorByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<UtilizatorDTO>($"api/utilizator/{id}");
        }

        public async Task<int> CreateUtilizatorAsync(CreateUtilizatorDTO utilizator)
        {
            var response = await _httpClient.PostAsJsonAsync("api/utilizator/register", utilizator);
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var doc = await JsonDocument.ParseAsync(stream);
                    if (doc.RootElement.TryGetProperty("Id", out var idProp) && idProp.TryGetInt32(out var id))
                        return id;
                }
                catch { /* ignore and fall back */ }
                return 1; // fallback for older API response without Id
            }
            
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create utilizator: {response.StatusCode} {err}");
        }

        public async Task<bool> UpdateUtilizatorAsync(UpdateUtilizatorDTO utilizator)
        {
            try 
            {
                var response = await _httpClient.PutAsJsonAsync($"api/utilizator/{utilizator.Id}", utilizator);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var doc = await JsonDocument.ParseAsync(stream);
                    var message = doc.RootElement.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "Eroare necunoscuta";
                    throw new Exception(message);
                }
                catch (JsonException)
                {
                    throw new Exception($"Eroare HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Eroare de conexiune: {ex.Message}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DeleteResult> DeleteUtilizatorAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/utilizator/{id}");
                
                if (response.IsSuccessStatusCode)
                    return new DeleteResult { Success = true };
                    
                var errorContent = await response.Content.ReadAsStringAsync();
                return new DeleteResult 
                { 
                    Success = false, 
                    ErrorMessage = response.StatusCode == System.Net.HttpStatusCode.Conflict
                        ? "Acest utilizator nu poate fi șters deoarece este asociat cu alte înregistrari."
                        : $"Eroare la ștergere: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                return new DeleteResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
