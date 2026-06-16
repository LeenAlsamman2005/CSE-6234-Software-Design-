using EventBookingSystem.Repositories;
using EventBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers;

public class HomeController : Controller
{
    private readonly EventService _events;
    private readonly INotificationRepository _notifications;

    public HomeController(EventService events, INotificationRepository notifications)
    {
        _events = events;
        _notifications = notifications;
    }

    public IActionResult Index()
    {
        ViewBag.UpcomingEvents = _events.ListPublished().Take(6).ToList();
        return View();
    }

    public IActionResult Notifications()
    {
        var uid = HttpContext.CurrentUserId();
        if (uid == null) return RedirectToAction("Login", "Account");
        var items = _notifications.ForUser(uid.Value).ToList();
        _notifications.MarkRead(uid.Value);
        return View(items);
    }

    public IActionResult Error() => View();
}
