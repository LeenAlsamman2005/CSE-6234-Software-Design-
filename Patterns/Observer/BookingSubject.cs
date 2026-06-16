namespace EventBookingSystem.Patterns.Observer;

/// <summary>
/// Observer Pattern: concrete subject. Holds observers and broadcasts state changes.
/// Registered as singleton in DI so notifications flow across requests.
/// </summary>
public class BookingSubject : IBookingSubject
{
    private readonly List<IBookingObserver> _observers = new();
    private readonly object _lock = new();

    public void Attach(IBookingObserver observer)
    {
        lock (_lock) _observers.Add(observer);
    }

    public void Detach(IBookingObserver observer)
    {
        lock (_lock) _observers.Remove(observer);
    }

    public void Notify(BookingEvent e)
    {
        IBookingObserver[] snapshot;
        lock (_lock) snapshot = _observers.ToArray();
        foreach (var o in snapshot) o.Update(e);
    }
}
