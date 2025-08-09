using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
                var responseContent = await response.Content.ReadFromJsonAsync<dynamic>();
                return 1; // Assuming success means 1 record was inserted
            }
            
            throw new Exception("Failed to create utilizator");
        }

        public async Task<bool> UpdateUtilizatorAsync(UpdateUtilizatorDTO utilizator)
        {
            var response = await _httpClient.PutAsJsonAsync("api/utilizator", utilizator);
            return response.IsSuccessStatusCode;
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
                        ? "Acest utilizator nu poate fi șters deoarece este asociat cu alte înregistrări."
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
