using System.Security.Cryptography;

namespace Infrastructure.Services.Authentication;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations,
            HashAlgorithmName.SHA256);

        var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
        var salt = Convert.ToBase64String(algorithm.Salt);

        return $"{Iterations}.{salt}.{key}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            var parts = hash.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 3)
            {
                // Unknown/legacy format -> treat as mismatch instead of throwing
                return false;
            }

            if (!int.TryParse(parts[0], out var iterations) || iterations <= 0)
                return false;

            // Parse salt/key; if invalid Base64, treat as mismatch (no exception leak)
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
            // Fallback without FixedTimeEquals (still safe enough for our case)
            return keyToCheck.SequenceEqual(key);
#endif
        }
        catch
        {
            // Any parsing/crypto error -> do not crash the API; simply report mismatch
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