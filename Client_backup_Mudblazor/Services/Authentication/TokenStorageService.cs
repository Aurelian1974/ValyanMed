using Blazored.LocalStorage;
using Shared.DTOs.Authentication;
using System.Text.Json;

namespace Client.Services.Authentication;

public interface ITokenStorageService
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task RemoveTokenAsync();
    Task<AuthenticationResponse?> GetUserInfoAsync();
    Task SetUserInfoAsync(AuthenticationResponse userInfo);
    Task RemoveUserInfoAsync();
}

public class TokenStorageService : ITokenStorageService
{
    private readonly ILocalStorageService _localStorage;
    private const string TokenKey = "valyanmed_auth_token";
    private const string UserKey = "valyanmed_user_info";

    public TokenStorageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<string>(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        await _localStorage.SetItemAsync(TokenKey, token);
    }

    public async Task RemoveTokenAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
    }

    public async Task<AuthenticationResponse?> GetUserInfoAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<AuthenticationResponse>(UserKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetUserInfoAsync(AuthenticationResponse userInfo)
    {
        await _localStorage.SetItemAsync(UserKey, userInfo);
    }

    public async Task RemoveUserInfoAsync()
    {
        await _localStorage.RemoveItemAsync(UserKey);
    }
}