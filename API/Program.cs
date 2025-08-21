using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Data.SqlClient;
using API.Services; // for ICurrentUserService
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Data;
using Core.Interfaces; // IMedicamentRepository
using Infrastructure.Repositories; // MedicamentRepository

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
        Title = "Personnel Management API",
        Version = "v1"
    });
});

// DB connection for repositories
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IMedicamentRepository, MedicamentRepository>();
builder.Services.AddScoped<IPersoanaRepository, PersoanaRepository>();
builder.Services.AddScoped<IPartenerRepository, PartenerRepository>();
builder.Services.AddScoped<IJudetRepository, JudetRepository>();
builder.Services.AddScoped<ILocalitateRepository, LocalitateRepository>();
builder.Services.AddScoped<IMaterialSanitarRepository, MaterialSanitarRepository>();
builder.Services.AddScoped<IDispozitivMedicalRepository, DispozitivMedicalRepository>();

// Application services
builder.Services.AddScoped<Application.Services.IMedicamentService, Application.Services.MedicamentService>();
builder.Services.AddScoped<Application.Services.IPersoanaService, Application.Services.PersoanaService>();
builder.Services.AddScoped<Application.Services.IPartenerService, Application.Services.PartenerService>();
builder.Services.AddScoped<Application.Services.IJudetService, Application.Services.JudetService>();
builder.Services.AddScoped<Application.Services.ILocalitateService, Application.Services.LocalitateService>();
builder.Services.AddScoped<Application.Services.IMaterialSanitarService, Application.Services.MaterialSanitarService>();
builder.Services.AddScoped<Application.Services.IDispozitivMedicalService, Application.Services.DispozitivMedicalService>();

// API-specific services
builder.Services.AddScoped<API.Services.IUtilizatorService, API.Services.UtilizatorService>();

// JWT token generator
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Current user accessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
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
catch (Exception)
{
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Personnel Management API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();