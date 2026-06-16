using EventBookingSystem.Models;
using EventBookingSystem.Repositories;

namespace EventBookingSystem.Services;

public record OrganizerReport(
    int TotalEvents,
    int TotalBookings,
    decimal TotalRevenue,
    IReadOnlyList<EventSalesRow> Rows);

public record EventSalesRow(string Title, int Bookings, int TicketsSold, decimal Revenue);

public record AdminReport(
    int TotalUsers,
    int TotalCustomers,
    int TotalOrganizers,
    int TotalEvents,
    int TotalBookings,
    decimal GrossRevenue,
    int FailedPayments);

public class ReportingService
{
    private readonly IBookingRepository _bookings;
    private readonly IEventRepository _events;
    private readonly IUserRepository _users;

    public ReportingService(IBookingRepository bookings, IEventRepository events, IUserRepository users)
    {
        _bookings = bookings;
        _events = events;
        _users = users;
    }

    public OrganizerReport ForOrganizer(int organizerId)
    {
        var events = _events.ByOrganizer(organizerId).ToList();
        var bookings = _bookings.ForOrganizer(organizerId)
            .Where(b => b.Status == BookingStatus.Confirmed).ToList();

        var rows = events.Select(ev =>
        {
            var evBookings = bookings.Where(b => b.EventId == ev.EventId).ToList();
            return new EventSalesRow(
                ev.Title,
                evBookings.Count,
                evBookings.Sum(b => b.Tickets.Count),
                evBookings.Sum(b => b.TotalAmount));
        }).ToList();

        return new OrganizerReport(
            events.Count,
            bookings.Count,
            bookings.Sum(b => b.TotalAmount),
            rows);
    }

    public AdminReport ForAdmin()
    {
        var users = _users.All().ToList();
        var allBookings = _bookings.All().ToList();
        var confirmed = allBookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
        return new AdminReport(
            users.Count,
            users.Count(u => u.Role == UserRole.Customer),
            users.Count(u => u.Role == UserRole.Organizer),
            _events.Published().Count(),
            confirmed.Count,
            confirmed.Sum(b => b.TotalAmount),
            allBookings.Count(b => b.Payment?.Status == PaymentStatus.Failed));
    }
}
