using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using techtalks_forsa.Data;
using techtalks_forsa.Models;

namespace techtalks_forsa.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public string DisplayName { get; set; } = string.Empty;
    public bool HasOffer { get; set; }
    public bool HasLearn { get; set; }
    public int SentCount { get; set; }
    public int PendingIncoming { get; set; }

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true) return;

        var userId = _userManager.GetUserId(User);
        if (userId == null) return;

        var profile = await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
        DisplayName = profile?.DisplayName ?? string.Empty;

        HasOffer = await _context.Listings.AnyAsync(l => l.OwnerUserId == userId && l.Type == ListingType.Offer);
        HasLearn = await _context.Listings.AnyAsync(l => l.OwnerUserId == userId && l.Type == ListingType.Learn);
        SentCount = await _context.SwapRequests.CountAsync(r => r.RequesterUserId == userId);
        PendingIncoming = await _context.SwapRequests.CountAsync(r =>
            r.Status == SwapRequestStatus.Pending &&
            r.TargetListing != null &&
            r.TargetListing.OwnerUserId == userId);
    }
}