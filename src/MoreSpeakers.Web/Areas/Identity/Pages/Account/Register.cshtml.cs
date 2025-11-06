using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MoreSpeakers.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public partial class RegisterModel : PageModel
{
    private readonly SignInManager<Data.Models.User> _signInManager;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IUserManager _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IRazorPartialToStringRenderer _stringRenderer;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<RegisterModel> _logger;
    
    public RegisterModel(
        SignInManager<Data.Models.User> signInManager,
        IExpertiseManager expertiseManager,
        IUserManager userManager,
        IEmailSender emailSender,
        IRazorPartialToStringRenderer stringRenderer,
        TelemetryClient telemetryClient,
        ILogger<RegisterModel> logger)
    {
        _signInManager = signInManager;
        _expertiseManager = expertiseManager;
        _userManager = userManager;
        _emailSender = emailSender;
        _stringRenderer = stringRenderer;
        _telemetryClient = telemetryClient;
        _logger = logger;
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
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public IEnumerable<Expertise> AvailableExpertise { get; set; } = new List<Expertise>();

    public IEnumerable<SpeakerType> SpeakerTypes { get; set; } = new List<SpeakerType>();

    // Properties needed for complete registration (step 5) confirmation
    public IEnumerable<Expertise> AllExpertise { get; set; } = new List<Expertise>();

    // Property to expose selected expertise IDs for registration completion (step 5)
    public int[] ExpertiseIds => Input.SelectedExpertiseIds ?? [];

    // Properties required by _RegistrationContainer.cshtml
    public int CurrentStep { get; set; } = Models.RegistrationProgressions.SpeakerProfileNeeded;
    public bool HasValidationErrors { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;


    public async Task OnGetAsync()
    {
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        // Initialize registration state
        CurrentStep = Models.RegistrationProgressions.SpeakerProfileNeeded;
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
        SuccessMessage = string.Empty;

        await LoadFormDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        return await OnPostSubmitAsync();
    }

    public async Task<IActionResult> OnPostValidateStepAsync(int step)
    {
        // Validate step number
        if (!Models.RegistrationProgressions.IsValid(step))
        {
            ModelState.AddModelError("", "Invalid step number.");
            return BadRequest();
        }

        // Reload necessary data for the view
        await LoadFormDataAsync();

        // Validate current step
        var stepValid = ValidateStep(step);

        // Additional async validations for step 1 (e.g., email uniqueness)
        if (step == Models.RegistrationProgressions.SpeakerProfileNeeded && stepValid)
        {
            if (!string.IsNullOrWhiteSpace(Input.Email))
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Input.Email", "This email address is already in use.");
                    ModelState.AddModelError("", "This email address is already in use.");
                    HasValidationErrors = true;
                    ValidationMessage = GetValidationMessage(step);
                    CurrentStep = step;
                    SuccessMessage = string.Empty;
                    return Partial("_RegistrationContainer", this);
                }
            }
        }

        if (!stepValid)
        {
            // Add visual indicators for better UX
            HasValidationErrors = true;
            ValidationMessage = GetValidationMessage(step);
            CurrentStep = step;
            SuccessMessage = string.Empty;

            // Return the complete registration container with current step and validation errors
            return Partial("_RegistrationContainer", this);
        }

        // All validation passed - move to next step
        var nextStep = step + 1;
        if (nextStep <= Models.RegistrationProgressions.SocialMediaNeeded)
        {
            HasValidationErrors = false;
            SuccessMessage = GetSuccessMessage(step);
            CurrentStep = nextStep;
            ValidationMessage = string.Empty;

            // Return complete registration container with next step
            return Partial("_RegistrationContainer", this);
        }

        // If we're at the last step, indicate success
        HasValidationErrors = false;
        SuccessMessage = "All steps completed successfully!";
        CurrentStep = nextStep;
        ValidationMessage = string.Empty;
        return Partial("_RegistrationContainer", this);
    }

    private string GetValidationMessage(int step)
    {
        return step switch
        {
            Models.RegistrationProgressions.SpeakerProfileNeeded => "Please complete all required account information before proceeding.",
            Models.RegistrationProgressions.RequiredInformationNeeded => "Please fill out your speaker profile completely.",
            Models.RegistrationProgressions.ExpertiseNeeded => "Please select at least one area of expertise.",
            Models.RegistrationProgressions.SocialMediaNeeded => "Please review your social media information.",
            _ => "Please complete the required information."
        };
    }

    private string GetSuccessMessage(int step)
    {
        return step switch
        {
            Models.RegistrationProgressions.SpeakerProfileNeeded => "Account information saved successfully!",
            Models.RegistrationProgressions.RequiredInformationNeeded => "Profile information saved successfully!",
            Models.RegistrationProgressions.ExpertiseNeeded => "Expertise areas saved successfully!",
            _ => "Information saved successfully!"
        };
    }

    public async Task<IActionResult> OnPostPreviousStepAsync(int step)
    {
        // Reload necessary data for the view
        await LoadFormDataAsync();

        // Return previous step without validation
        var prevStep = step - 1;
        if (prevStep < Models.RegistrationProgressions.SpeakerProfileNeeded)
            prevStep = Models.RegistrationProgressions.SpeakerProfileNeeded;

        CurrentStep = prevStep;
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
        SuccessMessage = string.Empty;

        return Partial("_RegistrationContainer", this);
    }

    public async Task<IActionResult> OnPostValidateEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new JsonResult(new { isValid = true, message = "" });
        }

        // Check if email is already in use
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return new JsonResult(new { isValid = false, message = "This email address is already in use." });
        }

        // Check if email format is valid
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return new JsonResult(new { isValid = false, message = "Please enter a valid email address." });
        }

        return new JsonResult(new { isValid = true, message = "" });
    }

    public async Task<IActionResult> OnPostValidateCustomExpertiseAsync(string expertiseName)
    {
        if (string.IsNullOrWhiteSpace(expertiseName))
        {
            return new JsonResult(new { isValid = true, message = "", suggestion = "" });
        }

        var trimmedName = expertiseName.Trim();

        // Check if expertise already exists (case-insensitive)
        var existingExpertise = await _expertiseManager.SearchForExpertiseExistsAsync(trimmedName);

        if (existingExpertise != null)
        {
            return new JsonResult(new
            {
                isValid = false,
                message = $"'{existingExpertise.Name}' already exists in our database.",
                suggestion = $"Consider selecting '{existingExpertise.Name}' from the list above instead.",
                existingId = existingExpertise.Id
            });
        }

        // Check for similar expertise (fuzzy matching for suggestions)
        var similarExpertise = await _expertiseManager.FuzzySearchForExistingExpertise(trimmedName);

        if (similarExpertise.Any())
        {
            var suggestions = string.Join(", ", similarExpertise.Select(s => s.Name));
            return new JsonResult(new
            {
                isValid = true,
                message = "",
                suggestion = $"Similar expertise found: {suggestions}. Consider selecting from existing options.",
                similarItems = similarExpertise
            });
        }

        return new JsonResult(new { isValid = true, message = "", suggestion = "" });
    }

    private async Task LoadFormDataAsync()
    {
        AvailableExpertise = await _expertiseManager.GetAllAsync();
        AllExpertise = AvailableExpertise; // Same data, different property name for registration completion (step 5)
        SpeakerTypes = await _userManager.GetSpeakerTypesAsync();
    }

    private bool ValidateStep(int step)
    {
        // Clear all model state errors first
        ModelState.Clear();

        switch (step)
        {
            case Models.RegistrationProgressions.SpeakerProfileNeeded: // Account step
                if (string.IsNullOrWhiteSpace(Input.FirstName))
                {
                    ModelState.AddModelError("Input.FirstName", "First name is required.");
                }
                if (string.IsNullOrWhiteSpace(Input.LastName))
                {
                    ModelState.AddModelError("Input.LastName", "Last name is required.");
                }
                // Email required and format validation
                if (string.IsNullOrWhiteSpace(Input.Email))
                {
                    ModelState.AddModelError("Input.Email", "Email is required.");
                    ModelState.AddModelError("", "Email is required.");
                }
                else
                {
                    var emailAttr = new EmailAddressAttribute();
                    if (!emailAttr.IsValid(Input.Email))
                    {
                        ModelState.AddModelError("Input.Email", "Please enter a valid email address.");
                        ModelState.AddModelError("", "Please enter a valid email address.");
                    }
                }
                // Phone required and basic format validation (numbers, spaces, dashes, parentheses, leading +)
                if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
                {
                    ModelState.AddModelError("Input.PhoneNumber", "Phone number is required.");
                    ModelState.AddModelError("", "Phone number is required.");
                }
                else
                {
                    // Allow formats like +1 (555) 123-4567 or 555-123-4567 etc.
                    var phone = Input.PhoneNumber!.Trim();
                    // Reject if contains letters
                    if (phone.Any(char.IsLetter))
                    {
                        ModelState.AddModelError("Input.PhoneNumber", "Please enter a valid phone number.");
                        ModelState.AddModelError("", "Please enter a valid phone number.");
                    }
                    else
                    {
                        // Ensure at least 10 digits present
                        var digitCount = phone.Count(char.IsDigit);
                        if (digitCount < 10)
                        {
                            ModelState.AddModelError("Input.PhoneNumber", "Phone number must contain at least 10 digits.");
                            ModelState.AddModelError("", "Phone number must contain at least 10 digits.");
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(Input.Password))
                {
                    ModelState.AddModelError("Input.Password", "Password is required.");
                }
                if (Input.Password != Input.ConfirmPassword)
                {
                    ModelState.AddModelError("Input.ConfirmPassword", "Passwords do not match.");
                }
                break;
            case Models.RegistrationProgressions.RequiredInformationNeeded: // Profile step
                if (Input.SpeakerTypeId <= 0)
                    ModelState.AddModelError("Input.SpeakerTypeId", "Please select a speaker type.");
                if (string.IsNullOrWhiteSpace(Input.Bio))
                    ModelState.AddModelError("Input.Bio", "Bio is required.");
                if (string.IsNullOrWhiteSpace(Input.Goals))
                    ModelState.AddModelError("Input.Goals", "Goals are required.");
                break;
            case Models.RegistrationProgressions.ExpertiseNeeded: // Expertise step
                if ((Input.SelectedExpertiseIds?.Length ?? 0) == 0 && (Input.CustomExpertise?.Length ?? 0) == 0)
                    ModelState.AddModelError("Input.SelectedExpertiseIds",
                        "Please select at least one area of expertise.");
                break;
            case Models.RegistrationProgressions.SocialMediaNeeded: // Social step - optional, no validation needed
                break;
        }

        return ModelState.IsValid;
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        // Reload data in case of validation errors
        await LoadFormDataAsync();

        // Validate all steps before final submission
        var allStepsValid = true;
        foreach(var i in Models.RegistrationProgressions.All)
            if (!ValidateStep(i))
                allStepsValid = false;

        if (allStepsValid)
        {
            var user = new User
            {
                Email = Input.Email, 
                UserName = Input.Email, 
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                PhoneNumber = Input.PhoneNumber,
                Bio = Input.Bio,
                Goals = Input.Goals,
                SessionizeUrl = Input.SessionizeUrl,
                HeadshotUrl = Input.HeadshotUrl,
                SpeakerTypeId = Input.SpeakerTypeId,    
            };

            IdentityResult saveResult;
            try
            {
                saveResult = await _userManager.CreateAsync(user, Input.Password);
            }
            catch (Exception ex)
            {
                await LoadFormDataAsync();
                CurrentStep = Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to first step on major failure
                HasValidationErrors = true;
                ValidationMessage = "The saving of registration failed. Please check the errors and try again.";
                return Page();
            }

            if (!saveResult.Succeeded)
            {
                foreach (var error in saveResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                CurrentStep = Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to first step on major failure
                HasValidationErrors = true;
                ValidationMessage = "The saving of registration failed. Please check the errors and try again.";
                return Page();
            }

            _logger.LogInformation("User created a new account with password");

            // Load the user from the identity store
            user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                await LoadFormDataAsync();
                CurrentStep = Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to first step on major failure
                HasValidationErrors = true;
                ValidationMessage = "Could not find user after saving registration. Please try again.";
                return Page();
            }
            
            // Add expertise relationships
            await _userManager.EmptyAndAddExpertiseForUserAsync(user.Id, Input.SelectedExpertiseIds);
            
            // Add custom expertise
            foreach (var customExpertise in Input.CustomExpertise.Where(ce => !string.IsNullOrWhiteSpace(ce)))
            {
                var expertiseId = await _expertiseManager.CreateExpertiseAsync(customExpertise.Trim(),
                    $"Custom expertise: {customExpertise.Trim()}");
                await _userManager.AddExpertiseToUserAsync(user.Id, expertiseId);
            }

            // Add social media links
            var socialMediaLinks = new List<SocialMedia>();
            for (var i = 0; i < Input.SocialMediaPlatforms.Length && i < Input.SocialMediaUrls.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(Input.SocialMediaPlatforms[i]) &&
                    !string.IsNullOrWhiteSpace(Input.SocialMediaUrls[i]))
                    socialMediaLinks.Add(new SocialMedia
                    {
                        UserId = user.Id,
                        Platform = Input.SocialMediaPlatforms[i],
                        Url = Input.SocialMediaUrls[i],
                        CreatedDate = DateTime.UtcNow
                    });
            }

            await _userManager.EmptyAndAddSocialMediaForUserAsync(user.Id, socialMediaLinks);

            // Send welcome email (mock implementation)
            await SendWelcomeEmailAsync(user);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new { area = "Identity", user.Id, code, S = "~//" },
                Request.Scheme)!;

            await _emailSender.QueueEmail(new System.Net.Mail.MailAddress(user.Email!, $"{user.FirstName} {user.LastName}"),
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            _telemetryClient.TrackEvent("ConfirmationEmailSent", new Dictionary<string, string>
            {
                { "UserId", user.Id.ToString() },
                { "Email", user.Email }
            });

            // Load data needed for registration completion (step 5) display
            await LoadFormDataAsync();

            CurrentStep = Models.RegistrationProgressions.Complete;
            HasValidationErrors = false;
            ValidationMessage = string.Empty;
            SuccessMessage = "Registration completed successfully!";

            // Return step 5 (confirmation) instead of redirecting away
            return Partial("_RegistrationContainer", this);
            
        }

        // If we got this far, something failed, redisplay form with current state
        await LoadFormDataAsync();
        CurrentStep = Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to first step on major failure
        HasValidationErrors = true;
        ValidationMessage = "Registration failed. Please check the errors and try again.";
        return Page();
    }

    /// <summary>
    /// Sends a welcome email to the newly registered user
    /// </summary>
    private async Task SendWelcomeEmailAsync(User user)
    {
        const string emailSubject = "Welcome to MoreSpeakers.com - Your Speaking Journey Begins!";

        try
        {
            var emailBody = await _stringRenderer.RenderPartialToStringAsync(HttpContext, "~/EmailTemplates/WelcomeEmail.cshtml", user);
            await _emailSender.QueueEmail(new System.Net.Mail.MailAddress(user.Email!, $"{user.FirstName} {user.LastName}"),
                emailSubject, emailBody);

            _telemetryClient.TrackEvent(Domain.Constants.TelemetryEvents.WelcomeEmail, new Dictionary<string, string>
            {
                { "UserId", user.Id.ToString() },
                { "Email", user.Email! }
            });
            _logger.LogInformation("Welcome email successfully sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            // Don't throw - registration should still succeed even if email fails
        }
    }
}