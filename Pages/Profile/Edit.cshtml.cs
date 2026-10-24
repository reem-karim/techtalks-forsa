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

        // True until the user has saved a profile with a display name once (shows the setup steps).
        public bool IsFirstSetup { get; set; }

        public class ProfileInputModel
        {
            [Required(ErrorMessage = "Please enter a name.")]
            [StringLength(100)]
            [Display(Name = "Your name")]
            public string DisplayName { get; set; } = string.Empty;

            [StringLength(500)]
            [Display(Name = "Education")]
            public string? Education { get; set; }

            [StringLength(1000)]
            [Display(Name = "About you")]
            public string? Bio { get; set; }

            [Display(Name = "I am a guardian for a minor, and I am 18 or older")]
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

            IsFirstSetup = profile == null || string.IsNullOrWhiteSpace(profile.DisplayName);

            if (profile == null)
            {
                // Pre-fill a friendly default so the first step is one click.
                var email = (await _userManager.GetUserAsync(User))?.Email ?? string.Empty;
                var at = email.IndexOf('@');
                Input.DisplayName = at > 0 ? email.Substring(0, at) : string.Empty;
                return;
            }

            Input.DisplayName = profile.DisplayName ?? string.Empty;
            Input.Education = profile.Education;
            Input.Bio = profile.Bio;
            Input.IsGuardian = profile.IsGuardian;
            Input.RowVersion = profile.RowVersion;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var wasSetUp = profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName);
            IsFirstSetup = !wasSetUp;

            if (!ModelState.IsValid)
            {
                return Page();
            }

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
                _context.ChangeTracker.Clear();

                var existing = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (existing == null)
                {
                    throw;
                }

                Apply(existing);
                await _context.SaveChangesAsync();
            }

            // FLOW: first time -> go on to "add your skills". Later edits -> back to the profile.
            if (!wasSetUp)
            {
                return RedirectToPage("/Welcome/Skills");
            }

            TempData["StatusMessage"] = "Profile saved.";
            return RedirectToPage("/Profile/View");
        }

        private void Apply(techtalks_forsa.Models.Profile profile)
        {
            profile.DisplayName = Input.DisplayName.Trim();
            profile.Education = Input.Education;
            profile.Bio = Input.Bio;
            profile.IsGuardian = Input.IsGuardian;
            // Teach / WantToLearn on the Profile row are no longer edited here: skills live in "My skills" (listings).
        }
    }
}