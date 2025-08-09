using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Client.Models;

namespace Client.Services
{
    public class AuthService : AuthenticationStateProvider, IAuthService, ILogoutService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient http, IJSRuntime jsRuntime)
        {
            _http = http;
            _jsRuntime = jsRuntime;
        }

        // Add this method - required by AuthenticationStateProvider
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            
            if (string.IsNullOrEmpty(token))
            {
                // Not authenticated
                return new AuthenticationState(new ClaimsPrincipal());
            }
            
            // Get the user information from localStorage
            var userName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userName");
            
            // Create a claims identity
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName ?? string.Empty)
                // Add more claims as needed (roles, etc.)
            }, "apiauth_type");
            
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
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

                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, result.NumeUtilizator)
                    }, "apiauth_type")))));

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

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userName");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userFullName");

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
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