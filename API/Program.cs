using System;
using System.Data; // Add this at the top if not present
using Microsoft.Data.SqlClient;
using API.Repositories;
using API.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen; // Add this line
using Application.Services;
using Infrastructure.Repositories;


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

// Register repositories and services
builder.Services.AddScoped<IPersoanaRepository, PersoanaRepository>();
builder.Services.AddScoped<IPersoanaService, PersoanaService>();
builder.Services.AddScoped<IUtilizatorService, UtilizatorService>();
builder.Services.AddScoped<IJudetRepository, JudetRepository>();
builder.Services.AddScoped<IJudetService, JudetService>();
builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7294/") });
builder.Services.AddScoped<ILocalitateRepository, LocalitateRepository>();
builder.Services.AddScoped<ILocalitateService, LocalitateService>();

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
    throw; // Oprește aplicația dacă nu se poate conecta
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