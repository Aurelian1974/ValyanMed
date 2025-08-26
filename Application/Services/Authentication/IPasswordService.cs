using Shared.Models.Authentication;

namespace Application.Services.Authentication;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}