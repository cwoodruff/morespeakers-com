using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Users;

[Authorize(Roles = "Administrator")]
public class DetailsModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        IUserManager userManager,
        ILogger<DetailsModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public required User User { get; set; }
    public IReadOnlyList<string> Roles { get; private set; } = Array.Empty<string>();
    public DateTimeOffset? LastSignInUtc { get; private set; } = null;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        var user = await _userManager.GetAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Retrieve roles via Manager abstraction
        try
        {
            Roles = await _userManager.GetRolesForUserAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve roles for user {UserId}", id);
            Roles = Array.Empty<string>();
        }

        User = user;
        // Not persisted today; conservative default
        LastSignInUtc = null;

        // Avoid caching admin details in intermediaries if an HttpContext is available
        Response.Headers["Cache-Control"] = "no-store";

        return Page();
    }
}
