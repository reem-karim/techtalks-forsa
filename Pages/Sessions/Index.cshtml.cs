using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using techtalks_forsa.Data;
using techtalks_forsa.Models;

namespace techtalks_forsa.Pages.Sessions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Session> MySessions { get; set; } = new();

        public string? CurrentUserId { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            CurrentUserId = userId;

            MySessions = await _context.Sessions
                .AsNoTracking()
                .Include(s => s.SwapRequest)
                    .ThenInclude(r => r!.TargetListing)
                .Where(s => s.SwapRequest!.RequesterUserId == userId ||
                            s.SwapRequest!.TargetListing!.OwnerUserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public Task<IActionResult> OnPostConfirmAsync(int id) =>
            ChangeStatusAsync(id, SessionStatus.Confirmed, "Session confirmed.",
                SessionStatus.Requested);

        public Task<IActionResult> OnPostCompleteAsync(int id) =>
            ChangeStatusAsync(id, SessionStatus.Completed, "Session marked as completed.",
                SessionStatus.Confirmed);

        public Task<IActionResult> OnPostCancelAsync(int id) =>
            ChangeStatusAsync(id, SessionStatus.Cancelled, "Session cancelled.",
                SessionStatus.Requested, SessionStatus.Confirmed);

        // Only the two people in the swap may change a session, and only along allowed paths:
        // Requested -> Confirmed -> Completed, or Requested/Confirmed -> Cancelled.
        private async Task<IActionResult> ChangeStatusAsync(
            int id,
            SessionStatus newStatus,
            string successMessage,
            params SessionStatus[] allowedFrom)
        {
            var userId = _userManager.GetUserId(User);

            var session = await _context.Sessions
                .Include(s => s.SwapRequest)
                    .ThenInclude(r => r!.TargetListing)
                .FirstOrDefaultAsync(s => s.Id == id &&
                    (s.SwapRequest!.RequesterUserId == userId ||
                     s.SwapRequest!.TargetListing!.OwnerUserId == userId));

            if (session == null)
            {
                TempData["StatusMessage"] = "That session no longer exists.";
                return RedirectToPage();
            }

            if (!allowedFrom.Contains(session.Status))
            {
                TempData["StatusMessage"] =
                    $"This session is already {session.Status.ToString().ToLowerInvariant()}, so that action isn't available.";
                return RedirectToPage();
            }

            session.Status = newStatus;

            try
            {
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = successMessage;
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["StatusMessage"] = "This session was changed or removed in the meantime. Please review it again.";
            }

            return RedirectToPage();
        }
    }
}