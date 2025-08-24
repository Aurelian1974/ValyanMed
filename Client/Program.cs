using Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using System.Text.Json.Serialization;

// Authentication services
using Client.Services.Authentication;
using Client.Authentication;
using Client.Converters;
using Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

try
{
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    // Core services
    builder.Services.AddMudServices();
    builder.Services.AddBlazoredLocalStorage();
    
    // JSON service for enum handling
    builder.Services.AddSingleton<IJsonService, JsonService>();

    // Simple authentication service for backup
    builder.Services.AddScoped<ISimpleAuthService, SimpleAuthService>();

    // Authentication services - add one by one
    builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
    builder.Services.AddScoped<IAuthenticationStateService, AuthenticationStateService>();
    builder.Services.AddScoped<IAuthenticationApiService, AuthenticationApiService>();
    builder.Services.AddScoped<AuthenticatedHttpClientHandler>();

    // Authentication state provider and authorization
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
    builder.Services.AddAuthorizationCore();

    // HttpClient configuration
    var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:7294/";
    
    builder.Services.AddHttpClient("authenticated", client =>
    {
        client.BaseAddress = new Uri(apiUrl);
        client.Timeout = TimeSpan.FromMinutes(2);
    })
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

    // Default HttpClient
    builder.Services.AddScoped(sp =>
    {
        var factory = sp.GetRequiredService<IHttpClientFactory>();
        return factory.CreateClient("authenticated");
    });

    var app = builder.Build();
    
    // Initialize authentication state on startup
    Console.WriteLine("Initializing authentication state...");
    try
    {
        var authStateService = app.Services.GetRequiredService<IAuthenticationStateService>();
        await authStateService.InitializeAsync();
        Console.WriteLine("Authentication state initialized successfully");
    }
    catch (Exception initEx)
    {
        Console.WriteLine($"Failed to initialize authentication state: {initEx.Message}");
    }
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Blazor startup error: {ex}");
    throw;
}
