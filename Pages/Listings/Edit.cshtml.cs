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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

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
            public string Category { get; set; } = string.Empty;

            [Required]
            public ListingType Type { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == Id && l.OwnerUserId == userId);

            if (listing == null)
            {
                return NotFound();
            }

            Input.Title = listing.Title;
            Input.Description = listing.Description;
            Input.Category = listing.Category;
            Input.Type = listing.Type;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == Id && l.OwnerUserId == userId);

            if (listing == null)
            {
                return NotFound();
            }

            listing.Title = Input.Title;
            listing.Description = Input.Description;
            listing.Category = Input.Category;
            listing.Type = Input.Type;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Listings/Index");
        }
    }
}