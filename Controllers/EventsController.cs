using EventBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers;

public class EventsController : Controller
{
    private readonly EventService _events;
    public EventsController(EventService events) => _events = events;

    public IActionResult Index(string? search, string? category)
    {
        ViewBag.Categories = _events.Categories();
        ViewBag.Search = search;
        ViewBag.Category = category ?? "All";
        return View(_events.ListPublished(search, category).ToList());
    }

    public IActionResult Details(int id)
    {
        var ev = _events.Get(id);
        if (ev == null) return NotFound();
        return View(ev);
    }
}
