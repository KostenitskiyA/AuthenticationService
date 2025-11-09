using Application.Interfaces;
using Crypt = BCrypt.Net.BCrypt;

namespace Infrastructure.Services;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return string.IsNullOrEmpty(password)
            ? throw new ArgumentException("Password cannot be null or empty", nameof(password))
            : Crypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;

        try
        {
            return Crypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}

