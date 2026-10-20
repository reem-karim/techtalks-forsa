using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Pages.Profile
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public ProfileInputModel Input { get; set; } = new();

        public class ProfileInputModel
        {
            [Required]
            [StringLength(100)]
            [Display(Name = "Display Name")]
            public string DisplayName { get; set; } = string.Empty;

            [StringLength(500)]
            [Display(Name = "Education")]
            public string? Education { get; set; }

            [StringLength(1000)]
            [Display(Name = "Bio")]
            public string? Bio { get; set; }

            [StringLength(500)]
            [Display(Name = "I Teach")]
            public string? Teach { get; set; }

            [StringLength(500)]
            [Display(Name = "I Want to Learn")]
            public string? WantToLearn { get; set; }

            [Display(Name = "I am a Guardian (for minors) and I am an adult (18 yrs old or older)")]
            public bool IsGuardian { get; set; }

            // Concurrency token round-tripped through the form (A9).
            public byte[]? RowVersion { get; set; }
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return;

            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return;

            Input.DisplayName = profile.DisplayName ?? string.Empty;
            Input.Education = profile.Education;
            Input.Bio = profile.Bio;
            Input.Teach = profile.Teach;
            Input.WantToLearn = profile.WantToLearn;
            Input.IsGuardian = profile.IsGuardian;
            Input.RowVersion = profile.RowVersion;
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

            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var isNew = profile == null;

            if (profile == null)
            {
                profile = new techtalks_forsa.Models.Profile
                {
                    UserId = userId
                };
                _context.Profiles.Add(profile);
            }
            else if (Input.RowVersion is { Length: > 0 })
            {
                // A9: detect that the profile was changed in another tab since this form was loaded.
                _context.Entry(profile).Property(p => p.RowVersion).OriginalValue = Input.RowVersion;
            }

            Apply(profile);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.ChangeTracker.Clear();

                var current = await _context.Profiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (current != null)
                {
                    Input.RowVersion = current.RowVersion;
                    ModelState.Remove("Input.RowVersion");
                    ModelState.AddModelError(string.Empty,
                        "Your profile was changed in another window since you opened it. Press Save again to overwrite those changes.");
                    return Page();
                }

                throw;
            }
            catch (DbUpdateException) when (isNew)
            {
                // A5: a double-click / second tab created the profile first (unique index on UserId).
                // Load that profile and update it instead of creating a second one.
                _context.ChangeTracker.Clear();

                var existing = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (existing == null)
                {
                    throw;
                }

                Apply(existing);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Profile/View");
        }

        private void Apply(techtalks_forsa.Models.Profile profile)
        {
            profile.DisplayName = Input.DisplayName;
            profile.Education = Input.Education;
            profile.Bio = Input.Bio;
            profile.Teach = Input.Teach;
            profile.WantToLearn = Input.WantToLearn;
            profile.IsGuardian = Input.IsGuardian;
        }
    }
}
