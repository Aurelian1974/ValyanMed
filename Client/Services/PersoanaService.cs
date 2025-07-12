using Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Client.Services
{
    public class PersoanaService : IPersoanaService
    {
        private readonly HttpClient _httpClient;

        public PersoanaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PersoanaModel>> GetAllPersonalAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<PersoanaModel>>("api/personal");
                return response ?? new List<PersoanaModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching personal data: {ex.Message}");
                throw;
            }
        }

        public async Task<PersoanaModel> GetPersoanaByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PersoanaModel>($"api/personal/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching persona with ID {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CreatePersoanaAsync(PersoanaModel persoana)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/personal", persoana);
                response.EnsureSuccessStatusCode();

                var createdPersoana = await response.Content.ReadFromJsonAsync<PersoanaModel>();
                return createdPersoana?.Id ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating persona: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdatePersoanaAsync(PersoanaModel persoana)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/personal", persoana);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating persona: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeletePersoanaAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/personal/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting persona with ID {id}: {ex.Message}");
                throw;
            }
        }
    }
}