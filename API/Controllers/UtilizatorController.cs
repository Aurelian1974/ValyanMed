using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using System;
using System.Threading.Tasks;
using API.Services;
using API.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilizatorController : ControllerBase
    {
        private readonly IUtilizatorService _utilizatorService;
        private readonly ILogger<UtilizatorController> _logger;

        public UtilizatorController(IUtilizatorService utilizatorService, ILogger<UtilizatorController> logger)
        {
            _utilizatorService = utilizatorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _utilizatorService.GetAllUtilizatoriAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, $"Error retrieving users: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _utilizatorService.GetUtilizatorByIdAsync(id);
                if (user == null)
                    return NotFound();
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, $"Error retrieving user: {ex.Message}");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUtilizatorDTO dto)
        {
            try
            {
                var id = await _utilizatorService.CreateUtilizatorAsync(dto);
                return Ok(new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUtilizatorDTO dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { Success = false, Message = "ID din URL nu corespunde cu ID din date" });
                }

                var success = await _utilizatorService.UpdateUtilizatorAsync(dto);

                if (success)
                {
                    return Ok(new { Success = true, Message = "Utilizator actualizat cu succes" });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Nu s-a putut actualiza utilizatorul" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                
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
                    return Ok();
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, $"Error deleting user: {ex.Message}");
            }
        }

        [HttpPost("authenticate"), HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _utilizatorService.AuthenticateAsync(dto.NumeUtilizatorSauEmail, dto.Parola);
                if (!result.Success)
                    return Unauthorized(new { result.Message });

                var response = new API.Models.AuthResponseDto
                {
                    Token = result.Token,
                    NumeUtilizator = result.User?.NumeUtilizator,
                    NumeComplet = result.NumeComplet,
                    Email = result.User?.Email
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user {User}", dto.NumeUtilizatorSauEmail);
                return StatusCode(500, $"Error during authentication: {ex.Message}");
            }
        }
    }
}