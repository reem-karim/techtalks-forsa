using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace techtalks_forsa.Pages.Profile.Minor
{
    [Authorize]
    public class ViewModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ViewModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public MinorProfile? MinorProfile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
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

            MinorProfile = await _context.MinorProfiles
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.GuardianProfileId == guardianProfile.Id);

            if (MinorProfile == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}