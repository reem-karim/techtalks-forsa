using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Models;

public enum SwapRequestStatus
{
    Pending,
    Accepted,
    Declined
}

public class SwapRequest
{
    public int Id { get; set; }

    [Required]
    public string RequesterUserId { get; set; } = string.Empty;

    [Required]
    public int TargetListingId { get; set; }
    public Listing? TargetListing { get; set; }

    public int? OfferedListingId { get; set; }
    public Listing? OfferedListing { get; set; }

    [StringLength(500)]
    public string? Message { get; set; }

    public SwapRequestStatus Status { get; set; } = SwapRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}