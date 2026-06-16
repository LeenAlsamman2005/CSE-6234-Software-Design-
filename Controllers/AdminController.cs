using EventBookingSystem.Models;
using EventBookingSystem.Repositories;
using EventBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers;

public class AdminController : Controller
{
    private readonly IUserRepository _users;
    private readonly IBookingRepository _bookings;
    private readonly ReportingService _reporting;

    public AdminController(IUserRepository users, IBookingRepository bookings, ReportingService reporting)
    {
        _users = users;
        _bookings = bookings;
        _reporting = reporting;
    }

    private IActionResult? Guard()
    {
        if (!HttpContext.IsInRole(UserRole.Admin))
            return RedirectToAction("Login", "Account");
        return null;
    }

    public IActionResult Index()
    {
        var g = Guard(); if (g != null) return g;
        return View(_reporting.ForAdmin());
    }

    public IActionResult Users()
    {
        var g = Guard(); if (g != null) return g;
        return View(_users.All().ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ToggleUser(int id)
    {
        var g = Guard(); if (g != null) return g;
        var u = _users.FindById(id);
        if (u == null || u.Role == UserRole.Admin) return NotFound();
        u.IsActive = !u.IsActive;
        _users.Update(u);
        _users.Save();
        TempData["Success"] = $"User {u.Name} is now {(u.IsActive ? "active" : "inactive")}.";
        return RedirectToAction(nameof(Users));
    }

    public IActionResult Bookings()
    {
        var g = Guard(); if (g != null) return g;
        return View(_bookings.All().ToList());
    }

    public IActionResult Reports()
    {
        var g = Guard(); if (g != null) return g;
        return View(_reporting.ForAdmin());
    }
}
