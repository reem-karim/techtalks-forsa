using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace techtalks_forsa.Pages.Profile
{
    public class ViewModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ViewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Explicitly tell C# that Profile refers to the MODEL
        public techtalks_forsa.Models.Profile? Profile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Challenge();
            }

            Profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (Profile == null)
            {
                return RedirectToPage("/Profile/Edit");
            }

            return Page();
        }
    }
}