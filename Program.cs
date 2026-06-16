using EventBookingSystem.Data;
using EventBookingSystem.Patterns.Facade;
using EventBookingSystem.Patterns.Observer;
using EventBookingSystem.Repositories;
using EventBookingSystem.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<ReportingService>();
builder.Services.AddScoped<BookingFacade>();

builder.Services.AddSingleton<IBookingSubject>(sp =>
{
    var subject = new BookingSubject();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    subject.Attach(new CustomerObserver(scopeFactory));
    subject.Attach(new OrganizerObserver(scopeFactory));
    subject.Attach(new AdminObserver(scopeFactory));
    return subject;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(2);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
