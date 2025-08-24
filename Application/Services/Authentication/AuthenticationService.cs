using FluentValidation;
using Infrastructure.Repositories.Authentication;
using Infrastructure.Services.Authentication;
using Shared.Common;
using Shared.DTOs.Authentication;
using Shared.Validators.Authentication;

namespace Application.Services.Authentication;

public interface IAuthenticationService
{
    Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request);
    Task<Result> LogoutAsync(int utilizatorId);
    Task<Result<bool>> ValidateTokenAsync(string token);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUtilizatorRepository _utilizatorRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthenticationService(
        IUtilizatorRepository utilizatorRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IValidator<LoginRequest> loginValidator)
    {
        _utilizatorRepository = utilizatorRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _loginValidator = loginValidator;
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request)
    {
        // Validare
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<AuthenticationResponse>.Failure(errors);
        }

        // G?sire utilizator
        var utilizatorResult = await _utilizatorRepository.GetByUsernameOrEmailAsync(request.NumeUtilizatorSauEmail);
        if (!utilizatorResult.IsSuccess)
        {
            return Result<AuthenticationResponse>.Failure(utilizatorResult.Errors);
        }

        var utilizator = utilizatorResult.Value;
        if (utilizator == null)
        {
            return Result<AuthenticationResponse>.Failure("Numele de utilizator sau parola sunt incorecte");
        }

        // Verificare parol?
        if (!_passwordService.VerifyPassword(request.Parola, utilizator.ParolaHash))
        {
            return Result<AuthenticationResponse>.Failure("Numele de utilizator sau parola sunt incorecte");
        }

        // Generare token
        var token = _jwtService.GenerateToken(utilizator);
        var expiration = DateTime.UtcNow.AddHours(24); // Sau din configura?ie

        var response = new AuthenticationResponse(
            utilizator.Id,
            utilizator.NumeUtilizator,
            utilizator.Email,
            utilizator.NumeComplet,
            token,
            expiration
        );

        return Result<AuthenticationResponse>.Success(response, "Autentificare reu?it?");
    }

    public async Task<Result> LogoutAsync(int utilizatorId)
    {
        // TODO: Implementare invalidare token în Redis/cache
        // Pentru moment, logout-ul se face pe client prin ?tergerea token-ului
        
        await Task.CompletedTask; // Pentru a evita warning-ul
        return Result.Success("Logout realizat cu succes");
    }

    public async Task<Result<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return Result<bool>.Success(false);
            }

            // Op?ional: verificare existen?? utilizator în baza de date
            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                var utilizatorResult = await _utilizatorRepository.GetByIdAsync(userId);
                if (!utilizatorResult.IsSuccess || utilizatorResult.Value == null)
                {
                    return Result<bool>.Success(false);
                }
            }

            return Result<bool>.Success(true);
        }
        catch
        {
            return Result<bool>.Success(false);
        }
    }
}