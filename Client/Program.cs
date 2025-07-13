using Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using ValyanMed.Client.Services;
using ValyanMed.Client.Models;
using Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddMudServices();

// Register authentication services
builder.Services.AddScoped<ValyanMed.Client.Services.IAuthService, ValyanMed.Client.Services.AuthService>();
builder.Services.AddScoped<ValyanMed.Client.Services.ILogoutService, ValyanMed.Client.Services.AuthService>();

// Bridge between namespaces using adapter
builder.Services.AddScoped<Client.Services.ILogoutService>(sp => 
    new Client.Services.LogoutServiceAdapter(
        sp.GetRequiredService<ValyanMed.Client.Services.ILogoutService>()));

builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<ValyanMed.Client.Services.IAuthService>() as ValyanMed.Client.Services.AuthService);
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<IExceptionHandler>(_ => new LoggingExceptionHandler());
builder.Services.AddScoped<IPersoanaService, PersoanaService>();

// Add this to your service registrations
builder.Services.AddScoped<Client.Services.JsInteropService>();
builder.Services.AddScoped<JudetService>();

// Single HttpClient registration
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "https://localhost:7294/") 
});

// If you need to test JS interop, add a simple service:
builder.Services.AddScoped<JsInteropTestService>();

await builder.Build().RunAsync();
