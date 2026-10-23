using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Pages.Profile.Minor
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            [Display(Name = "Child's Name")]
            public string Name { get; set; } = string.Empty;

            [Range(5, 17, ErrorMessage = "Age must be between 5 and 17. Anyone 18 or older needs their own account.")]
            [Display(Name = "Age")]
            public int Age { get; set; }

            [StringLength(50)]
            [Display(Name = "Grade")]
            public string? Grade { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var guardianProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (guardianProfile == null || !guardianProfile.IsGuardian)
            {
                return Forbid();
            }

            var name = Input.Name.Trim();

            // A4: same child twice = double-click / retry. Don't create a copy, just open the existing profile.
            var existing = await _context.MinorProfiles.FirstOrDefaultAsync(m =>
                m.GuardianProfileId == guardianProfile.Id &&
                m.Name == name &&
                m.Age == Input.Age);

            if (existing != null)
            {
                return RedirectToPage("/Profile/Minor/View", new { id = existing.Id });
            }

            var minorProfile = new MinorProfile
            {
                GuardianProfileId = guardianProfile.Id,
                Name = name,
                Age = Input.Age,
                Grade = Input.Grade
            };

            _context.MinorProfiles.Add(minorProfile);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Profile/Minor/View",
                new { id = minorProfile.Id });
        }
    }
}