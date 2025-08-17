using System;
using System.Data; // Add this at the top if not present
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen; // Add this line
using Application.Services;
using Infrastructure.Repositories;
using Core.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

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

app.UseAuthorization();

app.MapControllers();

app.Run();