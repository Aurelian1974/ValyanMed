using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Client.Authentication;
using Client.Services.Authentication;
using global::Shared.DTOs.Authentication;
using global::Shared.Models.Authentication;

namespace Client.Pages.Authentication;

public class LoginBase : ComponentBase
{
    [Inject] protected IAuthenticationApiService AuthApiService { get; set; } = null!;
    [Inject] protected IAuthenticationStateService AuthStateService { get; set; } = null!;
    [Inject] protected ITokenStorageService TokenStorage { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;

    protected LoginFormModel FormModel = new();
    protected bool IsLoading = false;
    protected List<string> Errors = new();

    protected bool HasErrors => Errors.Any();

    protected override async Task OnInitializedAsync()
    {
        var currentUser = await AuthStateService.GetCurrentUserAsync();
        if (currentUser != null)
        {
            Navigation.NavigateTo("/dashboard");
        }
    }

    protected async Task HandleLogin()
    {
        IsLoading = true;
        Errors.Clear();

        try
        {
            Console.WriteLine($"Login: Starting login for user {FormModel.NumeUtilizatorSauEmail}");
            
            var loginRequest = new LoginRequest(FormModel.NumeUtilizatorSauEmail, FormModel.Parola);
            var result = await AuthApiService.LoginAsync(loginRequest);

            if (result.IsSuccess && result.Value != null)
            {
                Console.WriteLine($"Login successful for user: {result.Value.NumeUtilizator}");
                
                await SaveAuthenticationData(result.Value);
                await UpdateAuthenticationState(result.Value);
                
                await Task.Delay(200);
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                Errors = result.Errors ?? new List<string> { "Credentiale invalide" };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login exception: {ex.Message}");
            Errors = new List<string> { "Eroare de conexiune la server" };
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void HandleForgotPassword()
    {
        Console.WriteLine("Forgot password clicked");
    }

    private async Task SaveAuthenticationData(AuthenticationResponse authResponse)
    {
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "valyanmed_auth_token", authResponse.Token);
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "valyanmed_user_info", System.Text.Json.JsonSerializer.Serialize(authResponse));
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", System.Text.Json.JsonSerializer.Serialize(authResponse));
    }

    private async Task UpdateAuthenticationState(AuthenticationResponse authResponse)
    {
        try
        {
            await TokenStorage.SetTokenAsync(authResponse.Token);
            await TokenStorage.SetUserInfoAsync(authResponse);
            await AuthStateService.SetCurrentUserAsync(authResponse);
        }
        catch (Exception serviceEx)
        {
            Console.WriteLine($"Service save failed: {serviceEx.Message}");
        }
        
        var customAuthProvider = (CustomAuthenticationStateProvider)AuthStateProvider;
        await customAuthProvider.MarkUserAsAuthenticatedAsync(authResponse.Token, authResponse);
    }

    public class LoginFormModel
    {
        public string NumeUtilizatorSauEmail { get; set; } = string.Empty;
        public string Parola { get; set; } = string.Empty;
    }
}