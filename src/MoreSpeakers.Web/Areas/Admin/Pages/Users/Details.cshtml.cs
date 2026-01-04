using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Users;

[Authorize(Roles = Domain.Constants.UserRoles.Administrator)]
public class DetailsModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly ITemplatedEmailSender _emailSender;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        IUserManager userManager,
        ITemplatedEmailSender emailSender,
        ILogger<DetailsModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
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

    // ------------------------------------------
    // POST handlers: Enable/Disable Lockout, Set Lockout End, Unlock
    // ------------------------------------------

    public async Task<IActionResult> OnPostEnableLockoutAsync(Guid id, bool enabled)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        // Prevent self-lock
        var current = await _userManager.GetUserIdAsync(HttpContext.User);
        if (current?.Id == id && enabled)
        {
            TempData["ErrorMessage"] = "You cannot enable lockout on your own account.";
            return RedirectToPage(new { id });
        }

        // Prevent locking the last admin
        var roles = await _userManager.GetRolesForUserAsync(id);
        if (enabled && roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase))
        {
            var adminCount = await _userManager.GetUserCountInRoleAsync("Administrator");
            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "Cannot enable lockout: this is the last administrator account.";
                return RedirectToPage(new { id });
            }
        }

        var ok = await _userManager.EnableLockoutAsync(id, enabled);
        TempData[ok ? "StatusMessage" : "ErrorMessage"] = ok
            ? (enabled ? "Lockout has been enabled." : "Lockout has been disabled.")
            : "Failed to update lockout setting.";

        // HTMX partial refresh path
        if (!Request.Headers.TryGetValue("HX-Request", out var hx) ||
            !string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage(new { id });
        }

        // Re-hydrate model state for partial
        var user = await _userManager.GetAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        User = user;
        try
        {
            Roles = await _userManager.GetRolesForUserAsync(id);
        }
        catch
        {
            Roles = Array.Empty<string>();
        }
        LastSignInUtc = null;
        return Partial("_UserSecurityCard", this);

    }

    public async Task<IActionResult> OnPostSetLockoutEndAsync(Guid id, DateTimeOffset? lockoutEndUtc)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        // Interpret provided value as UTC; validation happens in data layer, but we add a quick client guard
        if (lockoutEndUtc.HasValue && lockoutEndUtc.Value <= DateTimeOffset.UtcNow)
        {
            TempData["ErrorMessage"] = "Lockout end must be in the future (UTC).";
            return RedirectToPage(new { id });
        }

        // Prevent self-lock
        var current = await _userManager.GetUserIdAsync(HttpContext.User);
        if (current?.Id == id && lockoutEndUtc.HasValue)
        {
            TempData["ErrorMessage"] = "You cannot set a lockout end on your own account.";
            return RedirectToPage(new { id });
        }

        // Prevent locking the last admin
        var roles = await _userManager.GetRolesForUserAsync(id);
        if (lockoutEndUtc.HasValue && roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase))
        {
            var adminCount = await _userManager.GetUserCountInRoleAsync("Administrator");
            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "Cannot set lockout end: this is the last administrator account.";
                return RedirectToPage(new { id });
            }
        }

        var ok = await _userManager.SetLockoutEndAsync(id, lockoutEndUtc?.ToUniversalTime());
        TempData[ok ? "StatusMessage" : "ErrorMessage"] = ok
            ? (lockoutEndUtc.HasValue ? "Lockout end updated." : "Lockout removed.")
            : "Failed to update lockout end.";

        // HTMX partial refresh path
        if (!Request.Headers.TryGetValue("HX-Request", out var hx) ||
            !string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage(new { id });
        }

        var user = await _userManager.GetAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        User = user;
        try
        {
            Roles = await _userManager.GetRolesForUserAsync(id);
        }
        catch
        {
            Roles = Array.Empty<string>();
        }
        LastSignInUtc = null;
        return Partial("_UserSecurityCard", this);

    }

    public async Task<IActionResult> OnPostUnlockAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        // Self-unlock is allowed; no restriction here
        var ok = await _userManager.UnlockAsync(id);
        TempData[ok ? "StatusMessage" : "ErrorMessage"] = ok
            ? "User has been unlocked."
            : "Failed to unlock the user.";

        // HTMX partial refresh path
        if (Request.Headers.TryGetValue("HX-Request", out var hx) && string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            var user = await _userManager.GetAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            User = user;
            try
            {
                Roles = await _userManager.GetRolesForUserAsync(id);
            }
            catch
            {
                Roles = Array.Empty<string>();
            }
            LastSignInUtc = null;
            return Partial("_UserSecurityCard", this);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostTriggerPasswordResetAsync(Guid id)
    {
        var user = await _userManager.GetAsync(id);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Generate the callback URL with both token and email for password reset
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", token, email = user.Email },
            protocol: Request.Scheme);
        
        var ok = await _emailSender.SendTemplatedEmail("~/EmailTemplates/PasswordReset.cshtml",
            "AdminTriggeredPasswordReset",
            "Reset your password",
            user,
            new UserPasswordResetEmail { User = user, ResetEmailUrl = callbackUrl });

        TempData[ok ? "StatusMessage" : "ErrorMessage"] = ok
            ? "Password reset email has been sent."
            : "Failed to send password reset email.";

        if (Request.Headers.TryGetValue("HX-Request", out var hx) && string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            User = user;
            Roles = await _userManager.GetRolesForUserAsync(id);
            return Partial("_UserSecurityCard", this);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSetTemporaryPasswordAsync(Guid id)
    {
        var user = await _userManager.GetAsync(id);
        if (user == null) return NotFound();

        // Generate a random temporary password
        var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 12);
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);

        if (result.Succeeded)
        {
            user.MustChangePassword = true;
            await _userManager.SaveAsync(user);
            
            TempData["StatusMessage"] = $"Temporary password set to: {tempPassword}. User MUST change it on next login.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to set temporary password: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        if (Request.Headers.TryGetValue("HX-Request", out var hx) && string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            User = user;
            Roles = await _userManager.GetRolesForUserAsync(id);
            return Partial("_UserSecurityCard", this);
        }

        return RedirectToPage(new { id });
    }
}
