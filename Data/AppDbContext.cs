using EventBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>()
            .HasDiscriminator(u => u.Role)
            .HasValue<Customer>(UserRole.Customer)
            .HasValue<Organizer>(UserRole.Organizer)
            .HasValue<Admin>(UserRole.Admin);

        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();

        mb.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<TicketCategory>()
            .HasOne(t => t.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Booking>()
            .HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Ticket>()
            .HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Ticket>()
            .HasOne(t => t.TicketCategory)
            .WithMany()
            .HasForeignKey(t => t.TicketCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithOne(b => b.Payment!)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<TicketCategory>().Property(t => t.Price).HasColumnType("decimal(10,2)");
        mb.Entity<Booking>().Property(b => b.TotalAmount).HasColumnType("decimal(10,2)");
        mb.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(10,2)");
        mb.Entity<Ticket>().Property(t => t.Price).HasColumnType("decimal(10,2)");
    }
}
