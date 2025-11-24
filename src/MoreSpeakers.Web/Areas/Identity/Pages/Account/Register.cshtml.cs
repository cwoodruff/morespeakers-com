using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MoreSpeakers.Domain.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public partial class RegisterModel : PageModel
{
    private readonly SignInManager<Data.Models.User> _signInManager;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IUserManager _userManager;
    private readonly ITemplatedEmailSender _templatedEmailSender;
    private readonly ISocialMediaSiteManager _socialMediaSiteManager;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<RegisterModel> _logger;
    
    public RegisterModel(
        SignInManager<Data.Models.User> signInManager,
        IExpertiseManager expertiseManager,
        IUserManager userManager,
        ITemplatedEmailSender templatedEmailSender,
        ISocialMediaSiteManager socialMediaSiteManager,
        TelemetryClient telemetryClient,
        ILogger<RegisterModel> logger)
    {
        _signInManager = signInManager;
        _expertiseManager = expertiseManager;
        _userManager = userManager;
        _templatedEmailSender = templatedEmailSender;
        _socialMediaSiteManager = socialMediaSiteManager;
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
    public IEnumerable<SocialMediaSite> SocialMediaSites { get; set; } = new List<SocialMediaSite>();

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

    public async Task<IActionResult> OnPostValidateEmailAsync()
    {
        string? email = Input.Email;
        
        if (string.IsNullOrWhiteSpace(email))
        {
            return new JsonResult(new { isValid = true, message = "" });
        }
        
        // Check if the email address format is valid
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return new JsonResult(new { isValid = false, message = "Please enter a valid email address." });
        }

        // Check if email is already in use
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return new JsonResult(new { isValid = false, message = "This email address is already in use." });
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
                    ModelState.AddModelError("Input.PhoneNumber", "Phone number with country code is required.");
                    ModelState.AddModelError("", "Phone number with country code is required.");
                }
                else
                {
                    // Allow formats like +1 (555) 123-4567 or 555-123-4567 etc.
                    var phone = Input.PhoneNumber!.Trim();
                    // Reject if contains letters
                    if (phone.Any(char.IsLetter))
                    {
                        ModelState.AddModelError("Input.PhoneNumber", "Please enter a valid phone number with country code.");
                        ModelState.AddModelError("", "Please enter a valid phone number with country code.");
                    }
                    else
                    {
                        // Ensure at least 11 digits present with country code
                        var digitCount = phone.Count(char.IsDigit);
                        if (digitCount < 11)
                        {
                            ModelState.AddModelError("Input.PhoneNumber", "Phone number with country code must contain at least 11 digits.");
                            ModelState.AddModelError("", "Phone number with country code must contain at least 11 digits.");
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
        foreach (var i in Models.RegistrationProgressions.All)
            if (!ValidateStep(i))
                allStepsValid = false;

        if (!allStepsValid)
        {
            // If we got this far, something failed, redisplay form with the current state
            await LoadFormDataAsync();
            CurrentStep =
                Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to the first step on major failure
            HasValidationErrors = true;
            ValidationMessage = "Registration failed. Please check the errors and try again.";
            return Partial("_RegistrationContainer", this);
        }

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
            _logger.LogError(ex, "Failed to save new user account");
            await LoadFormDataAsync();
            CurrentStep = Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to the first step on major failure
            HasValidationErrors = true;
            ValidationMessage = "The saving of registration failed. Please check the errors and try again.";
            // TODO: This is loads the RegistrationContainer inside itself, which is not ideal.
            return Partial("_RegistrationContainer", this);
        }

        if (!saveResult.Succeeded)
        {
            foreach (var error in saveResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            CurrentStep =
                Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to the first step on major failure
            HasValidationErrors = true;
            ValidationMessage = "The saving of registration failed. Please check the errors and try again.";
            // TODO: This is loads the RegistrationContainer inside itself, which is not ideal.
            return Partial("_RegistrationContainer", this);
        }

        _logger.LogInformation("User created a new account with password");

        // Load the user from the identity store
        user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            _logger.LogError("Failed to find user after saving registration. Email: '{Email}'", Input.Email);
            await LoadFormDataAsync();
            CurrentStep =
                Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to the first step on major failure
            HasValidationErrors = true;
            ValidationMessage = "Could not find user after saving registration. Please try again.";
            // TODO: This is loads the RegistrationContainer inside itself, which is not ideal.
            return Partial("_RegistrationContainer", this);
        }
        
        // User Experiences
        user.UserExpertise.Clear();
        foreach (var ue in Input.SelectedExpertiseIds)
        {
            user.UserExpertise.Add(new UserExpertise { ExpertiseId = ue });
        }
        
        // Social Media Sites
        var socialDictionary = SocialMediaSiteHelper.ParseSocialMediaPairs(Request.Form);
        user.UserSocialMediaSites.Clear();
        foreach (var kvp in socialDictionary)
        {
            user.UserSocialMediaSites.Add(new UserSocialMediaSite
                {
                    UserId = user.Id,
                    SocialMediaSiteId = kvp.Value.SiteId,
                    SocialId = kvp.Value.SocialId,
                    User = user,
                    SocialMediaSite = new SocialMediaSite { Id = kvp.Value.SiteId, Icon = string.Empty, Name = string.Empty, UrlFormat = string.Empty }
                }
            );
        }

        try
        {
            user = await _userManager.SaveAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the user's expertise's and social media sites. Email: '{Email}'", Input.Email);
            await LoadFormDataAsync();
            CurrentStep =
                Models.RegistrationProgressions.SpeakerProfileNeeded; // Reset to the first step on major failure
            HasValidationErrors = true;
            ValidationMessage = "Failed to save your expertise and social media sites. Please try again.";
            // TODO: This is loads the RegistrationContainer inside itself, which is not ideal.
            return Partial("_RegistrationContainer", this);
        }

        // Send the welcome email
        var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/Welcome.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.Welcome,
            "Welcome to MoreSpeakers.com - Your Speaking Journey Begins!", user, user);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship declined email to mentee");
            // TODO: Create a visual indicator that the email was not sent
        }

        // Send Email Confirmation
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationLink = Url.Page("/Account/ConfirmEmail",
            null,
            new { area = "Identity", token = encodedToken, email = user.Email },
            Request.Scheme);
        if (confirmationLink == null)
        {
            _logger.LogWarning("Failed to generate confirmation link for user {UserId}", user.Id);
        }
        else
        {
            var confirmationModel = new UserConfirmationEmail { ConfirmationUrl = confirmationLink, User = user };
            emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/ConfirmUserEmail.cshtml",
                Domain.Constants.TelemetryEvents.EmailGenerated.Confirmation,
                "Welcome to MoreSpeakers.com - Let's confirm your email!", user, confirmationModel);
            if (!emailSent)
            {
                _logger.LogError("Failed to send email confirmation email to user {UserId}", user.Id);
            }
        }

        // Load data needed for registration completion (step 5) display
        await LoadFormDataAsync();

        CurrentStep = Models.RegistrationProgressions.Complete;
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
        SuccessMessage = "Registration completed successfully!";

        // Return step 5 (confirmation) instead of redirecting away
        return Partial("_RegistrationContainer", this);

    }

    public async Task<IActionResult> OnGetAddSocialMediaRowAsync(int socialMediaSitesCount = 0)
    {
        try
        {
            socialMediaSitesCount++;
            var model = new UserSocialMediaSiteRow
            {
                UserSocialMediaSite = null,
                SocialMediaSites = await _socialMediaSiteManager.GetAllAsync(),
                ItemNumber = socialMediaSitesCount
            };
            return Partial("_UserSocialMediaSiteRow", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add a social media row");
        }
        return Content("");
    }
}