using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace techtalks_forsa.Pages.Browse
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Listing? Listing { get; set; }
        public string OwnerDisplayName { get; set; } = "Forsa user";
        public bool IsOwnListing { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Listing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == id);

            if (Listing == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            IsOwnListing = Listing.OwnerUserId == userId;

            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == Listing.OwnerUserId);

            if (profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                OwnerDisplayName = profile.DisplayName;
            }

            return Page();
        }
    }
}