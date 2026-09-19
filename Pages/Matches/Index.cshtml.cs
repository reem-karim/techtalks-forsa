using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace techtalks_forsa.Pages.Matches
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

        public class MatchGroup
        {
            public Listing LearnListing { get; set; } = null!;
            public List<Listing> MatchingOffers { get; set; } = new();
        }

        public List<MatchGroup> Groups { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var myLearnListings = await _context.Listings
                .Where(l => l.OwnerUserId == userId && l.Type == ListingType.Learn)
                .ToListAsync();

            var allOffers = await _context.Listings
                .Where(l => l.Type == ListingType.Offer && l.OwnerUserId != userId)
                .ToListAsync();

            foreach (var learn in myLearnListings)
            {
                Groups.Add(new MatchGroup
                {
                    LearnListing = learn,
                    MatchingOffers = allOffers.Where(o => o.Category == learn.Category).ToList()
                });
            }
        }
    }
}