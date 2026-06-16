using EventBookingSystem.Models;
using EventBookingSystem.Repositories;
using EventBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers;

public class OrganizerController : Controller
{
    private readonly EventService _events;
    private readonly IBookingRepository _bookings;
    private readonly ReportingService _reporting;

    public OrganizerController(EventService events, IBookingRepository bookings, ReportingService reporting)
    {
        _events = events;
        _bookings = bookings;
        _reporting = reporting;
    }

    private IActionResult? Guard()
    {
        if (!HttpContext.IsInRole(UserRole.Organizer))
            return RedirectToAction("Login", "Account");
        return null;
    }

    public IActionResult Index()
    {
        var g = Guard(); if (g != null) return g;
        var uid = HttpContext.CurrentUserId()!.Value;
        ViewBag.Report = _reporting.ForOrganizer(uid);
        return View(_events.ListByOrganizer(uid).ToList());
    }

    [HttpGet]
    public IActionResult Create()
    {
        var g = Guard(); if (g != null) return g;
        ViewBag.Categories = _events.Categories().Where(c => c != "All");
        return View(new Event { EventDate = DateTime.UtcNow.AddDays(14) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Event ev, string[] catName, string[] catPrice, string[] catQty)
    {
        var g = Guard(); if (g != null) return g;
        ev.OrganizerId = HttpContext.CurrentUserId()!.Value;
        ev.Categories = new List<TicketCategory>();
        // String arrays + defensive parsing: the binder can drop empty entries,
        // leaving the arrays at different lengths.
        for (int i = 0; i < (catName?.Length ?? 0); i++)
        {
            if (string.IsNullOrWhiteSpace(catName![i])) continue;
            decimal price = 0;
            int qty = 0;
            if (i < (catPrice?.Length ?? 0)) decimal.TryParse(catPrice![i], out price);
            if (i < (catQty?.Length ?? 0)) int.TryParse(catQty![i], out qty);
            if (price < 0 || qty <= 0) continue;
            ev.Categories.Add(new TicketCategory
            {
                Name = catName[i].Trim(),
                Price = price,
                TotalQuantity = qty
            });
        }
        if (!ev.Categories.Any())
        {
            TempData["Error"] = "Add at least one ticket category.";
            ViewBag.Categories = _events.Categories().Where(c => c != "All");
            return View(ev);
        }
        _events.Create(ev);
        TempData["Success"] = "Event created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var g = Guard(); if (g != null) return g;
        var ev = _events.Get(id);
        if (ev == null || ev.OrganizerId != HttpContext.CurrentUserId()) return NotFound();
        ViewBag.Categories = _events.Categories().Where(c => c != "All");
        return View(ev);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Event form)
    {
        var g = Guard(); if (g != null) return g;
        var ev = _events.Get(id);
        if (ev == null || ev.OrganizerId != HttpContext.CurrentUserId()) return NotFound();
        ev.Title = form.Title;
        ev.Description = form.Description;
        ev.Venue = form.Venue;
        ev.EventDate = form.EventDate;
        ev.Category = form.Category;
        ev.Status = form.Status;
        _events.Update(ev);
        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var g = Guard(); if (g != null) return g;
        var ok = _events.Delete(id, HttpContext.CurrentUserId()!.Value);
        TempData[ok ? "Success" : "Error"] = ok ? "Event deleted." : "Cannot delete; active bookings exist.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Bookings()
    {
        var g = Guard(); if (g != null) return g;
        var uid = HttpContext.CurrentUserId()!.Value;
        return View(_bookings.ForOrganizer(uid).ToList());
    }

    public IActionResult Reports()
    {
        var g = Guard(); if (g != null) return g;
        var uid = HttpContext.CurrentUserId()!.Value;
        return View(_reporting.ForOrganizer(uid));
    }
}
