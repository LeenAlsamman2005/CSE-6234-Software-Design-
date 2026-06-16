using EventBookingSystem.Data;
using EventBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Repositories;

public interface IUserRepository
{
    User? FindByEmail(string email);
    User? FindById(int id);
    void Add(User user);
    IEnumerable<User> All();
    void Update(User user);
    void Save();
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public User? FindByEmail(string email) => _db.Users.FirstOrDefault(u => u.Email == email);
    public User? FindById(int id) => _db.Users.Find(id);
    public void Add(User user) => _db.Users.Add(user);
    public IEnumerable<User> All() => _db.Users.OrderBy(u => u.UserId).ToList();
    public void Update(User user) => _db.Users.Update(user);
    public void Save() => _db.SaveChanges();
}

public interface IEventRepository
{
    IEnumerable<Event> Published();
    IEnumerable<Event> ByOrganizer(int organizerId);
    Event? FindWithDetails(int id);
    void Add(Event ev);
    void Update(Event ev);
    void Remove(Event ev);
    void Save();
}

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;
    public EventRepository(AppDbContext db) => _db = db;

    public IEnumerable<Event> Published() =>
        _db.Events.Include(e => e.Categories)
                  .Where(e => e.Status == EventStatus.Published)
                  .OrderBy(e => e.EventDate)
                  .ToList();

    public IEnumerable<Event> ByOrganizer(int organizerId) =>
        _db.Events.Include(e => e.Categories)
                  .Where(e => e.OrganizerId == organizerId)
                  .OrderByDescending(e => e.EventDate)
                  .ToList();

    public Event? FindWithDetails(int id) =>
        _db.Events.Include(e => e.Categories)
                  .Include(e => e.Organizer)
                  .FirstOrDefault(e => e.EventId == id);

    public void Add(Event ev) => _db.Events.Add(ev);
    public void Update(Event ev) => _db.Events.Update(ev);
    public void Remove(Event ev) => _db.Events.Remove(ev);
    public void Save() => _db.SaveChanges();
}

public interface IBookingRepository
{
    IEnumerable<Booking> ForCustomer(int customerId);
    IEnumerable<Booking> ForOrganizer(int organizerId);
    IEnumerable<Booking> All();
    Booking? FindWithDetails(int id);
}

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    public BookingRepository(AppDbContext db) => _db = db;

    public IEnumerable<Booking> ForCustomer(int customerId) =>
        _db.Bookings.Include(b => b.Event)
                    .Include(b => b.Tickets)
                    .Include(b => b.Payment)
                    .Where(b => b.CustomerId == customerId)
                    .OrderByDescending(b => b.BookingDate)
                    .ToList();

    public IEnumerable<Booking> ForOrganizer(int organizerId) =>
        _db.Bookings.Include(b => b.Event)
                    .Include(b => b.Customer)
                    .Include(b => b.Tickets)
                    .Where(b => b.Event!.OrganizerId == organizerId)
                    .OrderByDescending(b => b.BookingDate)
                    .ToList();

    public IEnumerable<Booking> All() =>
        _db.Bookings.Include(b => b.Event)
                    .Include(b => b.Customer)
                    .Include(b => b.Payment)
                    .OrderByDescending(b => b.BookingDate)
                    .ToList();

    public Booking? FindWithDetails(int id) =>
        _db.Bookings.Include(b => b.Event)
                    .Include(b => b.Customer)
                    .Include(b => b.Tickets).ThenInclude(t => t.TicketCategory)
                    .Include(b => b.Payment)
                    .FirstOrDefault(b => b.BookingId == id);
}

public interface INotificationRepository
{
    IEnumerable<Notification> ForUser(int userId);
    int UnreadCount(int userId);
    void MarkRead(int userId);
}

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;
    public NotificationRepository(AppDbContext db) => _db = db;

    public IEnumerable<Notification> ForUser(int userId) =>
        _db.Notifications.Where(n => n.UserId == userId)
                         .OrderByDescending(n => n.CreatedAt).Take(50).ToList();

    public int UnreadCount(int userId) =>
        _db.Notifications.Count(n => n.UserId == userId && !n.IsRead);

    public void MarkRead(int userId)
    {
        var list = _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();
        foreach (var n in list) n.IsRead = true;
        _db.SaveChanges();
    }
}
