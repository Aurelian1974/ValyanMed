using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using API.Models;

[ApiController]
[Route("api/[controller]")]
public class UtilizatorController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<UtilizatorController> _logger;

    public UtilizatorController(IConfiguration config, ILogger<UtilizatorController> logger)
    {
        _config = config;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] UtilizatorRegisterDto dto)
    {
        _logger.LogInformation("Register endpoint called. Data: {@Dto}", dto);

        try
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);

            var parolaHash = BCrypt.Net.BCrypt.HashPassword(dto.Parola);
            _logger.LogInformation("Password hashed.");

            var parameters = new
            {
                NumeUtilizator = dto.NumeUtilizator,
                ParolaHash = parolaHash,
                Email = dto.Email,
                PersoanaId = dto.PersoanaId
            };

            _logger.LogInformation("Executing stored procedure usp_Utilizator_Insert with params: {@Params}", parameters);

            var rows = await connection.ExecuteAsync(
                "usp_Utilizator_Insert",
                parameters,
                commandType: CommandType.StoredProcedure);

            _logger.LogInformation("Rows affected: {Rows}", rows);

            if (rows > 0)
                return Ok();
            else
            {
                _logger.LogWarning("No rows inserted in database.");
                return BadRequest("Nu s-a putut insera utilizatorul.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during user registration.");
            return BadRequest($"Eroare la inserare: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("Login endpoint called for user: {Username}", loginDto.NumeUtilizatorSauEmail);

        try
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);

            // Verifică dacă utilizatorul există
            var user = await connection.QueryFirstOrDefaultAsync<UtilizatorDb>(
                "usp_Utilizator_Authenticate",
                new { NumeUtilizatorSauEmail = loginDto.NumeUtilizatorSauEmail },
                commandType: CommandType.StoredProcedure);

            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", loginDto.NumeUtilizatorSauEmail);
                return Unauthorized("Utilizator sau parolă incorectă");
            }

            // Verifică parola
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginDto.Parola, user.ParolaHash);
            if (!isValidPassword)
            {
                _logger.LogWarning("Invalid password for user: {Username}", loginDto.NumeUtilizatorSauEmail);
                return Unauthorized("Utilizator sau parolă incorectă");
            }

            // Generează token JWT
            var token = GenerateJwtToken(user);

            // Returnează rezultatul autentificării
            var result = new AuthResponseDto
            {
                Token = token,
                NumeUtilizator = user.NumeUtilizator,
                NumeComplet = $"{user.Nume} {user.Prenume}",
                Email = user.Email
            };

            _logger.LogInformation("Login successful for user: {Username}", loginDto.NumeUtilizatorSauEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during login for user: {Username}", loginDto.NumeUtilizatorSauEmail);
            return BadRequest($"Eroare la autentificare: {ex.Message}");
        }
    }

    private string GenerateJwtToken(UtilizatorDb user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.NumeUtilizator),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", $"{user.Nume} {user.Prenume}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}