using Application.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;
using Shared.Exceptions;

namespace API.Controllers.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authenticationService.LoginAsync(request);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { 
                data = result.Value,
                message = result.SuccessMessage 
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Ob?inere ID utilizator din token (în viitor)
            var utilizatorId = 0; // Placeholder
            
            var result = await _authenticationService.LogoutAsync(utilizatorId);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { message = result.SuccessMessage });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        try
        {
            var result = await _authenticationService.ValidateTokenAsync(token);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { isValid = result.Value });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }
}