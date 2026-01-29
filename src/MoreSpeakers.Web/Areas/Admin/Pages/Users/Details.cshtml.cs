using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Users;

[Authorize(Roles = Domain.Constants.UserRoles.Administrator)]
public partial class DetailsModel : PageModel
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

    public new required User User { get; set; }
    public IReadOnlyList<string> Roles { get; private set; } = [];
    public IReadOnlyList<string> AllRoles { get; private set; } = [];
    [BindProperty] public List<string> SelectedRoles { get; set; } = [];
    public DateTimeOffset? LastSignInUtc { get; private set; }

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
            AllRoles = await _userManager.GetAllRoleNamesAsync();
            SelectedRoles = [.. Roles];
        }
        catch (Exception ex)
        {
            LogFailedToRetrieveRolesForUser(ex, id);
            Roles = [];
            AllRoles = [];
        }

        User = user;
        // Not persisted today; conservative default
        LastSignInUtc = null;

        // Avoid caching admin details in intermediaries if an HttpContext is available
        Response.Headers.CacheControl = "no-store";

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
        return !Request.Headers.TryGetValue("HX-Request", out var hx) ||
               !string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase)
            ? RedirectToPage(new { id })
            : await GetDetailsResult(id);
    }

    public async Task<IActionResult> OnPostUpdateRolesAsync(Guid id)
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

        var currentRoles = await _userManager.GetRolesForUserAsync(id);
        var rolesToAdd = SelectedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
        var rolesToRemove = currentRoles.Except(SelectedRoles, StringComparer.OrdinalIgnoreCase).ToList();

        // Prevent removing the last administrator
        if (rolesToRemove.Contains(Domain.Constants.UserRoles.Administrator, StringComparer.OrdinalIgnoreCase))
        {
            var adminCount = await _userManager.GetUserCountInRoleAsync(Domain.Constants.UserRoles.Administrator);
            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "Cannot remove Administrator role: this is the last administrator account.";
                return await GetDetailsResult(id);
            }
        }

        if (rolesToAdd.Count != 0)
        {
            var result = await _userManager.AddToRolesAsync(id, rolesToAdd);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to add roles: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return await GetDetailsResult(id);
            }
        }

        if (rolesToRemove.Count != 0)
        {
            var result = await _userManager.RemoveFromRolesAsync(id, rolesToRemove);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to remove roles: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return await GetDetailsResult(id);
            }
        }

        TempData["StatusMessage"] = "Roles updated successfully.";
        return await GetDetailsResult(id);
    }

    private async Task<IActionResult> GetDetailsResult(Guid id)
    {
        var user = await _userManager.GetAsync(id);
        if (user == null) return NotFound();

        User = user;
        Roles = await _userManager.GetRolesForUserAsync(id);
        AllRoles = await _userManager.GetAllRoleNamesAsync();
        SelectedRoles = [.. Roles];
        LastSignInUtc = null;

        if (Request.Headers.TryGetValue("HX-Request", out var hx) && string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            // If the request came from a specific card, return only that card's partial.
            // Otherwise, we might need a default or return nothing/badrequest if unknown.
            // But usually we know where it came from based on the handler called.
            // To be safe and "most HTMX", we can use HX-Target header if available.
            
            if (Request.Headers.TryGetValue("HX-Target", out var target))
            {
                if (target == "user-security-card")
                {
                    return Partial("_UserSecurityCard", this);
                }
                if (target == "user-roles-card")
                {
                    return Partial("_UserRolesCard", this);
                }
            }
            
            // Fallback for handlers that didn't specify or if we want to be explicit in the handler
            return Page(); // This might not be what we want for HTMX if we expect a partial
        }

        return RedirectToPage(new { id });
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

        return await GetDetailsResult(id);
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

        return await GetDetailsResult(id);
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

        return await GetDetailsResult(id);
    }

    public async Task<IActionResult> OnPostSetTemporaryPasswordAsync(Guid id)
    {
        var user = await _userManager.GetAsync(id);
        if (user == null) return NotFound();

        // Generate a random temporary password
        var tempPassword = Guid.NewGuid().ToString("N")[..12];
        
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

        return await GetDetailsResult(id);
    }

    public async Task<IActionResult> OnPostSoftDeleteAsync(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        // Prevent self-delete
        var current = await _userManager.GetUserIdAsync(HttpContext.User);
        if (current?.Id == id)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToPage(new { id });
        }

        // Prevent deleting the last admin
        var roles = await _userManager.GetRolesForUserAsync(id);
        if (roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase))
        {
            var adminCount = await _userManager.GetUserCountInRoleAsync("Administrator");
            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "Cannot delete: this is the last administrator account.";
                return RedirectToPage(new { id });
            }
        }

        var ok = await _userManager.SoftDeleteAsync(id);
        TempData[ok ? "StatusMessage" : "ErrorMessage"] = ok ? "User has been soft-deleted." : "Failed to soft-delete user.";

        if (Request.Headers.TryGetValue("HX-Request", out var hx) && string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            User = (await _userManager.GetAsync(id))!;
            Roles = await _userManager.GetRolesForUserAsync(id);
            return Partial("_UserSecurityCard", this);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRestoreAsync(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        var ok = await _userManager.RestoreAsync(id);
        TempData[ok ? "StatusMessage" : "ErrorMessage"] = ok ? "User has been restored." : "Failed to restore user.";

        if (Request.Headers.TryGetValue("HX-Request", out var hx) && string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase))
        {
            User = (await _userManager.GetAsync(id))!;
            Roles = await _userManager.GetRolesForUserAsync(id);
            return Partial("_UserSecurityCard", this);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        // Prevent self-delete
        var current = await _userManager.GetUserIdAsync(HttpContext.User);
        if (current?.Id == id)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToPage(new { id });
        }

        // Prevent deleting the last admin
        var roles = await _userManager.GetRolesForUserAsync(id);
        if (roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase))
        {
            var adminCount = await _userManager.GetUserCountInRoleAsync("Administrator");
            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "Cannot delete: this is the last administrator account.";
                return RedirectToPage(new { id });
            }
        }

        var ok = await _userManager.DeleteAsync(id);
        if (ok)
        {
            TempData["StatusMessage"] = "User has been permanently deleted.";
            return RedirectToPage("Index");
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to permanently delete user.";
            return RedirectToPage(new { id });
        }
    }
}
