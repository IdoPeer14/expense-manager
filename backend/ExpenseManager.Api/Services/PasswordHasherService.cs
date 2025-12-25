using ExpenseManager.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseManager.Api.Services;

public class PasswordHasherService
{
    private readonly PasswordHasher<User> _passwordHasher;

    public PasswordHasherService()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password, string hashedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
        return result == PasswordVerificationResult.Success;
    }
}
