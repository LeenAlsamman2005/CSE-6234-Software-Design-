using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    [MaxLength(40)]
    public string AudienceRole { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? BookingId { get; set; }
}
