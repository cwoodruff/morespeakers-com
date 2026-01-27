using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly ITemplatedEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(IUserManager userManager, ITemplatedEmailSender emailSender, ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel? Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; init; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input!.Email);
            if (user is not { EmailConfirmed: true }) // Minimal check for existing user, simplified for this flow
            {
                // Find user and check if email is confirmed. For security, don't reveal that the user does not exist or is not confirmed
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // Re-fetch user to be sure we have the latest state if needed
            user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null) return RedirectToPage("./ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", token, email = user.Email },
                protocol: Request.Scheme);

            var ok = await _emailSender.SendTemplatedEmail(
                "~/EmailTemplates/PasswordReset.cshtml",
                "UserForgotPassword",
                "Reset your password",
                user,
                new UserPasswordResetEmail { User = user, ResetEmailUrl = callbackUrl });

            if (!ok)
            {
                _logger.LogError("Failed to send password reset email to {Email}", Input.Email);
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }
}
