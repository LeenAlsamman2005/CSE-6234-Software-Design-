using EventBookingSystem.Models;
using EventBookingSystem.Services;

namespace EventBookingSystem.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Users.Any()) return;

        var admin = new Admin
        {
            Name = "System Admin",
            Email = "admin@ebs.com",
            Password = PasswordHasher.Hash("admin123"),
            Role = UserRole.Admin
        };

        var organizer1 = new Organizer
        {
            Name = "Leen Alsamman",
            Email = "leen@ebs.com",
            Password = PasswordHasher.Hash("organizer123"),
            Role = UserRole.Organizer
        };

        var organizer2 = new Organizer
        {
            Name = "Hanan Esam",
            Email = "hanan@ebs.com",
            Password = PasswordHasher.Hash("organizer123"),
            Role = UserRole.Organizer
        };

        var customer1 = new Customer
        {
            Name = "Fatima Ali",
            Email = "fatima@ebs.com",
            Password = PasswordHasher.Hash("customer123"),
            Role = UserRole.Customer
        };

        var customer2 = new Customer
        {
            Name = "Zenab Hosameldin",
            Email = "zenab@ebs.com",
            Password = PasswordHasher.Hash("customer123"),
            Role = UserRole.Customer
        };

        db.Users.AddRange(admin, organizer1, organizer2, customer1, customer2);
        db.SaveChanges();

        var ev1 = new Event
        {
            Title = "Rock Concert 2026",
            Description = "Live rock concert featuring top international artists.",
            Venue = "Axiata Arena, Kuala Lumpur",
            EventDate = DateTime.UtcNow.AddDays(30),
            Category = "Concert",
            Status = EventStatus.Published,
            OrganizerId = organizer1.UserId,
            Categories = new List<TicketCategory>
            {
                new() { Name = "VIP", Price = 350m, TotalQuantity = 50 },
                new() { Name = "Standard", Price = 150m, TotalQuantity = 200 },
                new() { Name = "Economy", Price = 80m, TotalQuantity = 500 }
            }
        };

        var ev2 = new Event
        {
            Title = "Tech Summit 2026",
            Description = "Annual technology summit on AI, cloud, and cybersecurity.",
            Venue = "MMU Cyberjaya Auditorium",
            EventDate = DateTime.UtcNow.AddDays(45),
            Category = "Conference",
            Status = EventStatus.Published,
            OrganizerId = organizer1.UserId,
            Categories = new List<TicketCategory>
            {
                new() { Name = "All-Access", Price = 220m, TotalQuantity = 100 },
                new() { Name = "Student", Price = 50m, TotalQuantity = 300 }
            }
        };

        var ev3 = new Event
        {
            Title = "Food Festival",
            Description = "Two-day food festival with international cuisines.",
            Venue = "Pavilion KL",
            EventDate = DateTime.UtcNow.AddDays(15),
            Category = "Festival",
            Status = EventStatus.Published,
            OrganizerId = organizer2.UserId,
            Categories = new List<TicketCategory>
            {
                new() { Name = "Entry", Price = 25m, TotalQuantity = 1000 },
                new() { Name = "Premium Tasting", Price = 90m, TotalQuantity = 200 }
            }
        };

        db.Events.AddRange(ev1, ev2, ev3);
        db.SaveChanges();
    }
}
