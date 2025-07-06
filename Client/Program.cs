using Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop; // Adaugă acest import pentru IJSRuntime
using MudBlazor.Services;
using ValyanMed.Client.Services;
using ValyanMed.Client.Models;
using Client.Services; // Adaugă această linie pentru a rezolva erorile

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7294/") });
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IExceptionHandler>(_ => new LoggingExceptionHandler());

await builder.Build().RunAsync();
