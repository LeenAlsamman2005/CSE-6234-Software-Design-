using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models;

public enum EventStatus
{
    Draft,
    Published,
    Cancelled,
    Completed
}

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Published;

    [MaxLength(80)]
    public string Category { get; set; } = "General";

    public int OrganizerId { get; set; }
    public Organizer? Organizer { get; set; }

    public ICollection<TicketCategory> Categories { get; set; } = new List<TicketCategory>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class TicketCategory
{
    [Key]
    public int TicketCategoryId { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required, MaxLength(60)]
    public string Name { get; set; } = "Standard";

    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int TotalQuantity { get; set; }

    [Range(0, 100000)]
    public int SoldQuantity { get; set; }

    public int Available => TotalQuantity - SoldQuantity;
}
