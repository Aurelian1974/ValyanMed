using Microsoft.JSInterop;
using System.Text.Json;
using Shared.DTOs.Authentication;

namespace Client.Services;

public interface ISimpleAuthService
{
    Task<bool> SaveAuthDataAsync(string token, AuthenticationResponse userInfo);
    Task<string?> GetTokenAsync();
    Task<AuthenticationResponse?> GetUserInfoAsync();
    Task ClearAuthDataAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class SimpleAuthService : ISimpleAuthService
{
    private readonly IJSRuntime _jsRuntime;
    
    // Use multiple keys for redundancy
    private readonly string[] TOKEN_KEYS = { "auth_token", "valyanmed_auth_token" };
    private readonly string[] USER_KEYS = { "auth_user", "valyanmed_user_info", "currentUser" };

    public SimpleAuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> SaveAuthDataAsync(string token, AuthenticationResponse userInfo)
    {
        try
        {
            Console.WriteLine($"SimpleAuthService: Saving auth data for user {userInfo.NumeUtilizator}");
            
            // Save token with multiple keys for redundancy
            foreach (var key in TOKEN_KEYS)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, token);
                Console.WriteLine($"Token saved with key: {key}");
            }
            
            // Save user info with multiple keys for redundancy
            var userJson = JsonSerializer.Serialize(userInfo);
            foreach (var key in USER_KEYS)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, userJson);
                Console.WriteLine($"User info saved with key: {key}");
            }
            
            // Verify saves worked
            var verifyToken = await GetTokenAsync();
            var verifyUser = await GetUserInfoAsync();
            
            bool success = !string.IsNullOrEmpty(verifyToken) && verifyUser != null;
            Console.WriteLine($"SimpleAuthService: Save verification = {success}");
            
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SimpleAuthService save error: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        // Try all possible token keys
        foreach (var key in TOKEN_KEYS)
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"SimpleAuthService: Token found with key {key}");
                    return token;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SimpleAuthService get token error for {key}: {ex.Message}");
            }
        }
        
        Console.WriteLine("SimpleAuthService: No token found in any key");
        return null;
    }

    public async Task<AuthenticationResponse?> GetUserInfoAsync()
    {
        // Try all possible user info keys
        foreach (var key in USER_KEYS)
        {
            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (!string.IsNullOrEmpty(userJson))
                {
                    var userInfo = JsonSerializer.Deserialize<AuthenticationResponse>(userJson);
                    if (userInfo != null)
                    {
                        Console.WriteLine($"SimpleAuthService: User info found with key {key} for {userInfo.NumeUtilizator}");
                        return userInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SimpleAuthService get user info error for {key}: {ex.Message}");
            }
        }
        
        Console.WriteLine("SimpleAuthService: No user info found in any key");
        return null;
    }

    public async Task ClearAuthDataAsync()
    {
        try
        {
            // Clear all possible keys
            var allKeys = TOKEN_KEYS.Concat(USER_KEYS).ToArray();
            
            foreach (var key in allKeys)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
                Console.WriteLine($"Cleared localStorage key: {key}");
            }
            
            Console.WriteLine("SimpleAuthService: All auth data cleared");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SimpleAuthService clear error: {ex.Message}");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            var userInfo = await GetUserInfoAsync();
            
            if (string.IsNullOrEmpty(token) || userInfo == null)
            {
                Console.WriteLine("SimpleAuthService: Not authenticated - missing token or user info");
                return false;
            }
                
            // Check if token is expired
            bool isExpired = userInfo.Expiration <= DateTime.UtcNow;
            if (isExpired)
            {
                Console.WriteLine("SimpleAuthService: Token expired, clearing data");
                await ClearAuthDataAsync();
                return false;
            }
            
            Console.WriteLine($"SimpleAuthService: User is authenticated - {userInfo.NumeUtilizator}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SimpleAuthService IsAuthenticated error: {ex.Message}");
            return false;
        }
    }
}