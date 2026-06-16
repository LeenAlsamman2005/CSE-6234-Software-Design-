using EventBookingSystem.Models;

namespace EventBookingSystem.Services;

public static class SessionExtensions
{
    public const string UserIdKey = "UID";
    public const string UserNameKey = "UNAME";
    public const string UserRoleKey = "UROLE";

    public static void SignIn(this HttpContext ctx, User user)
    {
        ctx.Session.SetInt32(UserIdKey, user.UserId);
        ctx.Session.SetString(UserNameKey, user.Name);
        ctx.Session.SetString(UserRoleKey, user.Role.ToString());
    }

    public static void SignOut(this HttpContext ctx) => ctx.Session.Clear();

    public static int? CurrentUserId(this HttpContext ctx) => ctx.Session.GetInt32(UserIdKey);

    public static string? CurrentUserName(this HttpContext ctx) => ctx.Session.GetString(UserNameKey);

    public static UserRole? CurrentRole(this HttpContext ctx)
    {
        var s = ctx.Session.GetString(UserRoleKey);
        return Enum.TryParse<UserRole>(s, out var r) ? r : null;
    }

    public static bool IsAuthenticated(this HttpContext ctx) => ctx.CurrentUserId().HasValue;

    public static bool IsInRole(this HttpContext ctx, UserRole role) => ctx.CurrentRole() == role;
}
