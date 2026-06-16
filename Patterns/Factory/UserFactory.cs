using EventBookingSystem.Models;

namespace EventBookingSystem.Patterns.Factory;

/// <summary>
/// Factory Method Pattern: centralizes role-based User creation.
/// Adding a new role requires editing only this factory.
/// </summary>
public static class UserFactory
{
    public static User CreateUser(UserRole role, string name, string email, string hashedPassword)
    {
        User user = role switch
        {
            UserRole.Customer => new Customer(),
            UserRole.Organizer => new Organizer(),
            UserRole.Admin => new Admin(),
            _ => throw new ArgumentException($"Unknown role: {role}")
        };
        user.Name = name;
        user.Email = email;
        user.Password = hashedPassword;
        user.Role = role;
        return user;
    }
}
