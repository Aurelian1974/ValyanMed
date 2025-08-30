using System.Security.Cryptography;
using Application.Services.Authentication;

namespace Infrastructure.Services.Authentication;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        // Folosim BCrypt pentru noile parole
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // Verific?m dac? este bcrypt hash (începe cu $2a$, $2b$, $2x$, $2y$)
            if (hash.StartsWith("$2"))
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }

            // Fallback pentru formatul vechi PBKDF2
            return VerifyPbkdf2Password(password, hash);
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyPbkdf2Password(string password, string hash)
    {
        try
        {
            var parts = hash.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var iterations) || iterations <= 0)
                return false;

            if (!TryFromBase64(parts[1], out var salt))
                return false;
            if (!TryFromBase64(parts[2], out var key))
                return false;

            using var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            var keyToCheck = algorithm.GetBytes(KeySize);
#if NET6_0_OR_GREATER
            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
#else
            return keyToCheck.SequenceEqual(key);
#endif
        }
        catch
        {
            return false;
        }
    }

    private static bool TryFromBase64(string value, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            bytes = Array.Empty<byte>();
            return false;
        }
    }
}