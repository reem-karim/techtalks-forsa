using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Models;

public enum SessionStatus
{
    Requested,
    Confirmed,
    Completed,
    Cancelled
}

public class Session
{
    public int Id { get; set; }

    [Required]
    public int SwapRequestId { get; set; }
    public SwapRequest? SwapRequest { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Requested;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}