using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Data;
using FluentValidation;

// New authentication services and repositories
using Infrastructure.Repositories.Authentication;
using Infrastructure.Services.Authentication;
using Application.Services.Authentication;
using Shared.Validators.Authentication;

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

// DB connection for repositories
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// New Authentication Repositories
builder.Services.AddScoped<IPersoanaRepository, PersoanaRepository>();
builder.Services.AddScoped<IUtilizatorRepository, UtilizatorRepository>();

// Authentication Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService>(sp =>
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
builder.Services.AddScoped<Application.Services.Authentication.IPersoanaService, Application.Services.Authentication.PersoanaService>();
builder.Services.AddScoped<Application.Services.Authentication.IUtilizatorService, Application.Services.Authentication.UtilizatorService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// FluentValidation Validators
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.CreatePersoanaRequest>, CreatePersoanaValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.UpdatePersoanaRequest>, UpdatePersoanaValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.CreateUtilizatorRequest>, CreateUtilizatorValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.UpdateUtilizatorRequest>, UpdateUtilizatorValidator>();
builder.Services.AddScoped<IValidator<Shared.DTOs.Authentication.LoginRequest>, LoginValidator>();

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