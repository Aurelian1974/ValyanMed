using Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Client.Models;
using Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

// Register authentication services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILogoutService, AuthService>();

// Bridge between namespaces using adapter
builder.Services.AddScoped<Client.Services.ILogoutService>(sp => 
    new Client.Services.LogoutServiceAdapter(
        sp.GetRequiredService<ILogoutService>()));

builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<IAuthService>() as AuthService);
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<IExceptionHandler>(_ => new LoggingExceptionHandler());
builder.Services.AddScoped<IPersoanaService, PersoanaService>();
builder.Services.AddScoped<IUtilizatorService, Client.Services.UtilizatorService>();

builder.Services.AddScoped<Client.Services.JsInteropService>();
builder.Services.AddScoped<JudetService>();
builder.Services.AddScoped<ILocalitateService, LocalitateService>();
builder.Services.AddScoped<IPersoanaService, PersoanaService>();

// Medicament client
builder.Services.AddScoped<IMedicamentClient, MedicamentClient>();

// Single HttpClient registration
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "https://localhost:7294/") 
});

builder.Services.AddScoped<JsInteropTestService>();

await builder.Build().RunAsync();
