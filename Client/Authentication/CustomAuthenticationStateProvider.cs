using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Client.Services.Authentication;

namespace Client.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly IAuthenticationStateService _authStateService;

    public CustomAuthenticationStateProvider(
        ITokenStorageService tokenStorage,
        IAuthenticationStateService authStateService)
    {
        _tokenStorage = tokenStorage;
        _authStateService = authStateService;
        
        // Ascult? schimb?rile în starea de autentificare
        _authStateService.OnAuthenticationStateChanged += NotifyAuthenticationStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenStorage.GetTokenAsync();
            var userInfo = await _tokenStorage.GetUserInfoAsync();

            if (string.IsNullOrEmpty(token) || userInfo == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Verific? expirarea token-ului
            if (userInfo.Expiration <= DateTime.UtcNow)
            {
                await _authStateService.ClearCurrentUserAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Creeaz? claims din informa?iile utilizatorului
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                new(ClaimTypes.Name, userInfo.NumeUtilizator),
                new(ClaimTypes.Email, userInfo.Email),
                new("nume_complet", userInfo.NumeComplet)
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            // Adaugat handling pentru a preveni crash-urile
            Console.WriteLine($"Error getting authentication state: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
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
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}