using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Models;

public enum ListingType
{
    Offer,  // "I can teach this"
    Learn   // "I want to learn this"
}

public class Listing
{
    public int Id { get; set; }

    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public ListingType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/*The idea: One Listing covers both "I can teach this" and "I want to learn this"
same shape, different Type. That's what lets Epic 3's matching logic just compare Type and Category.*/