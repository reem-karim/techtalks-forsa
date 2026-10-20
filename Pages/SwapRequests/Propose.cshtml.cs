using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Pages.SwapRequests
{
    [Authorize]
    public class ProposeModel : PageModel
    {
        // B1: after a request was declined the requester has to wait before asking for the same listing again.
        private static readonly TimeSpan DeclineCooldown = TimeSpan.FromDays(7);

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProposeModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int ListingId { get; set; }

        public Listing? TargetListing { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> MyOfferOptions { get; set; } = new();

        public class InputModel
        {
            public int? OfferedListingId { get; set; }

            [StringLength(500)]
            public string? Message { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // B3b: only "Offer" listings can be the target of a swap request.
            TargetListing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == ListingId && l.Type == ListingType.Offer);
            if (TargetListing == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (TargetListing.OwnerUserId == userId) return Forbid();

            await LoadOptionsAsync(userId!);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            TargetListing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == ListingId && l.Type == ListingType.Offer);
            if (TargetListing == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();
            if (TargetListing.OwnerUserId == userId) return Forbid();

            // B3: enforce [StringLength(500)] instead of letting SQL throw a truncation error.
            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync(userId);
                return Page();
            }

            // B2: the offered listing must be one of MY "Offer" listings (or empty).
            if (Input.OfferedListingId != null)
            {
                var offeredOk = await _context.Listings.AnyAsync(l =>
                    l.Id == Input.OfferedListingId &&
                    l.OwnerUserId == userId &&
                    l.Type == ListingType.Offer);

                if (!offeredOk)
                {
                    ModelState.AddModelError(string.Empty, "Please choose one of your own \"I can teach\" listings, or leave it empty.");
                    await LoadOptionsAsync(userId);
                    return Page();
                }
            }

            // A2 / B1: one open request per requester + listing, cooldown after a decline.
            var blocker = await FindBlockingRequestAsync(userId);
            if (blocker != null)
            {
                return await RejectAsync(userId, blocker);
            }

            var swapRequest = new SwapRequest
            {
                RequesterUserId = userId,
                TargetListingId = TargetListing.Id,
                OfferedListingId = Input.OfferedListingId,
                Message = Input.Message,
                Status = SwapRequestStatus.Pending
            };

            _context.SwapRequests.Add(swapRequest);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // A double-click / second tab won the race (unique index), or the listing was deleted meanwhile (A7).
                _context.ChangeTracker.Clear();

                var targetStillThere = await _context.Listings.AnyAsync(l => l.Id == ListingId);
                if (!targetStillThere)
                {
                    return NotFound();
                }

                var existing = await FindBlockingRequestAsync(userId);
                if (existing != null)
                {
                    return await RejectAsync(userId, existing);
                }

                throw;
            }

            return RedirectToPage("/SwapRequests/Sent");
        }

        private async Task<SwapRequest?> FindBlockingRequestAsync(string userId)
        {
            var cutoff = DateTime.UtcNow - DeclineCooldown;

            return await _context.SwapRequests
                .AsNoTracking()
                .Where(r => r.RequesterUserId == userId &&
                            r.TargetListingId == ListingId &&
                            (r.Status != SwapRequestStatus.Declined ||
                             (r.RespondedAt ?? r.CreatedAt) > cutoff))
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        private async Task<IActionResult> RejectAsync(string userId, SwapRequest blocker)
        {
            // A double-click that already created the request should simply land on "Sent", not on an error.
            if (blocker.Status == SwapRequestStatus.Pending &&
                blocker.CreatedAt > DateTime.UtcNow.AddSeconds(-30) &&
                blocker.OfferedListingId == Input.OfferedListingId &&
                blocker.Message == Input.Message)
            {
                return RedirectToPage("/SwapRequests/Sent");
            }

            ModelState.AddModelError(string.Empty, blocker.Status switch
            {
                SwapRequestStatus.Pending => "You already have a pending request for this listing. Wait for the owner to answer it.",
                SwapRequestStatus.Accepted => "Your request for this listing was already accepted. You can message the owner from Requests.",
                _ => "This request was declined recently. Please wait a few days before asking for the same listing again."
            });

            await LoadOptionsAsync(userId);
            return Page();
        }

        private async Task LoadOptionsAsync(string userId)
        {
            var myOffers = await _context.Listings
                .Where(l => l.OwnerUserId == userId && l.Type == ListingType.Offer)
                .OrderBy(l => l.Title)
                .ToListAsync();

            MyOfferOptions = myOffers
                .Select(l => new SelectListItem(l.Title, l.Id.ToString()))
                .ToList();
        }
    }
}
