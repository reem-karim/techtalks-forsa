using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace techtalks_forsa.Pages.SwapRequests
{
    [Authorize]
    public class IncomingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IncomingModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<SwapRequest> Requests { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            Requests = await _context.SwapRequests
                .Include(r => r.TargetListing)
                .Include(r => r.OfferedListing)
                .Where(r => r.TargetListing != null &&
                            r.TargetListing.OwnerUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostRespondAsync(
            int id,
            string decision)
        {
            var userId = _userManager.GetUserId(User);

            var request = await _context.SwapRequests
                .Include(r => r.TargetListing)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request?.TargetListing == null ||
                request.TargetListing.OwnerUserId != userId)
            {
                return NotFound();
            }

            request.Status = decision == "accept"
                ? SwapRequestStatus.Accepted
                : SwapRequestStatus.Declined;

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}