using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResetPasswordModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(IUserManager userManager, ILogger<ResetPasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel? Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; init; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? Password { get; init; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; init; }

        [Required]
        public required string Token { get; init; }
    }

    public IActionResult OnGet(string? token = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest("A token must be supplied for password reset.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("An email address must be supplied for password reset.");
        }

        Input = new InputModel
        {
            Token = token,
            Email = email
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input!.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.Password!);
        if (result.Succeeded)
        {
            // Clear MustChangePassword if it was set
            if (user.MustChangePassword)
            {
                user.MustChangePassword = false;
                await _userManager.SaveAsync(user);
            }
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return Page();
    }
}
