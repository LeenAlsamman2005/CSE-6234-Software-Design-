using System.Security.Cryptography;
using System.Text;

namespace EventBookingSystem.Services;

public static class PasswordHasher
{
    private const string Salt = "EBS_2026_SALT";

    public static string Hash(string plain)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain + Salt));
        return Convert.ToBase64String(bytes);
    }

    public static bool Verify(string plain, string hashed) => Hash(plain) == hashed;
}
