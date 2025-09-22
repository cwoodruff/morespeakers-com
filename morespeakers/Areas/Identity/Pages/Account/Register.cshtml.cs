// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using morespeakers.Models;
using morespeakers.Services;
using morespeakers.Data;

namespace MoreSpeakers.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IExpertiseService _expertiseService;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IExpertiseService expertiseService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _expertiseService = expertiseService;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public IEnumerable<Expertise> AvailableExpertise { get; set; } = new List<Expertise>();

        public IEnumerable<SpeakerType> SpeakerTypes { get; set; } = new List<SpeakerType>();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Additional User entity fields
            [Required]
            [StringLength(100)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(100)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(6000)]
            [Display(Name = "Bio")]
            [DataType(DataType.MultilineText)]
            public string Bio { get; set; }

            [Required]
            [StringLength(2000)]
            [Display(Name = "Goals")]
            [DataType(DataType.MultilineText)]
            public string Goals { get; set; }

            [Url]
            [StringLength(500)]
            [Display(Name = "Sessionize Profile URL")]
            public string SessionizeUrl { get; set; }

            [StringLength(500)]
            [Display(Name = "Headshot URL")]
            public string HeadshotUrl { get; set; }

            [Required]
            [Display(Name = "Speaker Type")]
            public int SpeakerTypeId { get; set; }

            [Required]
            [Display(Name = "Areas of Expertise")]
            public int[] SelectedExpertiseIds { get; set; } = Array.Empty<int>();

            [Display(Name = "Custom Expertise")]
            public string[] CustomExpertise { get; set; } = Array.Empty<string>();

            // Social Media Links
            [Display(Name = "Social Media Platforms")]
            public string[] SocialMediaPlatforms { get; set; } = Array.Empty<string>();

            [Display(Name = "Social Media URLs")]
            public string[] SocialMediaUrls { get; set; } = Array.Empty<string>();
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            await LoadFormDataAsync();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            return await OnPostSubmitAsync(returnUrl);
        }

        public async Task<IActionResult> OnPostValidateStepAsync(int step)
        {
            // Reload necessary data for the view
            await LoadFormDataAsync();

            // Only validate fields for the current step
            var stepValid = ValidateStep(step);

            if (!stepValid)
            {
                // Return the current step with validation errors
                return Partial("_RegisterStep" + step, this);
            }

            // Move to next step
            int nextStep = step + 1;
            if (nextStep <= 4)
            {
                return Partial("_RegisterStep" + nextStep, this);
            }

            // If we're at the last step, return success
            return new JsonResult(new { success = true, nextStep = nextStep });
        }

        public async Task<IActionResult> OnPostPreviousStepAsync(int step)
        {
            // Reload necessary data for the view
            await LoadFormDataAsync();

            // Return previous step without validation
            int prevStep = step - 1;
            if (prevStep >= 1)
            {
                return Partial("_RegisterStep" + prevStep, this);
            }

            return Partial("_RegisterStep1", this);
        }

        private async Task LoadFormDataAsync()
        {
            AvailableExpertise = await _expertiseService.GetAllExpertiseAsync();
            SpeakerTypes = new List<SpeakerType>
            {
                new SpeakerType { Id = 1, Name = "NewSpeaker", Description = "I'm new to speaking and looking for mentorship" },
                new SpeakerType { Id = 2, Name = "ExperiencedSpeaker", Description = "I'm an experienced speaker willing to mentor others" }
            };
        }

        private bool ValidateStep(int step)
        {
            // Clear all model state errors first
            ModelState.Clear();

            switch (step)
            {
                case 1: // Account step
                    if (string.IsNullOrWhiteSpace(Input.FirstName))
                        ModelState.AddModelError("Input.FirstName", "First name is required.");
                    if (string.IsNullOrWhiteSpace(Input.LastName))
                        ModelState.AddModelError("Input.LastName", "Last name is required.");
                    if (string.IsNullOrWhiteSpace(Input.Email))
                        ModelState.AddModelError("Input.Email", "Email is required.");
                    if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
                        ModelState.AddModelError("Input.PhoneNumber", "Phone number is required.");
                    if (string.IsNullOrWhiteSpace(Input.Password))
                        ModelState.AddModelError("Input.Password", "Password is required.");
                    if (Input.Password != Input.ConfirmPassword)
                        ModelState.AddModelError("Input.ConfirmPassword", "Passwords do not match.");
                    break;
                case 2: // Profile step
                    if (Input.SpeakerTypeId <= 0)
                        ModelState.AddModelError("Input.SpeakerTypeId", "Please select a speaker type.");
                    if (string.IsNullOrWhiteSpace(Input.Bio))
                        ModelState.AddModelError("Input.Bio", "Bio is required.");
                    if (string.IsNullOrWhiteSpace(Input.Goals))
                        ModelState.AddModelError("Input.Goals", "Goals are required.");
                    break;
                case 3: // Expertise step
                    if ((Input.SelectedExpertiseIds?.Length ?? 0) == 0 && (Input.CustomExpertise?.Length ?? 0) == 0)
                        ModelState.AddModelError("Input.SelectedExpertiseIds", "Please select at least one area of expertise.");
                    break;
                case 4: // Social step - optional, no validation needed
                    break;
            }

            return ModelState.IsValid;
        }

        public async Task<IActionResult> OnPostSubmitAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Reload data in case of validation errors
            await LoadFormDataAsync();

            // Validate all steps before final submission
            bool allStepsValid = true;
            for (int i = 1; i <= 4; i++)
            {
                if (!ValidateStep(i))
                {
                    allStepsValid = false;
                }
            }

            if (allStepsValid)
            {
                var user = CreateUser();

                // Set all the user properties
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.PhoneNumber = Input.PhoneNumber;
                user.Bio = Input.Bio;
                user.Goals = Input.Goals;
                user.SessionizeUrl = Input.SessionizeUrl;
                user.HeadshotUrl = Input.HeadshotUrl;
                user.SpeakerTypeId = Input.SpeakerTypeId;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add expertise relationships
                    foreach (var expertiseId in Input.SelectedExpertiseIds)
                    {
                        _context.UserExpertise.Add(new UserExpertise
                        {
                            UserId = user.Id,
                            ExpertiseId = expertiseId
                        });
                    }

                    // Add custom expertise
                    foreach (var customExpertise in Input.CustomExpertise.Where(ce => !string.IsNullOrWhiteSpace(ce)))
                    {
                        // Create new expertise
                        var expertise = new Expertise
                        {
                            Name = customExpertise.Trim(),
                            Description = $"Custom expertise: {customExpertise.Trim()}",
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.Expertise.Add(expertise);
                        await _context.SaveChangesAsync(); // Save to get the ID

                        _context.UserExpertise.Add(new UserExpertise
                        {
                            UserId = user.Id,
                            ExpertiseId = expertise.Id
                        });
                    }

                    // Add social media links
                    for (int i = 0; i < Input.SocialMediaPlatforms.Length && i < Input.SocialMediaUrls.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(Input.SocialMediaPlatforms[i]) &&
                            !string.IsNullOrWhiteSpace(Input.SocialMediaUrls[i]))
                        {
                            _context.SocialMedia.Add(new SocialMedia
                            {
                                UserId = user.Id,
                                Platform = Input.SocialMediaPlatforms[i],
                                Url = Input.SocialMediaUrls[i],
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }

                    await _context.SaveChangesAsync();

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
