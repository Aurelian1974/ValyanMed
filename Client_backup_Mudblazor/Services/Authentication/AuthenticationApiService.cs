using System.Net.Http.Json;
using System.Text.Json;
using Shared.Common;
using Shared.DTOs.Authentication;
using Client.Services;

namespace Client.Services.Authentication;

public interface IAuthenticationApiService
{
    Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request);
    Task<Result> LogoutAsync();
    Task<Result<bool>> ValidateTokenAsync(string token);
}

public class AuthenticationApiService : IAuthenticationApiService
{
    private readonly HttpClient _httpClient;
    private readonly IJsonService _jsonService;

    public AuthenticationApiService(HttpClient httpClient, IJsonService jsonService)
    {
        _httpClient = httpClient;
        _jsonService = jsonService;
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonService.Options);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = _jsonService.Deserialize<ApiResponse<AuthenticationResponse>>(content);
                
                if (result?.Data != null)
                {
                    return Result<AuthenticationResponse>.Success(result.Data, result.Message);
                }
                
                return Result<AuthenticationResponse>.Failure("R?spuns invalid de la server");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResult = _jsonService.Deserialize<ApiErrorResponse>(errorContent);
            
            return Result<AuthenticationResponse>.Failure(errorResult?.Errors ?? new[] { "Eroare la autentificare" });
        }
        catch (HttpRequestException ex)
        {
            return Result<AuthenticationResponse>.Failure($"Eroare de conexiune: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<AuthenticationResponse>.Failure($"Eroare la procesarea r?spunsului: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<AuthenticationResponse>.Failure($"Eroare nea?teptat?: {ex.Message}");
        }
    }

    public async Task<Result> LogoutAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("api/auth/logout", null);
            
            if (response.IsSuccessStatusCode)
            {
                return Result.Success("Logout realizat cu succes");
            }
            
            return Result.Failure("Eroare la logout");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Eroare la logout: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/validate-token", token, _jsonService.Options);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = _jsonService.Deserialize<ApiResponse<bool>>(content);
                
                return Result<bool>.Success(result?.Data ?? false);
            }
            
            return Result<bool>.Success(false);
        }
        catch
        {
            return Result<bool>.Success(false);
        }
    }

    private class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    private class ApiErrorResponse
    {
        public string[]? Errors { get; set; }
        public string? Message { get; set; }
    }
}