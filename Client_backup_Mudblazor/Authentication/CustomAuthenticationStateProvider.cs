using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Client.Services.Authentication;
using Shared.DTOs.Authentication;
using Microsoft.JSInterop;

namespace Client.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly IAuthenticationStateService _authStateService;
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;

    public CustomAuthenticationStateProvider(
        ITokenStorageService tokenStorage,
        IAuthenticationStateService authStateService,
        IJSRuntime jsRuntime)
    {
        _tokenStorage = tokenStorage;
        _authStateService = authStateService;
        _jsRuntime = jsRuntime;
        
        // Ascult? schimb?rile în starea de autentificare
        _authStateService.OnAuthenticationStateChanged += NotifyAuthenticationStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            Console.WriteLine("CustomAuthenticationStateProvider: Getting authentication state...");
            
            // Initialize authentication service if not done yet
            if (!_isInitialized)
            {
                Console.WriteLine("Initializing authentication state service...");
                await _authStateService.InitializeAsync();
                _isInitialized = true;
            }
            
            // Try multiple storage methods to find valid token
            var token = await GetTokenFromAnySource();
            var userInfo = await GetUserInfoFromAnySource();
            
            Console.WriteLine($"Final check - Token exists: {!string.IsNullOrEmpty(token)}");
            Console.WriteLine($"Final check - UserInfo exists: {userInfo != null}");

            if (string.IsNullOrEmpty(token) || userInfo == null)
            {
                Console.WriteLine("No token or userInfo - returning anonymous user");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Verific? expirarea token-ului
            if (userInfo.Expiration <= DateTime.UtcNow)
            {
                Console.WriteLine("Token expired - clearing auth state");
                await ClearAllAuthenticationDataAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            Console.WriteLine($"Creating authenticated user for: {userInfo.NumeUtilizator}");

            // Creeaz? claims din informa?iile utilizatorului
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                new(ClaimTypes.Name, userInfo.NumeUtilizator),
                new(ClaimTypes.Email, userInfo.Email),
                new("nume_complet", userInfo.NumeComplet),
                new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            Console.WriteLine($"Authentication successful - IsAuthenticated: {user.Identity?.IsAuthenticated}");
            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting authentication state: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private async Task<string?> GetTokenFromAnySource()
    {
        // Try service-based storage first
        try
        {
            var token = await _tokenStorage.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token found via TokenStorageService");
                return token;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TokenStorageService failed: {ex.Message}");
        }

        // Try direct localStorage access with multiple keys
        var keys = new[] { "valyanmed_auth_token", "auth_token" };
        foreach (var key in keys)
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"Token found via direct localStorage with key: {key}");
                    return token;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Direct localStorage read failed for {key}: {ex.Message}");
            }
        }

        return null;
    }

    private async Task<AuthenticationResponse?> GetUserInfoFromAnySource()
    {
        // Try service-based storage first
        try
        {
            var userInfo = await _tokenStorage.GetUserInfoAsync();
            if (userInfo != null)
            {
                Console.WriteLine("UserInfo found via TokenStorageService");
                return userInfo;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TokenStorageService getUserInfo failed: {ex.Message}");
        }

        // Try direct localStorage access with multiple keys
        var keys = new[] { "valyanmed_user_info", "currentUser", "auth_user" };
        foreach (var key in keys)
        {
            try
            {
                var userInfoJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (!string.IsNullOrEmpty(userInfoJson))
                {
                    var userInfo = JsonSerializer.Deserialize<AuthenticationResponse>(userInfoJson);
                    if (userInfo != null)
                    {
                        Console.WriteLine($"UserInfo found via direct localStorage with key: {key}");
                        return userInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Direct localStorage userInfo read failed for {key}: {ex.Message}");
            }
        }

        return null;
    }

    public async Task MarkUserAsAuthenticatedAsync(string token, AuthenticationResponse userInfo)
    {
        Console.WriteLine($"MarkUserAsAuthenticatedAsync called for user: {userInfo.NumeUtilizator}");
        
        try
        {
            // Save through ALL available methods for maximum reliability
            
            // 1. Service-based storage
            await _tokenStorage.SetTokenAsync(token);
            await _tokenStorage.SetUserInfoAsync(userInfo);
            await _authStateService.SetCurrentUserAsync(userInfo);
            Console.WriteLine("Data saved via services");
            
            // 2. Direct localStorage with multiple keys for redundancy
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "valyanmed_auth_token", token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "auth_token", token);
            
            var userInfoJson = JsonSerializer.Serialize(userInfo);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "valyanmed_user_info", userInfoJson);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", userInfoJson);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "auth_user", userInfoJson);
            Console.WriteLine("Data saved via direct localStorage with multiple keys");
            
            // 3. Verify saves worked
            var verifyToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "auth_token");
            var verifyUser = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "auth_user");
            Console.WriteLine($"Verification: Token saved = {!string.IsNullOrEmpty(verifyToken)}, User saved = {!string.IsNullOrEmpty(verifyUser)}");
            
            // Force refresh the authentication state
            _isInitialized = true;
            
            // Notific? schimbarea st?rii
            NotifyAuthenticationStateChanged();
            
            Console.WriteLine("Authentication state change notified - user should now be authenticated");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MarkUserAsAuthenticatedAsync: {ex.Message}");
        }
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        Console.WriteLine("MarkUserAsLoggedOutAsync called");
        await ClearAllAuthenticationDataAsync();
    }

    private async Task ClearAllAuthenticationDataAsync()
    {
        try
        {
            // Clear through services
            await _tokenStorage.RemoveTokenAsync();
            await _tokenStorage.RemoveUserInfoAsync();
            await _authStateService.ClearCurrentUserAsync();
            
            // Clear all possible localStorage keys
            var keys = new[] { 
                "valyanmed_auth_token", "valyanmed_user_info", "currentUser", 
                "auth_token", "auth_user" 
            };
            
            foreach (var key in keys)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            
            Console.WriteLine("Auth data cleared successfully through all methods");
            
            _isInitialized = false;
            NotifyAuthenticationStateChanged();
            
            Console.WriteLine("Logout state change notified");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing authentication: {ex.Message}");
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user") }, "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private void NotifyAuthenticationStateChanged()
    {
        Console.WriteLine("NotifyAuthenticationStateChanged triggered");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}