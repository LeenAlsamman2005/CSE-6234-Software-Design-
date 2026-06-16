using EventBookingSystem.Models;
using EventBookingSystem.Repositories;

namespace EventBookingSystem.Services;

public class EventService
{
    private readonly IEventRepository _events;
    public EventService(IEventRepository events) => _events = events;

    public IEnumerable<Event> ListPublished(string? search = null, string? category = null)
    {
        var items = _events.Published();
        if (!string.IsNullOrWhiteSpace(search))
            items = items.Where(e => e.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                                  || e.Venue.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            items = items.Where(e => e.Category == category);
        return items;
    }

    public IEnumerable<Event> ListByOrganizer(int organizerId) => _events.ByOrganizer(organizerId);

    public Event? Get(int id) => _events.FindWithDetails(id);

    public void Create(Event ev) { _events.Add(ev); _events.Save(); }

    public void Update(Event ev) { _events.Update(ev); _events.Save(); }

    public bool Delete(int id, int organizerId)
    {
        var ev = _events.FindWithDetails(id);
        if (ev == null || ev.OrganizerId != organizerId) return false;
        if (ev.Bookings.Any(b => b.Status == BookingStatus.Confirmed)) return false;
        _events.Remove(ev);
        _events.Save();
        return true;
    }

    public IEnumerable<string> Categories() =>
        new[] { "All", "Concert", "Conference", "Festival", "Sports", "Workshop", "General" };
}
