using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Models
{
    public class MinorProfile
    {
        public int Id { get; set; }

        [Required]
        public int GuardianProfileId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(5, 18)]
        public int Age { get; set; }

        [StringLength(50)]
        public string? Grade { get; set; }

        public Profile? GuardianProfile { get; set; }
    }
}