using Shared.Models.Authentication;
using System.Security.Claims;

namespace Application.Services.Authentication;

public interface IJwtService
{
    string GenerateToken(Utilizator utilizator);
    ClaimsPrincipal? ValidateToken(string token);
}