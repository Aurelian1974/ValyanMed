using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using ValyanMed.Client.Models;

namespace ValyanMed.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _jsRuntime;
        
        public AuthService(HttpClient http, IJSRuntime jsRuntime)
        {
            _http = http;
            _jsRuntime = jsRuntime;
        }
        
        public async Task<(bool Success, string Message)> Register(UtilizatorRegisterDto model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/utilizator", model);
                
                if (response.IsSuccessStatusCode)
                    return (true, "Înregistrarea a fost realizată cu succes!");
                    
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"A apărut o eroare: {ex.Message}");
            }
        }
        
        public async Task<(bool Success, string Message, string Token)> Login(string numeUtilizatorSauEmail, string parola)
        {
            try
            {
                var loginModel = new LoginDto 
                { 
                    NumeUtilizatorSauEmail = numeUtilizatorSauEmail, 
                    Parola = parola 
                };
                
                var response = await _http.PostAsJsonAsync("api/utilizator/login", loginModel);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                    
                    // Salvează token-ul în localStorage
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userName", result.NumeUtilizator);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userFullName", result.NumeComplet);
                    
                    return (true, "Autentificare reușită", result.Token);
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error, null);
            }
            catch (Exception ex)
            {
                return (false, $"A apărut o eroare: {ex.Message}", null);
            }
        }
        
        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userName");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userFullName");
        }
    }
    
    // Clasa DTO pentru răspunsul de autentificare
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string NumeUtilizator { get; set; }
        public string NumeComplet { get; set; }
    }
}