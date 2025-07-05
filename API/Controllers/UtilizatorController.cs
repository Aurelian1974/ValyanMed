using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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
}