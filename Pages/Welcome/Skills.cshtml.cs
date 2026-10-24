using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using techtalks_forsa.Data;
using techtalks_forsa.Models;

namespace techtalks_forsa.Pages.Welcome
{
    // Step 2 of the flow: add one skill you can teach and one you want to learn, on a single short form.
    [Authorize]
    public class SkillsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SkillsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> CategoryOptions => SkillCategories.All
            .Select(c => new SelectListItem(c, c)).ToList();

        public class InputModel
        {
            [StringLength(100)]
            [Display(Name = "Skill")]
            public string? TeachTitle { get; set; }

            [Display(Name = "Category")]
            public string? TeachCategory { get; set; }

            [StringLength(100)]
            [Display(Name = "Skill")]
            public string? LearnTitle { get; set; }

            [Display(Name = "Category")]
            public string? LearnCategory { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var teachTitle = Input.TeachTitle?.Trim();
            var learnTitle = Input.LearnTitle?.Trim();

            ValidatePair(teachTitle, Input.TeachCategory, "Input.TeachCategory");
            ValidatePair(learnTitle, Input.LearnCategory, "Input.LearnCategory");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var addedTeach = false;
            var addedLearn = false;

            if (!string.IsNullOrEmpty(teachTitle))
            {
                addedTeach = await AddIfMissingAsync(userId, teachTitle, Input.TeachCategory!, ListingType.Offer);
            }

            if (!string.IsNullOrEmpty(learnTitle))
            {
                addedLearn = await AddIfMissingAsync(userId, learnTitle, Input.LearnCategory!, ListingType.Learn);
            }

            await _context.SaveChangesAsync();

            if (addedLearn)
            {
                TempData["StatusMessage"] = "Skills saved. Here are people who can teach what you want to learn.";
                return RedirectToPage("/Matches/Index");
            }

            if (addedTeach)
            {
                TempData["StatusMessage"] = "Skill saved. Now browse what others offer.";
            }

            return RedirectToPage("/Browse/Index");
        }

        private void ValidatePair(string? title, string? category, string categoryKey)
        {
            if (string.IsNullOrEmpty(title)) return;

            if (string.IsNullOrEmpty(category) || !SkillCategories.All.Contains(category))
            {
                ModelState.AddModelError(categoryKey, "Please choose a category.");
            }
        }

        // Returns true when a new listing was added (an identical one is silently reused: double-click safe).
        private async Task<bool> AddIfMissingAsync(string userId, string title, string category, ListingType type)
        {
            var exists = await _context.Listings.AnyAsync(l =>
                l.OwnerUserId == userId &&
                l.Title == title &&
                l.Category == category &&
                l.Type == type);

            if (exists) return false;

            _context.Listings.Add(new Listing
            {
                OwnerUserId = userId,
                Title = title,
                Category = category,
                Type = type
            });

            return true;
        }
    }
}