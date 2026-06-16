using EventBookingSystem.Models;

namespace EventBookingSystem.Patterns.Observer;

public record BookingEvent(int BookingId, int CustomerId, int EventId, BookingStatus Status, decimal Amount);

/// <summary>
/// Observer Pattern: observer interface for booking state changes.
/// </summary>
public interface IBookingObserver
{
    void Update(BookingEvent e);
}

/// <summary>
/// Observer Pattern: subject interface.
/// </summary>
public interface IBookingSubject
{
    void Attach(IBookingObserver observer);
    void Detach(IBookingObserver observer);
    void Notify(BookingEvent e);
}
