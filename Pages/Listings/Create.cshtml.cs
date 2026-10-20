using techtalks_forsa.Data;
using techtalks_forsa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace techtalks_forsa.Pages.Listings
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
            [Required]
            [StringLength(100)]
            public string Title { get; set; } = string.Empty;

            [StringLength(1000)]
            public string? Description { get; set; }

            [Required]
            [Display(Name = "Category")]
            public string Category { get; set; } = string.Empty;

            // Nullable so that [Required] really fires when the field is missing (a non-nullable enum silently becomes Offer).
            [Required]
            [Display(Name = "Listing Type")]
            public ListingType? Type { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // B6: category must be one of the known categories, type must be a real enum value.
            if (!string.IsNullOrEmpty(Input.Category) && !SkillCategories.All.Contains(Input.Category))
            {
                ModelState.AddModelError("Input.Category", "Please choose a category from the list.");
            }
            if (Input.Type != null && !Enum.IsDefined(Input.Type.Value))
            {
                ModelState.AddModelError("Input.Type", "Please choose a valid listing type.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var title = Input.Title.Trim();
            var type = Input.Type!.Value;

            // A4: no identical listing twice. A listing created a moment ago is a double-click: just go on.
            var duplicate = await _context.Listings
                .Where(l => l.OwnerUserId == userId &&
                            l.Title == title &&
                            l.Category == Input.Category &&
                            l.Type == type)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();

            if (duplicate != null)
            {
                if (duplicate.CreatedAt > DateTime.UtcNow.AddSeconds(-30))
                {
                    return RedirectToPage("/Listings/Index");
                }

                ModelState.AddModelError(string.Empty, "You already have a listing with the same title, category and type.");
                return Page();
            }

            var listing = new Listing
            {
                OwnerUserId = userId,
                Title = title,
                Description = Input.Description,
                Category = Input.Category,
                Type = type
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Listings/Index");
        }
    }
}