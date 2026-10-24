using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using techtalks_forsa.Data;

namespace techtalks_forsa.Infrastructure;

/// <summary>
/// Keeps the user flow logical: a signed-in user who has not finished their profile
/// (no display name yet) is sent to "Profile/Edit" before anything else.
/// Login, register, logout, privacy and the setup pages themselves are never redirected.
/// </summary>
public class ProfileSetupFilter : IAsyncPageFilter
{
    private static readonly HashSet<string> ExemptPages = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Profile/Edit",
        "/Welcome/Skills",
        "/Privacy",
        "/Error"
    };

    private readonly ApplicationDbContext _db;

    public ProfileSetupFilter(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var area = context.ActionDescriptor.AreaName;
            var page = context.ActionDescriptor.ViewEnginePath ?? string.Empty;

            var isIdentityArea = string.Equals(area, "Identity", StringComparison.OrdinalIgnoreCase);

            if (!isIdentityArea && !ExemptPages.Contains(page))
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId != null)
                {
                    var hasProfile = await _db.Profiles.AsNoTracking()
                        .AnyAsync(p => p.UserId == userId &&
                                       p.DisplayName != null &&
                                       p.DisplayName != "");

                    if (!hasProfile)
                    {
                        context.Result = new RedirectToPageResult("/Profile/Edit");
                        return;
                    }
                }
            }
        }

        await next();
    }
}