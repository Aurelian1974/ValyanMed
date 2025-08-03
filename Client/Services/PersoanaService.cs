using Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs;

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

        public async Task<PersoanaDTO> CreatePersoanaAsync(CreatePersoanaDTO persoana)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/personal", persoana);
                response.EnsureSuccessStatusCode();
                var createdPersoana = await response.Content.ReadFromJsonAsync<PersoanaDTO>();
                return createdPersoana;
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

        public class DeleteResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }

        public async Task<DeleteResult> DeletePersoanaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/personal/{id}");
            if (response.IsSuccessStatusCode)
                return new DeleteResult { Success = true };

            var error = await response.Content.ReadAsStringAsync();
            return new DeleteResult { Success = false, ErrorMessage = error };
        }

        private string deleteError;

        private async Task DeletePersoana(int id)
        {
            var result = await DeletePersoanaAsync(id);
            if (!result.Success)
            {
                deleteError = result.ErrorMessage;
            }
            else
            {
                deleteError = null;
            }
        }

        public string RenderDeleteError()
        {
            if (!string.IsNullOrEmpty(deleteError))
            {
                return $"<div class=\"alert alert-danger\">{deleteError}</div>";
            }
            return string.Empty;
        }
    }
}