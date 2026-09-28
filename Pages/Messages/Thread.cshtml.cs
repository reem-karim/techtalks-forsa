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

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var myId = _userManager.GetUserId(User);
            if (myId == null) return Challenge();

            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            _context.Messages.Add(new Message
            {
                SenderId = myId,
                ReceiverId = UserId,
                Content = Input.Content
            });
            await _context.SaveChangesAsync();

            return RedirectToPage(new { userId = UserId });
        }

        private async Task LoadAsync()
        {
            var myId = _userManager.GetUserId(User);

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == UserId);
            if (profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                OtherDisplayName = profile.DisplayName;
            }

            Messages = await _context.Messages
                .Where(m => (m.SenderId == myId && m.ReceiverId == UserId) || (m.SenderId == UserId && m.ReceiverId == myId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}