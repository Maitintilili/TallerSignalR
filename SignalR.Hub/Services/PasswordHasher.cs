using System.Security.Cryptography;

namespace SignalR.HubServer.Services;

/// <summary>Hash de contraseñas con PBKDF2 + salt aleatorio. Nunca se guarda la contraseña en texto plano.</summary>
public static class PasswordHasher
{
    private const int Iterations = 600_000;
    private const int HashSize = 32;
    private const int SaltSize = 16;

    public static (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verify(string password, string hash, string salt)
    {
        var expected = Convert.FromBase64String(hash);
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
