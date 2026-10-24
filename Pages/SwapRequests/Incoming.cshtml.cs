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
            string? decision)
        {
            // A1: only accept the two known values; anything else is a bad request (no more fail-open "Declined").
            SwapRequestStatus newStatus;
            if (string.Equals(decision, "accept", StringComparison.Ordinal))
            {
                newStatus = SwapRequestStatus.Accepted;
            }
            else if (string.Equals(decision, "decline", StringComparison.Ordinal))
            {
                newStatus = SwapRequestStatus.Declined;
            }
            else
            {
                return BadRequest();
            }

            var userId = _userManager.GetUserId(User);

            var request = await _context.SwapRequests
                .Include(r => r.TargetListing)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request?.TargetListing == null ||
                request.TargetListing.OwnerUserId != userId)
            {
                // A7: the request (or its listing) may have been deleted in another tab.
                TempData["StatusMessage"] = "That request no longer exists.";
                return RedirectToPage();
            }

            // A1: a request can only be answered once.
            if (request.Status != SwapRequestStatus.Pending)
            {
                TempData["StatusMessage"] = $"This request was already {request.Status.ToString().ToLowerInvariant()}.";
                return RedirectToPage();
            }

            request.Status = newStatus;
            request.RespondedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = newStatus == SwapRequestStatus.Accepted
                    ? "Request accepted. Send them a message or schedule a session below."
                    : "Request declined.";
            }
            catch (DbUpdateConcurrencyException)
            {
                // Someone else (another tab / the listing delete) changed or removed the row first.
                TempData["StatusMessage"] = "This request was changed or removed in the meantime. Please review it again.";
            }

            return RedirectToPage();
        }
    }
}