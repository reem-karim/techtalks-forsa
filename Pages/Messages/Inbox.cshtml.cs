using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace techtalks_forsa.Pages.Messages
{
    [Authorize]
    public class InboxModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InboxModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class Conversation
        {
            public string OtherUserId { get; set; } = string.Empty;
            public string OtherDisplayName { get; set; } = "Forsa user";
            public string LastMessage { get; set; } = string.Empty;
            public DateTime LastSentAt { get; set; }
        }

        public List<Conversation> Conversations { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var myMessages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var grouped = myMessages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.First())
                .ToList();

            var otherIds = grouped.Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId).Distinct().ToList();

            var profiles = await _context.Profiles.Where(p => otherIds.Contains(p.UserId)).ToListAsync();

            Conversations = grouped.Select(m =>
            {
                var otherId = m.SenderId == userId ? m.ReceiverId : m.SenderId;
                var profile = profiles.FirstOrDefault(p => p.UserId == otherId);

                return new Conversation
                {
                    OtherUserId = otherId,
                    OtherDisplayName = !string.IsNullOrWhiteSpace(profile?.DisplayName) ? profile!.DisplayName! : "Forsa user",
                    LastMessage = m.Content,
                    LastSentAt = m.SentAt
                };
            })
            .OrderByDescending(c => c.LastSentAt)
            .ToList();
        }
    }
}