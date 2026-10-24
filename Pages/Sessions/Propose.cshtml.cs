using System.ComponentModel.DataAnnotations;
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
    public class ProposeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProposeModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int SwapRequestId { get; set; }

        public string ListingTitle { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Please choose a date and time.")]
            [Display(Name = "Date and time")]
            public DateTime? ScheduledAt { get; set; }
        }

        // Loads the swap request and checks the current user may schedule a session for it.
        // Returns a result that sends the user away, or null when everything is fine.
        private async Task<IActionResult?> LoadAsync()
        {
            var userId = _userManager.GetUserId(User);

            var swap = await _context.SwapRequests
                .AsNoTracking()
                .Include(r => r.TargetListing)
                .FirstOrDefaultAsync(r => r.Id == SwapRequestId);

            // Missing, or the user is not part of this swap: same answer, so we never reveal which.
            if (swap?.TargetListing == null ||
                (swap.RequesterUserId != userId && swap.TargetListing.OwnerUserId != userId))
            {
                return NotFound();
            }

            if (swap.Status != SwapRequestStatus.Accepted)
            {
                TempData["StatusMessage"] = "You can schedule a session only after a swap request has been accepted.";
                return RedirectToPage("/Sessions/Index");
            }

            // The database allows one session per swap. A cancelled one may be rescheduled; any other may not.
            var hasActiveSession = await _context.Sessions
                .AsNoTracking()
                .AnyAsync(s => s.SwapRequestId == SwapRequestId &&
                               s.Status != SessionStatus.Cancelled);

            if (hasActiveSession)
            {
                TempData["StatusMessage"] = "This swap already has a session. You can manage it below.";
                return RedirectToPage("/Sessions/Index");
            }

            ListingTitle = swap.TargetListing.Title;
            return null;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await LoadAsync();
            return result ?? Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await LoadAsync();
            if (result != null) return result;

            // The browser sends the time as typed, with no time zone, so compare with the local clock.
            if (Input.ScheduledAt.HasValue && Input.ScheduledAt.Value <= DateTime.Now)
            {
                ModelState.AddModelError("Input.ScheduledAt", "Please choose a time in the future.");
            }

            if (!ModelState.IsValid) return Page();

            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SwapRequestId == SwapRequestId);

            if (session == null)
            {
                _context.Sessions.Add(new Session
                {
                    SwapRequestId = SwapRequestId,
                    ScheduledAt = Input.ScheduledAt,
                    Status = SessionStatus.Requested
                });
            }
            else
            {
                // LoadAsync only lets a cancelled session get here: reuse the row with the new time.
                session.ScheduledAt = Input.ScheduledAt;
                session.Status = SessionStatus.Requested;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Most likely the unique index: a session was created at the same moment (double click / second tab).
                TempData["StatusMessage"] = "We couldn't save that session. It may already exist, so please check below.";
                return RedirectToPage("/Sessions/Index");
            }

            TempData["StatusMessage"] = "Session proposed. Either of you can now confirm it.";
            return RedirectToPage("/Sessions/Index");
        }
    }
}