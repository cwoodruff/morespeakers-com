#nullable disable

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Web.Data;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly Domain.Interfaces.IEmailSender _emailSender;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly IExpertiseService _expertiseService;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly TelemetryClient _telemetryClient;

    public RegisterModel(
        UserManager<User> userManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        Domain.Interfaces.IEmailSender emailSender,
        IExpertiseService expertiseService,
        ApplicationDbContext context,
        TelemetryClient telemetryClient)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _expertiseService = expertiseService;
        _context = context;
        _telemetryClient = telemetryClient;
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

    // Property to expose selected expertise IDs for registration completion (step 5_
    public int[] ExpertiseIds => Input?.SelectedExpertiseIds ?? [];

    // Properties required by _RegistrationContainer.cshtml
    public int CurrentStep { get; set; } = RegistrationProgressions.SpeakerProfileNeeded;
    public bool HasValidationErrors { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;


    public async Task OnGetAsync()
    {
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        // Initialize Input model if not already initialized
        Input ??= new InputModel();

        // Initialize registration state
        CurrentStep = RegistrationProgressions.SpeakerProfileNeeded;
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
        if (!RegistrationProgressions.IsValid(step))
        {
            ModelState.AddModelError("", "Invalid step number.");
            return BadRequest();
        }

        // Reload necessary data for the view
        await LoadFormDataAsync();

        // Validate current step
        var stepValid = ValidateStep(step);

        // Additional async validations for step 1 (e.g., email uniqueness)
        if (step == RegistrationProgressions.SpeakerProfileNeeded && stepValid)
        {
            if (!string.IsNullOrWhiteSpace(Input?.Email))
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
        if (nextStep <= RegistrationProgressions.SocialMediaNeeded)
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
            RegistrationProgressions.SpeakerProfileNeeded => "Please complete all required account information before proceeding.",
            RegistrationProgressions.RequiredInformationNeeded => "Please fill out your speaker profile completely.",
            RegistrationProgressions.ExpertiseNeeded => "Please select at least one area of expertise.",
            RegistrationProgressions.SocialMediaNeeded => "Please review your social media information.",
            _ => "Please complete the required information."
        };
    }

    private string GetSuccessMessage(int step)
    {
        return step switch
        {
            RegistrationProgressions.SpeakerProfileNeeded => "Account information saved successfully!",
            RegistrationProgressions.RequiredInformationNeeded => "Profile information saved successfully!",
            RegistrationProgressions.ExpertiseNeeded => "Expertise areas saved successfully!",
            _ => "Information saved successfully!"
        };
    }

    public async Task<IActionResult> OnPostPreviousStepAsync(int step)
    {
        // Reload necessary data for the view
        await LoadFormDataAsync();

        // Return previous step without validation
        var prevStep = step - 1;
        if (prevStep < RegistrationProgressions.SpeakerProfileNeeded)
            prevStep = RegistrationProgressions.SpeakerProfileNeeded;

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
        var existingExpertise = await _context.Expertise
            .Where(e => e.Name.ToLower() == trimmedName.ToLower())
            .FirstOrDefaultAsync();

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
        var similarExpertise = await _context.Expertise
            .Where(e => e.Name.ToLower().Contains(trimmedName.ToLower()) ||
                       trimmedName.ToLower().Contains(e.Name.ToLower()))
            .Take(3)
            .Select(e => new { e.Id, e.Name })
            .ToListAsync();

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
        AvailableExpertise = await _expertiseService.GetAllExpertiseAsync();
        AllExpertise = AvailableExpertise; // Same data, different property name for registration completion (step 5)
        SpeakerTypes = new List<SpeakerType>
        {
            new() { Id = 1, Name = "NewSpeaker", Description = "I'm new to speaking and looking for mentorship" },
            new()
            {
                Id = 2, Name = "ExperiencedSpeaker", Description = "I'm an experienced speaker willing to mentor others"
            }
        };
    }

    private bool ValidateStep(int step)
    {
        // Clear all model state errors first
        ModelState.Clear();

        switch (step)
        {
            case RegistrationProgressions.SpeakerProfileNeeded: // Account step
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
            case RegistrationProgressions.RequiredInformationNeeded: // Profile step
                if (Input.SpeakerTypeId <= 0)
                    ModelState.AddModelError("Input.SpeakerTypeId", "Please select a speaker type.");
                if (string.IsNullOrWhiteSpace(Input.Bio))
                    ModelState.AddModelError("Input.Bio", "Bio is required.");
                if (string.IsNullOrWhiteSpace(Input.Goals))
                    ModelState.AddModelError("Input.Goals", "Goals are required.");
                break;
            case RegistrationProgressions.ExpertiseNeeded: // Expertise step
                if ((Input.SelectedExpertiseIds?.Length ?? 0) == 0 && (Input.CustomExpertise?.Length ?? 0) == 0)
                    ModelState.AddModelError("Input.SelectedExpertiseIds",
                        "Please select at least one area of expertise.");
                break;
            case RegistrationProgressions.SocialMediaNeeded: // Social step - optional, no validation needed
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
        foreach(var i in RegistrationProgressions.All)
            if (!ValidateStep(i))
                allStepsValid = false;

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
                    _context.UserExpertise.Add(new UserExpertise
                    {
                        UserId = user.Id,
                        ExpertiseId = expertiseId
                    });

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
                for (var i = 0; i < Input.SocialMediaPlatforms.Length && i < Input.SocialMediaUrls.Length; i++)
                    if (!string.IsNullOrWhiteSpace(Input.SocialMediaPlatforms[i]) &&
                        !string.IsNullOrWhiteSpace(Input.SocialMediaUrls[i]))
                        _context.SocialMedia.Add(new SocialMedia
                        {
                            UserId = user.Id,
                            Platform = Input.SocialMediaPlatforms[i],
                            Url = Input.SocialMediaUrls[i],
                            CreatedDate = DateTime.UtcNow
                        });

                await _context.SaveChangesAsync();

                // Send welcome email (mock implementation)
                await SendWelcomeEmailAsync(user);

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    null,
                    new { area = "Identity", userId, code, S = "~//" },
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

                CurrentStep = RegistrationProgressions.Complete;
                HasValidationErrors = false;
                ValidationMessage = string.Empty;
                SuccessMessage = "Registration completed successfully!";

                // Return step 5 (confirmation) instead of redirecting away
                return Partial("_RegistrationContainer", this);
            }

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        }

        // If we got this far, something failed, redisplay form with current state
        await LoadFormDataAsync();
        CurrentStep = RegistrationProgressions.SpeakerProfileNeeded; // Reset to first step on major failure
        HasValidationErrors = true;
        ValidationMessage = "Registration failed. Please check the errors and try again.";
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
            throw new NotSupportedException("The default UI requires a user store with email support.");
        return (IUserEmailStore<User>)_userStore;
    }

    /// <summary>
    /// Sends a welcome email to the newly registered user (mock implementation)
    /// </summary>
    private async Task SendWelcomeEmailAsync(User user)
    {
        var speakerType = user.SpeakerTypeId == 1 ? "New Speaker" : "Experienced Speaker";
        var emailSubject = "Welcome to MoreSpeakers.com - Your Speaking Journey Begins!";

        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Welcome to MoreSpeakers.com</title>
    <style>
        body {{ font-family: 'Inter', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #fd7e14 0%, #e55d0e 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: white; padding: 30px; border-radius: 0 0 8px 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }}
        .badge {{ background: #fd7e14; color: white; padding: 8px 16px; border-radius: 20px; display: inline-block; font-size: 14px; font-weight: bold; }}
        .next-steps {{ background: #f8f9fa; padding: 20px; border-radius: 6px; margin: 20px 0; }}
        .step {{ display: flex; align-items: center; margin: 15px 0; }}
        .step-number {{ background: #fd7e14; color: white; width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px; font-weight: bold; }}
        .btn {{ background: #fd7e14; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎤 Welcome to MoreSpeakers.com!</h1>
            <p>Your speaking journey starts here</p>
        </div>
        
        <div class='content'>
            <h2>Hello {user.FirstName}!</h2>
            
            <p>Congratulations on joining the MoreSpeakers.com community! We're thrilled to have you as a <span class='badge'>{speakerType}</span> in our growing network of passionate speakers.</p>
            
            <h3>Your Registration Details:</h3>
            <ul>
                <li><strong>Name:</strong> {user.FirstName} {user.LastName}</li>
                <li><strong>Email:</strong> {user.Email}</li>
                <li><strong>Speaker Type:</strong> {speakerType}</li>
                <li><strong>Registration Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</li>
            </ul>
            
            <div class='next-steps'>
                <h3>🚀 What's Next?</h3>
                
                <div class='step'>
                    <div class='step-number'>1</div>
                    <div>
                        <strong>Complete Your Profile</strong><br>
                        <small>Add more details, upload a headshot, and showcase your expertise to help others find and connect with you.</small>
                    </div>
                </div>
                
                <div class='step'>
                    <div class='step-number'>2</div>
                    <div>
                        <strong>{(user.SpeakerTypeId == 1 ? "Find Mentors" : "Connect with New Speakers")}</strong><br>
                        <small>{(user.SpeakerTypeId == 1
                            ? "Browse our community of experienced speakers and find mentors who can guide your speaking journey."
                            : "Discover new speakers who could benefit from your experience and mentorship.")}</small>
                    </div>
                </div>

                <div class='step'>
                    <div class='step-number'>3</div>
                    <div>
                        <strong>Join the Community</strong><br>
                        <small>Participate in discussions, share your experiences, and connect with fellow speakers from around the world.</small>
                    </div>
                </div>
            </div>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='https://localhost:5001/Profile/Edit' class='btn'>Complete My Profile →</a>
            </div>

            <h3>📧 Email Confirmation Required</h3>
            <p>To activate your account and access all features, please check your email for a confirmation link. If you don't see it in your inbox, don't forget to check your spam folder.</p>

            <h3>🤝 Community Guidelines</h3>
            <p>We're building a supportive and inclusive community. Please be respectful, helpful, and authentic in all your interactions.</p>

        </div>

        <div class='footer'>
            <p>
                <strong>MoreSpeakers.com</strong><br>
                Connecting speakers, sharing knowledge, building community<br>
                <a href='mailto:support@morespeakers.com'>support@morespeakers.com</a>
            </p>
            <p style='font-size: 12px; color: #999;'>
                You received this email because you registered an account at MoreSpeakers.com.<br>
                If you have any questions, please contact our support team.
            </p>
        </div>
    </div>
</body>
</html>";

        try
        {
            // Mock implementation - log the email instead of actually sending it
            _logger.LogInformation("MOCK EMAIL SENT");
            _logger.LogInformation("To: {Email}", user.Email);
            _logger.LogInformation("Subject: {Subject}", emailSubject);
            _logger.LogInformation("Body: {Body}", emailBody);

            await _emailSender.QueueEmail(new System.Net.Mail.MailAddress(user.Email!, $"{user.FirstName} {user.LastName}"),
                emailSubject, emailBody);

            _telemetryClient.TrackEvent("WelcomeEmailSent", new Dictionary<string, string>
            {
                { "UserId", user.Id.ToString() },
                { "Email", user.Email }
            });
            _logger.LogInformation("Welcome email successfully sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            // Don't throw - registration should still succeed even if email fails
        }
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 6)]
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

        [Display(Name = "Custom Expertise")] public string[] CustomExpertise { get; set; } = Array.Empty<string>();

        // Social Media Links
        [Display(Name = "Social Media Platforms")]
        public string[] SocialMediaPlatforms { get; set; } = Array.Empty<string>();

        [Display(Name = "Social Media URLs")] public string[] SocialMediaUrls { get; set; } = Array.Empty<string>();
    }
}