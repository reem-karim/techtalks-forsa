using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace techtalks_forsa.Pages.Listings
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Listing> MyListings { get; set; } = new();

        // Listing id -> number of swap requests that would be deleted together with the listing (B5 warning).
        public Dictionary<int, int> RequestCounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            MyListings = await _context.Listings
                .Where(l => l.OwnerUserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var ids = MyListings.Select(l => l.Id).ToList();

            RequestCounts = await _context.SwapRequests
                .Where(r => ids.Contains(r.TargetListingId))
                .GroupBy(r => r.TargetListingId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == id && l.OwnerUserId == userId);

            if (listing == null)
            {
                // A8: deleting twice (double-click / second tab) is not an error, the listing is simply gone.
                return RedirectToPage();
            }

            // B5: never silently wipe a swap that was already agreed (or has sessions).
            var inAgreedSwap = await _context.SwapRequests.AnyAsync(r =>
                r.Status == SwapRequestStatus.Accepted &&
                (r.TargetListingId == id || r.OfferedListingId == id));

            var hasSessions = await _context.Sessions.AnyAsync(s =>
                s.SwapRequest != null &&
                (s.SwapRequest.TargetListingId == id || s.SwapRequest.OfferedListingId == id));

            if (inAgreedSwap || hasSessions)
            {
                TempData["StatusMessage"] = $"\"{listing.Title}\" is part of an accepted swap and can't be deleted.";
                return RedirectToPage();
            }

            try
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                // A6: detach this listing from other requests that offered it, otherwise the FK blocks the delete.
                await _context.SwapRequests
                    .Where(r => r.OfferedListingId == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.OfferedListingId, (int?)null));

                _context.Listings.Remove(listing);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // A7: already deleted by a parallel request.
            }
            catch (DbUpdateException)
            {
                // A7: a request referencing the listing appeared at the same moment. Ask the user to retry.
                TempData["StatusMessage"] = "Someone just sent a request for this listing. Please review it and try again.";
            }

            return RedirectToPage();
        }
    }
}
