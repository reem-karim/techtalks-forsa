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
            TargetListing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == ListingId);
            if (TargetListing == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (TargetListing.OwnerUserId == userId) return Forbid();

            var myOffers = await _context.Listings
                .Where(l => l.OwnerUserId == userId && l.Type == ListingType.Offer)
                .ToListAsync();

            MyOfferOptions = myOffers
                .Select(l => new SelectListItem(l.Title, l.Id.ToString()))
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            TargetListing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == ListingId);
            if (TargetListing == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();
            if (TargetListing.OwnerUserId == userId) return Forbid();

            var swapRequest = new SwapRequest
            {
                RequesterUserId = userId,
                TargetListingId = TargetListing.Id,
                OfferedListingId = Input.OfferedListingId,
                Message = Input.Message,
                Status = SwapRequestStatus.Pending
            };

            _context.SwapRequests.Add(swapRequest);
            await _context.SaveChangesAsync();

            return RedirectToPage("/SwapRequests/Sent");
        }
    }
}