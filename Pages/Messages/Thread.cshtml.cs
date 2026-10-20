using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Pages.Messages
{
    [Authorize]
    public class ThreadModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ThreadModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; } = string.Empty;

        public string OtherDisplayName { get; set; } = "Forsa user";
        public List<Message> Messages { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [StringLength(2000)]
            public string Content { get; set; } = string.Empty;
        }

        // False when there is no accepted swap between the two users (history stays readable, sending is blocked).
        public bool CanSend { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var guard = await GuardAsync();
            if (guard != null) return guard;

            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var guard = await GuardAsync();
            if (guard != null) return guard;

            var myId = _userManager.GetUserId(User)!;

            // C2: messaging is only possible between users with an accepted swap.
            if (!CanSend)
            {
                ModelState.AddModelError(string.Empty, "You can message each other once a swap request between you has been accepted.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            var content = Input.Content.Trim();

            // A3: the same text sent twice within a few seconds is a double-click / retry, not a second message.
            var since = DateTime.UtcNow.AddSeconds(-10);
            var duplicate = await _context.Messages.AnyAsync(m =>
                m.SenderId == myId &&
                m.ReceiverId == UserId &&
                m.Content == content &&
                m.SentAt > since);

            if (!duplicate)
            {
                _context.Messages.Add(new Message
                {
                    SenderId = myId,
                    ReceiverId = UserId,
                    Content = content
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { userId = UserId });
        }

        // Validates the conversation partner and sets CanSend. Returns a result when the request must stop.
        private async Task<IActionResult?> GuardAsync()
        {
            var myId = _userManager.GetUserId(User);
            if (myId == null) return Challenge();

            if (string.IsNullOrWhiteSpace(UserId)) return NotFound();

            // C4: no conversation with yourself.
            if (UserId == myId) return RedirectToPage("/Messages/Inbox");

            // C3: the other user has to exist.
            if (await _userManager.FindByIdAsync(UserId) == null) return NotFound();

            CanSend = await _context.SwapRequests.AnyAsync(r =>
                r.Status == SwapRequestStatus.Accepted &&
                r.TargetListing != null &&
                ((r.RequesterUserId == myId && r.TargetListing.OwnerUserId == UserId) ||
                 (r.RequesterUserId == UserId && r.TargetListing.OwnerUserId == myId)));

            if (!CanSend)
            {
                var hasHistory = await _context.Messages.AnyAsync(m =>
                    (m.SenderId == myId && m.ReceiverId == UserId) ||
                    (m.SenderId == UserId && m.ReceiverId == myId));

                if (!hasHistory) return Forbid();
            }

            return null;
        }

        private async Task LoadAsync()
        {
            var myId = _userManager.GetUserId(User);

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == UserId);
            if (profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                OtherDisplayName = profile.DisplayName;
            }

            // C5: Id as tie-breaker keeps messages with the same timestamp in a stable order.
            Messages = await _context.Messages
                .Where(m => (m.SenderId == myId && m.ReceiverId == UserId) || (m.SenderId == UserId && m.ReceiverId == myId))
                .OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }
    }
}
