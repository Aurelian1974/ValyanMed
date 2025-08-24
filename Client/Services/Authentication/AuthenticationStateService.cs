using Shared.DTOs.Authentication;
using Blazored.LocalStorage;

namespace Client.Services.Authentication;

public interface IAuthenticationStateService
{
    Task<AuthenticationResponse?> GetCurrentUserAsync();
    Task SetCurrentUserAsync(AuthenticationResponse user);
    Task ClearCurrentUserAsync();
    Task InitializeAsync();
    event Action? OnAuthenticationStateChanged;
}

public class AuthenticationStateService : IAuthenticationStateService
{
    private readonly ILocalStorageService _localStorage;
    private AuthenticationResponse? _currentUser;

    public AuthenticationStateService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public event Action? OnAuthenticationStateChanged;

    public async Task InitializeAsync()
    {
        try
        {
            var storedUser = await _localStorage.GetItemAsync<AuthenticationResponse>("currentUser");
            _currentUser = storedUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing auth state: {ex.Message}");
            _currentUser = null;
        }
    }

    public async Task<AuthenticationResponse?> GetCurrentUserAsync()
    {
        try
        {
            if (_currentUser == null)
            {
                _currentUser = await _localStorage.GetItemAsync<AuthenticationResponse>("currentUser");
            }
            return _currentUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting current user: {ex.Message}");
            return null;
        }
    }

    public async Task SetCurrentUserAsync(AuthenticationResponse user)
    {
        try
        {
            _currentUser = user;
            await _localStorage.SetItemAsync("currentUser", user);
            OnAuthenticationStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting current user: {ex.Message}");
        }
    }

    public async Task ClearCurrentUserAsync()
    {
        try
        {
            _currentUser = null;
            await _localStorage.RemoveItemAsync("currentUser");
            OnAuthenticationStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing current user: {ex.Message}");
        }
    }
}