using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBookingSystem.Models;

public enum UserRole
{
    Customer,
    Organizer,
    Admin
}

public abstract class User
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(120), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public abstract string Dashboard { get; }
}

public class Customer : User
{
    public override string Dashboard => "/Events";
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class Organizer : User
{
    public override string Dashboard => "/Organizer";
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

public class Admin : User
{
    public override string Dashboard => "/Admin";
}
