using EventBookingSystem.Models;
using EventBookingSystem.Patterns.Facade;
using EventBookingSystem.Repositories;
using EventBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers;

public class BookingsController : Controller
{
    private readonly EventService _events;
    private readonly BookingFacade _facade;
    private readonly IBookingRepository _bookings;

    public BookingsController(EventService events, BookingFacade facade, IBookingRepository bookings)
    {
        _events = events;
        _facade = facade;
        _bookings = bookings;
    }

    private IActionResult? RequireCustomer()
    {
        if (!HttpContext.IsInRole(UserRole.Customer))
            return RedirectToAction("Login", "Account");
        return null;
    }

    [HttpGet]
    public IActionResult Checkout(int eventId)
    {
        var guard = RequireCustomer();
        if (guard != null) return guard;
        var ev = _events.Get(eventId);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Checkout(int eventId,
        string paymentMethod, string? cardNumber, string? bank, string? walletId)
    {
        var guard = RequireCustomer();
        if (guard != null) return guard;
        var ev = _events.Get(eventId);
        if (ev == null) return NotFound();

        // Parse quantities[ID] form fields manually; the default Dictionary<int,int>
        // binder throws when unrelated form keys are present.
        var lines = new List<BookingLine>();
        foreach (var key in Request.Form.Keys)
        {
            var m = System.Text.RegularExpressions.Regex.Match(key, @"^quantities\[(\d+)\]$");
            if (!m.Success) continue;
            if (int.TryParse(m.Groups[1].Value, out var catId) &&
                int.TryParse(Request.Form[key], out var qty) && qty > 0)
            {
                lines.Add(new BookingLine(catId, qty));
            }
        }

        if (!lines.Any())
        {
            TempData["Error"] = "Please select at least one ticket.";
            return RedirectToAction(nameof(Checkout), new { eventId });
        }

        var details = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(cardNumber)) details["CardNumber"] = cardNumber;
        if (!string.IsNullOrWhiteSpace(bank)) details["Bank"] = bank;
        if (!string.IsNullOrWhiteSpace(walletId)) details["WalletId"] = walletId;

        var req = new BookingRequest(
            HttpContext.CurrentUserId()!.Value,
            eventId,
            lines,
            paymentMethod,
            details);

        var outcome = _facade.ProcessFullBooking(req);
        if (!outcome.Success)
        {
            TempData["Error"] = outcome.Message;
            return RedirectToAction(nameof(Checkout), new { eventId });
        }
        TempData["Success"] = outcome.Message;
        return RedirectToAction(nameof(Confirmation), new { id = outcome.BookingId });
    }

    public IActionResult Confirmation(int id)
    {
        var guard = RequireCustomer();
        if (guard != null) return guard;
        var b = _bookings.FindWithDetails(id);
        if (b == null || b.CustomerId != HttpContext.CurrentUserId()) return NotFound();
        return View(b);
    }

    public IActionResult History()
    {
        var guard = RequireCustomer();
        if (guard != null) return guard;
        var uid = HttpContext.CurrentUserId()!.Value;
        return View(_bookings.ForCustomer(uid).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        var guard = RequireCustomer();
        if (guard != null) return guard;
        var ok = _facade.CancelBooking(id, HttpContext.CurrentUserId()!.Value);
        TempData[ok ? "Success" : "Error"] = ok ? "Booking cancelled." : "Cannot cancel this booking.";
        return RedirectToAction(nameof(History));
    }
}
