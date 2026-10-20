using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Pages.Listings
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public List<SelectListItem> CategoryOptions => SkillCategories.All
            .Select(c => new SelectListItem(c, c)).ToList();

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            public string Title { get; set; } = string.Empty;

            [StringLength(1000)]
            public string? Description { get; set; }

            [Required]
            public string Category { get; set; } = string.Empty;

            [Required]
            public ListingType? Type { get; set; }

            // Concurrency token round-tripped through the form (A9).
            public byte[]? RowVersion { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == Id && l.OwnerUserId == userId);

            if (listing == null)
            {
                return NotFound();
            }

            Input.Title = listing.Title;
            Input.Description = listing.Description;
            Input.Category = listing.Category;
            Input.Type = listing.Type;
            Input.RowVersion = listing.RowVersion;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // B6
            if (!string.IsNullOrEmpty(Input.Category) && !SkillCategories.All.Contains(Input.Category))
            {
                ModelState.AddModelError("Input.Category", "Please choose a category from the list.");
            }
            if (Input.Type != null && !Enum.IsDefined(Input.Type.Value))
            {
                ModelState.AddModelError("Input.Type", "Please choose a valid listing type.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == Id && l.OwnerUserId == userId);

            if (listing == null)
            {
                return NotFound();
            }

            var newType = Input.Type!.Value;

            // B4: changing Offer <-> Learn would break open requests that point at this listing.
            if (listing.Type != newType)
            {
                var hasOpenRequests = await _context.SwapRequests.AnyAsync(r =>
                    (r.Status == SwapRequestStatus.Pending || r.Status == SwapRequestStatus.Accepted) &&
                    (r.TargetListingId == listing.Id || r.OfferedListingId == listing.Id));

                if (hasOpenRequests)
                {
                    ModelState.AddModelError("Input.Type",
                        "You can't change the type while this listing has pending or accepted swap requests.");
                    return Page();
                }
            }

            // A9: detect that the listing was changed in another tab since this form was loaded.
            if (Input.RowVersion is { Length: > 0 })
            {
                _context.Entry(listing).Property(l => l.RowVersion).OriginalValue = Input.RowVersion;
            }

            listing.Title = Input.Title.Trim();
            listing.Description = Input.Description;
            listing.Category = Input.Category;
            listing.Type = newType;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.ChangeTracker.Clear();

                var current = await _context.Listings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == Id && l.OwnerUserId == userId);

                if (current == null)
                {
                    TempData["StatusMessage"] = "That listing no longer exists.";
                    return RedirectToPage("/Listings/Index");
                }

                // Show the user's values again with the fresh token: pressing Save once more overwrites knowingly.
                Input.RowVersion = current.RowVersion;
                ModelState.Remove("Input.RowVersion");
                ModelState.AddModelError(string.Empty,
                    "This listing was changed in another window since you opened it. Press Save again to overwrite those changes, or go back to keep them.");
                return Page();
            }

            return RedirectToPage("/Listings/Index");
        }
    }
}
