using EventBookingSystem.Data;
using EventBookingSystem.Models;
using EventBookingSystem.Patterns.Observer;
using EventBookingSystem.Patterns.Strategy;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Patterns.Facade;

public record BookingLine(int TicketCategoryId, int Quantity);

public record BookingRequest(
    int CustomerId,
    int EventId,
    IReadOnlyList<BookingLine> Lines,
    string PaymentMethod,
    IDictionary<string, string> PaymentDetails);

public record BookingOutcome(bool Success, int BookingId, string Message, decimal Amount);

/// <summary>
/// Facade Pattern: single entry point that orchestrates Booking + Payment Strategy + Observer.
/// Hides multi-step transaction complexity from controllers.
/// </summary>
public class BookingFacade
{
    private readonly AppDbContext _db;
    private readonly IBookingSubject _subject;

    public BookingFacade(AppDbContext db, IBookingSubject subject)
    {
        _db = db;
        _subject = subject;
    }

    public BookingOutcome ProcessFullBooking(BookingRequest req)
    {
        using var tx = _db.Database.BeginTransaction();
        try
        {
            var ev = _db.Events
                .Include(e => e.Categories)
                .FirstOrDefault(e => e.EventId == req.EventId);
            if (ev == null) return new BookingOutcome(false, 0, "Event not found.", 0);
            if (ev.Status != EventStatus.Published)
                return new BookingOutcome(false, 0, "Event not available for booking.", 0);

            var booking = new Booking
            {
                CustomerId = req.CustomerId,
                EventId = req.EventId,
                Status = BookingStatus.Pending,
                BookingDate = DateTime.UtcNow
            };

            decimal total = 0m;
            foreach (var line in req.Lines)
            {
                if (line.Quantity <= 0) continue;
                var cat = ev.Categories.FirstOrDefault(c => c.TicketCategoryId == line.TicketCategoryId);
                if (cat == null)
                    return new BookingOutcome(false, 0, $"Ticket category {line.TicketCategoryId} invalid.", 0);
                if (cat.Available < line.Quantity)
                    return new BookingOutcome(false, 0,
                        $"Only {cat.Available} {cat.Name} ticket(s) left.", 0);

                for (int i = 0; i < line.Quantity; i++)
                {
                    booking.Tickets.Add(new Ticket
                    {
                        TicketCategoryId = cat.TicketCategoryId,
                        Price = cat.Price,
                        SeatNumber = $"{cat.Name[0]}-{cat.SoldQuantity + i + 1:D3}",
                        Status = "Reserved"
                    });
                }
                cat.SoldQuantity += line.Quantity;
                total += cat.Price * line.Quantity;
            }

            if (total <= 0)
                return new BookingOutcome(false, 0, "Select at least one ticket.", 0);

            booking.TotalAmount = total;
            _db.Bookings.Add(booking);
            _db.SaveChanges();

            var ctx = new PaymentContext();
            ctx.SetStrategy(PaymentContext.Resolve(req.PaymentMethod));
            var result = ctx.Process(total, req.PaymentDetails);

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = total,
                PaymentMethod = ctx.CurrentMethod,
                Status = result.Success ? PaymentStatus.Successful : PaymentStatus.Failed,
                Reference = result.Reference,
                PaidAt = DateTime.UtcNow
            };
            _db.Payments.Add(payment);

            if (!result.Success)
            {
                booking.Status = BookingStatus.Cancelled;
                foreach (var line in req.Lines)
                {
                    var cat = ev.Categories.FirstOrDefault(c => c.TicketCategoryId == line.TicketCategoryId);
                    if (cat != null) cat.SoldQuantity -= line.Quantity;
                }
                _db.SaveChanges();
                tx.Commit();
                _subject.Notify(new BookingEvent(booking.BookingId, booking.CustomerId,
                    booking.EventId, booking.Status, booking.TotalAmount));
                return new BookingOutcome(false, booking.BookingId, $"Payment failed: {result.Message}", total);
            }

            booking.Status = BookingStatus.Confirmed;
            foreach (var t in booking.Tickets) t.Status = "Confirmed";
            _db.SaveChanges();
            tx.Commit();

            _subject.Notify(new BookingEvent(booking.BookingId, booking.CustomerId,
                booking.EventId, booking.Status, booking.TotalAmount));

            return new BookingOutcome(true, booking.BookingId,
                $"Booking confirmed. {result.Message}", total);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return new BookingOutcome(false, 0, $"Internal error: {ex.Message}", 0);
        }
    }

    public bool CancelBooking(int bookingId, int requestingCustomerId)
    {
        var b = _db.Bookings
            .Include(x => x.Tickets)
            .ThenInclude(t => t.TicketCategory)
            .FirstOrDefault(x => x.BookingId == bookingId && x.CustomerId == requestingCustomerId);
        if (b == null || b.Status != BookingStatus.Confirmed) return false;

        b.Status = BookingStatus.Cancelled;
        foreach (var t in b.Tickets)
        {
            if (t.TicketCategory != null) t.TicketCategory.SoldQuantity = Math.Max(0, t.TicketCategory.SoldQuantity - 1);
            t.Status = "Cancelled";
        }
        if (b.Payment != null) b.Payment.Status = PaymentStatus.Refunded;
        _db.SaveChanges();

        _subject.Notify(new BookingEvent(b.BookingId, b.CustomerId, b.EventId, b.Status, b.TotalAmount));
        return true;
    }
}
