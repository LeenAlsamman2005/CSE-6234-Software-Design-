using EventBookingSystem.Data;
using EventBookingSystem.Models;

namespace EventBookingSystem.Patterns.Observer;

/// <summary>
/// Concrete observers persist notifications to DB. They use IServiceScopeFactory because
/// the subject is a singleton but DbContext is scoped.
/// </summary>
public class CustomerObserver : IBookingObserver
{
    private readonly IServiceScopeFactory _scopeFactory;
    public CustomerObserver(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public void Update(BookingEvent e)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Notifications.Add(new Notification
        {
            UserId = e.CustomerId,
            AudienceRole = "Customer",
            Title = $"Booking #{e.BookingId} {e.Status}",
            Message = $"Your booking #{e.BookingId} is now {e.Status}. Amount: RM{e.Amount:F2}.",
            BookingId = e.BookingId
        });
        db.SaveChanges();
    }
}

public class OrganizerObserver : IBookingObserver
{
    private readonly IServiceScopeFactory _scopeFactory;
    public OrganizerObserver(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public void Update(BookingEvent e)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ev = db.Events.Find(e.EventId);
        if (ev == null) return;
        db.Notifications.Add(new Notification
        {
            UserId = ev.OrganizerId,
            AudienceRole = "Organizer",
            Title = $"New booking on {ev.Title}",
            Message = $"Booking #{e.BookingId} status: {e.Status} (RM{e.Amount:F2}).",
            BookingId = e.BookingId
        });
        db.SaveChanges();
    }
}

public class AdminObserver : IBookingObserver
{
    private readonly IServiceScopeFactory _scopeFactory;
    public AdminObserver(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public void Update(BookingEvent e)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var admins = db.Admins.ToList();
        foreach (var a in admins)
        {
            db.Notifications.Add(new Notification
            {
                UserId = a.UserId,
                AudienceRole = "Admin",
                Title = $"Audit: Booking #{e.BookingId} {e.Status}",
                Message = $"Customer {e.CustomerId} on event {e.EventId}: {e.Status} RM{e.Amount:F2}.",
                BookingId = e.BookingId
            });
        }
        db.SaveChanges();
    }
}
