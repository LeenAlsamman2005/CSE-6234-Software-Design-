using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models;

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed,
    Refunded
}

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(40)]
    public string PaymentMethod { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    [MaxLength(120)]
    public string Reference { get; set; } = string.Empty;
}
