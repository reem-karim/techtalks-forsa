using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Models;

public class Profile
{
    public int Id { get; set; }

    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Education { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? Teach { get; set; }

    [StringLength(500)]
    public string? WantToLearn { get; set; }

    public bool IsGuardian {get; set;}

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}