using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using API.Models;
using API.Services;
using Shared.DTOs;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class UtilizatorController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<UtilizatorController> _logger;
    private readonly IUtilizatorService _utilizatorService;

    public UtilizatorController(IConfiguration config, ILogger<UtilizatorController> logger, IUtilizatorService utilizatorService)
    {
        _config = config;
        _logger = logger;
        _utilizatorService = utilizatorService;
    }

    [HttpPost]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UtilizatorRegisterDto dto)
    {
        _logger.LogInformation("Register endpoint called. Data: {@Dto}", dto);

        try
        {
            var createDto = new CreateUtilizatorDTO
            {
                NumeUtilizator = dto.NumeUtilizator,
                Parola = dto.Parola,
                Email = dto.Email,
                PersoanaId = dto.PersoanaId,
                Telefon = null
            };

            var id = await _utilizatorService.CreateUtilizatorAsync(createDto);

            if (id > 0)
                return Ok(new { Success = true, Id = id, Message = "Utilizator înregistrat cu succes" });
            else
                return BadRequest(new { Success = false, Message = "Nu s-a putut înregistra utilizatorul" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during user registration");
            return BadRequest(new { Success = false, Message = $"Eroare la înregistrare: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("Login endpoint called for user: {Username}", loginDto.NumeUtilizatorSauEmail);

        try
        {
            var authResult = await _utilizatorService.AuthenticateAsync(
                loginDto.NumeUtilizatorSauEmail, 
                loginDto.Parola);

            if (!authResult.Success)
            {
                _logger.LogWarning("Authentication failed: {Message}", authResult.Message);
                return Unauthorized(authResult.Message);
            }

            var token = GenerateJwtToken(authResult.User);

            var result = new AuthResponseDto
            {
                Token = token,
                NumeUtilizator = authResult.User.NumeUtilizator,
                NumeComplet = authResult.NumeComplet,
                Email = authResult.User.Email
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UtilizatorDTO>>> GetAll()
    {
        try
        {
            var utilizatori = await _utilizatorService.GetAllUtilizatoriAsync();
            return Ok(utilizatori);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, $"Error retrieving users: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UtilizatorDTO>> GetById(int id)
    {
        try
        {
            var utilizator = await _utilizatorService.GetUtilizatorByIdAsync(id);
            
            if (utilizator == null)
                return NotFound($"User with ID {id} not found");
                
            return Ok(utilizator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user with id {id}");
            return StatusCode(500, $"Error retrieving user: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUtilizatorDTO dto)
    {
        try
        {
            _logger.LogInformation("=== UPDATE CONTROLLER DEBUG ===");
            _logger.LogInformation("URL ID: {UserId}", id);
            _logger.LogInformation("DTO ID: {DtoId}", dto.Id);
            _logger.LogInformation("DTO NumeUtilizator: {NumeUtilizator}", dto.NumeUtilizator);
            _logger.LogInformation("DTO Email: {Email}", dto.Email);
            _logger.LogInformation("DTO Telefon: {Telefon}", dto.Telefon);
            _logger.LogInformation("DTO PersoanaId: {PersoanaId}", dto.PersoanaId);
            _logger.LogInformation("DTO Guid: {Guid}", dto.Guid);
            _logger.LogInformation("DTO HasPassword: {HasPassword}", !string.IsNullOrWhiteSpace(dto.Parola));
            
            if (id != dto.Id)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} vs DTO ID {DtoId}", id, dto.Id);
                return BadRequest(new { Success = false, Message = "ID din URL nu corespunde cu ID din date" });
            }

            var success = await _utilizatorService.UpdateUtilizatorAsync(dto);
            
            _logger.LogInformation("Service returned: {Success}", success);
            
            if (success)
            {
                _logger.LogInformation("User {UserId} updated successfully", id);
                return Ok(new { Success = true, Message = "Utilizator actualizat cu succes" });
            }
            else
            {
                _logger.LogWarning("Failed to update user {UserId} - no rows affected", id);
                return BadRequest(new { Success = false, Message = "Nu s-a putut actualiza utilizatorul" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}. Exception: {ExceptionMessage}", id, ex.Message);
            
            if (ex.Message.Contains("Numele de utilizator exista deja"))
            {
                return Conflict(new { Success = false, Message = "Numele de utilizator exista deja" });
            }
            if (ex.Message.Contains("Adresa de email exista deja"))
            {
                return Conflict(new { Success = false, Message = "Adresa de email exista deja" });
            }
            if (ex.Message.Contains("Persoana asociata nu exista"))
            {
                return BadRequest(new { Success = false, Message = "Persoana asociata nu exista" });
            }
            if (ex.Message.Contains("Utilizatorul nu a fost gasit"))
            {
                return NotFound(new { Success = false, Message = "Utilizatorul nu a fost gasit" });
            }
            
            return StatusCode(500, new { Success = false, Message = $"Eroare interna: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _utilizatorService.DeleteUtilizatorAsync(id);
            
            if (success)
                return Ok(new { Success = true, Message = "Utilizator șters cu succes" });
            else
                return NotFound(new { Success = false, Message = "Utilizatorul nu a fost gasit" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user with id {id}");
            return StatusCode(500, $"Error deleting user: {ex.Message}");
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
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
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