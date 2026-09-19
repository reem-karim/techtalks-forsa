using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace techtalks_forsa.Pages.Browse
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

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        public List<ListingWithOwner> Results { get; set; } = new();

        public List<SelectListItem> CategoryOptions => SkillCategories.All
            .Select(c => new SelectListItem(c, c)).ToList();

        public class ListingWithOwner
        {
            public Listing Listing { get; set; } = null!;
            public string OwnerDisplayName { get; set; } = "Forsa user";
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Listings
                .Where(l => l.Type == ListingType.Offer && l.OwnerUserId != userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Category))
            {
                query = query.Where(l => l.Category == Category);
            }

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                query = query.Where(l =>
                    l.Title.Contains(Keyword) ||
                    (l.Description != null && l.Description.Contains(Keyword)));
            }

            var listings = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var ownerIds = listings.Select(l => l.OwnerUserId).Distinct().ToList();

            var profiles = await _context.Profiles
                .Where(p => ownerIds.Contains(p.UserId))
                .ToListAsync();

            Results = listings.Select(l => new ListingWithOwner
            {
                Listing = l,
                OwnerDisplayName = profiles.FirstOrDefault(p => p.UserId == l.OwnerUserId)?.DisplayName
                    is string name && !string.IsNullOrWhiteSpace(name) ? name : "Forsa user"
            }).ToList();
        }
    }
}