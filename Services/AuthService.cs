using EventBookingSystem.Models;
using EventBookingSystem.Patterns.Factory;
using EventBookingSystem.Repositories;

namespace EventBookingSystem.Services;

public record AuthResult(bool Success, User? User, string Message);

public class AuthService
{
    private readonly IUserRepository _users;
    public AuthService(IUserRepository users) => _users = users;

    public AuthResult Login(string email, string password)
    {
        var user = _users.FindByEmail(email);
        if (user == null) return new AuthResult(false, null, "Account not found.");
        if (!user.IsActive) return new AuthResult(false, null, "Account is inactive.");
        if (!PasswordHasher.Verify(password, user.Password))
            return new AuthResult(false, null, "Invalid credentials.");
        return new AuthResult(true, user, "Welcome back.");
    }

    public AuthResult Register(string name, string email, string password, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || password.Length < 6)
            return new AuthResult(false, null, "Invalid input.");
        if (role == UserRole.Admin)
            return new AuthResult(false, null, "Admin signup not allowed.");
        if (_users.FindByEmail(email) != null)
            return new AuthResult(false, null, "Email already in use.");

        var user = UserFactory.CreateUser(role, name.Trim(), email.Trim().ToLower(),
            PasswordHasher.Hash(password));
        _users.Add(user);
        _users.Save();
        return new AuthResult(true, user, "Account created.");
    }
}
