using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Data;
using FluentValidation;

// Authentication services and repositories
using Infrastructure.Repositories.Authentication;
using Infrastructure.Services.Authentication;
using Application.Services.Authentication;
using Infrastructure.Services.Medical;
using Application.Services.Medical;

// Medical services and repositories
using Infrastructure.Repositories.Medical;
using Shared.Validators.Authentication;
using Shared.Validators.Medical;

// Common services
using Application.Services.Common;
using Infrastructure.Services.Common;

// Data layer
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ValyanMed API",
        Version = "v1",
        Description = "API pentru sistemul de management medical ValyanMed"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Data layer - Connection factory
builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IDatabaseTestService, DatabaseTestService>();

// DB connection for repositories (legacy support)
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication Repositories - ACTUALIZAT pentru a folosi UtilizatoriSistem
builder.Services.AddScoped<Application.Services.Authentication.IPersoanaRepository, PersoanaRepository>();
builder.Services.AddScoped<Application.Services.Authentication.IUtilizatorRepository, UtilizatoriSistemRepository>(); // SCHIMBAT

// Medical Repositories
builder.Services.AddScoped<Application.Services.Medical.IPacientRepository, PacientRepository>();
builder.Services.AddScoped<Application.Services.Medical.IPersonalMedicalRepository, PersonalMedicalRepository>();

// Authentication Services
builder.Services.AddScoped<Application.Services.Authentication.IPasswordService, PasswordService>();
builder.Services.AddScoped<Application.Services.Authentication.IJwtService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new JwtService(
        config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"),
        config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured"),
        config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured"),
        int.Parse(config["Jwt:ExpirationHours"] ?? "24")
    );
});

// Application Services
builder.Services.AddScoped<IPersoanaService, Application.Services.Authentication.PersoanaService>();
builder.Services.AddScoped<IUtilizatorService, Application.Services.Authentication.UtilizatorService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Medical Services
builder.Services.AddScoped<IPacientService, PacientService>();
builder.Services.AddScoped<IJudetService, JudetService>();
builder.Services.AddScoped<IPersonalMedicalService, PersonalMedicalService>();

// Common Services
builder.Services.AddScoped<ILocationService, LocationService>();

// FluentValidation Validators
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.CreatePersoanaRequest>, CreatePersoanaValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.UpdatePersoanaRequest>, UpdatePersoanaValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.CreateUtilizatorRequest>, CreateUtilizatorValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.UpdateUtilizatorRequest>, UpdateUtilizatorValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.LoginRequest>, LoginValidator>();

// Medical Validators
builder.Services.AddScoped<IValidator<Shared.DTOs.Medical.CreatePersonalMedicalRequest>, CreatePersonalMedicalValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Medical.UpdatePersonalMedicalRequest>, UpdatePersonalMedicalValidator>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

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

try
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    using var connection = new SqlConnection(connectionString);
    connection.Open();
    connection.Close();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to connect to database");
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValyanMed API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();