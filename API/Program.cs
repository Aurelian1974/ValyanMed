using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Services;
using Core.Interfaces;
using Infrastructure.Repositories;
using System.Data;
using Microsoft.Data.SqlClient;
using API.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen; // Add this line


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddHttpContextAccessor(); // Add this line

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Personnel Management API",
        Version = "v1",
        Description = "API for managing personnel records"
    });
});

// Register IDbConnection for DI
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register repositories (Infrastructure) against Core interfaces
builder.Services.AddScoped<Core.Interfaces.IPersoanaRepository, Infrastructure.Repositories.PersoanaRepository>();
builder.Services.AddScoped<Core.Interfaces.IJudetRepository, Infrastructure.Repositories.JudetRepository>();
builder.Services.AddScoped<Core.Interfaces.ILocalitateRepository, Infrastructure.Repositories.LocalitateRepository>();
builder.Services.AddScoped<Core.Interfaces.IMedicamentRepository, Infrastructure.Repositories.MedicamentRepository>();

// Register application services (Application)
builder.Services.AddScoped<Application.Services.IPersoanaService, Application.Services.PersoanaService>();
builder.Services.AddScoped<Application.Services.IJudetService, Application.Services.JudetService>();
builder.Services.AddScoped<Application.Services.ILocalitateService, Application.Services.LocalitateService>();
builder.Services.AddScoped<Application.Services.IMedicamentService, Application.Services.MedicamentService>();

// Also register API-specific services
builder.Services.AddScoped<API.Services.IUtilizatorService, API.Services.UtilizatorService>();

// Register services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"JWT Token validated successfully for user: {context.Principal?.Identity?.Name}");
                foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
                {
                    Console.WriteLine($"JWT Claim: {claim.Type} = {claim.Value}");
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                Console.WriteLine($"JWT Token received: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
                return Task.CompletedTask;
            }
        };
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
        };
    });

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Verificare conexiune la baza de date la startup
try
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    using var connection = new SqlConnection(connectionString);
    connection.Open();
    app.Logger.LogInformation("Conexiunea la baza de date a reușit.");
    connection.Close();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Eroare la conectarea la baza de date!");
    throw; // Oprește aplicatia daca nu se poate conecta
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Personnel Management API v1"));
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

// Add authentication and authorization middleware
Console.WriteLine("Setting up Authentication middleware...");
app.UseAuthentication();
Console.WriteLine("Setting up Authorization middleware...");
app.UseAuthorization();

app.MapControllers();

app.Run();