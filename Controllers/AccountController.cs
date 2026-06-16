using EventBookingSystem.Models;
using EventBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;
    public AccountController(AuthService auth) => _auth = auth;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password)
    {
        var r = _auth.Login(email ?? "", password ?? "");
        if (!r.Success || r.User == null) { ViewBag.Error = r.Message; return View(); }
        HttpContext.SignIn(r.User);
        TempData["Success"] = $"Welcome back, {r.User.Name}.";
        return Redirect(r.User.Dashboard);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Register(string name, string email, string password, string role)
    {
        if (!Enum.TryParse<UserRole>(role, out var parsed))
        { ViewBag.Error = "Invalid role."; return View(); }
        var r = _auth.Register(name ?? "", email ?? "", password ?? "", parsed);
        if (!r.Success || r.User == null) { ViewBag.Error = r.Message; return View(); }
        HttpContext.SignIn(r.User);
        TempData["Success"] = "Account created. Welcome!";
        return Redirect(r.User.Dashboard);
    }

    public IActionResult Logout()
    {
        HttpContext.SignOut();
        return RedirectToAction("Index", "Home");
    }
}
