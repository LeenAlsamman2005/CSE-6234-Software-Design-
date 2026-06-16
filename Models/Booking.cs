using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models;

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Rescheduled
}

public class Booking
{
    [Key]
    public int BookingId { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalAmount { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public Payment? Payment { get; set; }
}

public class Ticket
{
    [Key]
    public int TicketId { get; set; }

    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public int TicketCategoryId { get; set; }
    public TicketCategory? TicketCategory { get; set; }

    [MaxLength(20)]
    public string SeatNumber { get; set; } = string.Empty;

    public decimal Price { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Reserved";
}
