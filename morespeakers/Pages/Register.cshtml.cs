using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using morespeakers.Models;
using morespeakers.Services;

namespace morespeakers.Pages;

public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IExpertiseService _expertiseService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ISpeakerService _speakerService;

    public RegisterModel(
        UserManager<User> userManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        IExpertiseService expertiseService,
        IFileUploadService fileUploadService,
        ISpeakerService speakerService)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _expertiseService = expertiseService;
        _fileUploadService = fileUploadService;
        _speakerService = speakerService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public IEnumerable<Expertise> AllExpertise { get; set; } = new List<Expertise>();

    public class InputModel
    {
        [Required]
        [Display(Name = "Speaker Type")]
        public int SpeakerTypeId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        [StringLength(6000, ErrorMessage = "The bio cannot exceed 6000 characters (approximately 1000 words).")]
        [Display(Name = "Bio")]
        public string Bio { get; set; } = "";

        [Required]
        [StringLength(2000, ErrorMessage = "The goals description cannot exceed 2000 characters.")]
        [Display(Name = "Goals")]
        public string Goals { get; set; } = "";

        [Url]
        [Display(Name = "Sessionize Profile URL")]
        public string? SessionizeUrl { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile? HeadshotFile { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Please select at least one area of expertise.")]
        public List<int> SelectedExpertiseIds { get; set; } = new();

        public List<string> CustomExpertise { get; set; } = new();

        [Required]
        [MinLength(1, ErrorMessage = "Please provide at least one social media link.")]
        public List<string> SocialMediaPlatforms { get; set; } = new();

        [Required]
        [MinLength(1, ErrorMessage = "Please provide at least one social media link.")]
        public List<string> SocialMediaUrls { get; set; } = new();
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        AllExpertise = await _expertiseService.GetAllExpertiseAsync();
    }

    public async Task<IActionResult> OnPostValidateEmailAsync()
    {
        var result = new { IsValid = true, Message = "" };
        
        if (string.IsNullOrWhiteSpace(Input.Email))
        {
            result = new { IsValid = false, Message = "Email is required." };
        }
        else if (!new EmailAddressAttribute().IsValid(Input.Email))
        {
            result = new { IsValid = false, Message = "Please enter a valid email address." };
        }
        else
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                result = new { IsValid = false, Message = "This email address is already registered." };
            }
        }
        
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostValidatePasswordAsync()
    {
        var result = new { IsValid = true, Message = "" };
        
        if (string.IsNullOrWhiteSpace(Input.Password))
        {
            result = new { IsValid = false, Message = "Password is required." };
        }
        else if (Input.Password.Length < 6)
        {
            result = new { IsValid = false, Message = "Password must be at least 6 characters long." };
        }
        else if (Input.Password != Input.ConfirmPassword)
        {
            result = new { IsValid = false, Message = "Password and confirmation password do not match." };
        }
        
        return new JsonResult(result);
    }

    public IActionResult OnPostValidateExpertiseAsync()
    {
        var result = new { IsValid = true, Message = "" };
        
        if (!Input.SelectedExpertiseIds.Any() && !Input.CustomExpertise.Any(ce => !string.IsNullOrWhiteSpace(ce)))
        {
            result = new { IsValid = false, Message = "Please select at least one area of expertise." };
        }
        
        return new JsonResult(result);
    }

    public IActionResult OnPostValidateSocialMediaAsync()
    {
        var result = new { IsValid = true, Message = "" };
        
        var validSocialMediaCount = 0;
        for (int i = 0; i < Input.SocialMediaPlatforms.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(Input.SocialMediaPlatforms[i]) && 
                !string.IsNullOrWhiteSpace(Input.SocialMediaUrls[i]))
            {
                validSocialMediaCount++;
            }
        }

        if (validSocialMediaCount == 0)
        {
            result = new { IsValid = false, Message = "At least one social media link is required." };
        }
        
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        AllExpertise = await _expertiseService.GetAllExpertiseAsync();

        // Validate social media inputs
        if (Input.SocialMediaPlatforms.Count != Input.SocialMediaUrls.Count)
        {
            ModelState.AddModelError("", "Social media platforms and URLs count mismatch.");
        }

        // Validate that at least one social media link is provided
        var validSocialMediaCount = 0;
        for (int i = 0; i < Input.SocialMediaPlatforms.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(Input.SocialMediaPlatforms[i]) && 
                !string.IsNullOrWhiteSpace(Input.SocialMediaUrls[i]))
            {
                validSocialMediaCount++;
            }
        }

        if (validSocialMediaCount == 0)
        {
            ModelState.AddModelError("", "At least one social media link is required.");
        }

        // Validate expertise selection
        if (!Input.SelectedExpertiseIds.Any() && !Input.CustomExpertise.Any())
        {
            ModelState.AddModelError("Input.SelectedExpertiseIds", "Please select at least one area of expertise.");
        }

        // Validate file upload
        if (Input.HeadshotFile != null && !_fileUploadService.IsValidImageFile(Input.HeadshotFile))
        {
            ModelState.AddModelError("Input.HeadshotFile", "Please upload a valid image file (PNG, JPG, GIF) under 5MB.");
        }

        if (ModelState.IsValid)
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            
            // Set additional properties
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.PhoneNumber = Input.PhoneNumber;
            user.Bio = Input.Bio;
            user.Goals = Input.Goals;
            user.SpeakerTypeId = Input.SpeakerTypeId;
            user.SessionizeUrl = Input.SessionizeUrl;

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                try
                {
                    // Handle file upload
                    if (Input.HeadshotFile != null)
                    {
                        var fileName = await _fileUploadService.UploadHeadshotAsync(Input.HeadshotFile, user.Id);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            user.HeadshotUrl = _fileUploadService.GetHeadshotPath(fileName);
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    // Add expertise areas
                    foreach (var expertiseId in Input.SelectedExpertiseIds)
                    {
                        await _speakerService.AddExpertiseToUserAsync(user.Id, expertiseId);
                    }

                    // Add custom expertise areas
                    foreach (var customExpertise in Input.CustomExpertise.Where(ce => !string.IsNullOrWhiteSpace(ce)))
                    {
                        // Check if expertise already exists
                        var existingExpertise = (await _expertiseService.SearchExpertiseAsync(customExpertise.Trim()))
                            .FirstOrDefault(e => e.Name.Equals(customExpertise.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (existingExpertise != null)
                        {
                            await _speakerService.AddExpertiseToUserAsync(user.Id, existingExpertise.Id);
                        }
                        else
                        {
                            // Create new expertise
                            if (await _expertiseService.CreateExpertiseAsync(customExpertise.Trim()))
                            {
                                var newExpertise = (await _expertiseService.SearchExpertiseAsync(customExpertise.Trim())).First();
                                await _speakerService.AddExpertiseToUserAsync(user.Id, newExpertise.Id);
                            }
                        }
                    }

                    // Add social media links
                    for (int i = 0; i < Input.SocialMediaPlatforms.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(Input.SocialMediaPlatforms[i]) && 
                            !string.IsNullOrWhiteSpace(Input.SocialMediaUrls[i]))
                        {
                            await _speakerService.AddSocialMediaLinkAsync(
                                user.Id, 
                                Input.SocialMediaPlatforms[i], 
                                Input.SocialMediaUrls[i]);
                        }
                    }

                    // Add user to appropriate role
                    var roleName = Input.SpeakerTypeId == 1 ? "NewSpeaker" : "ExperiencedSpeaker";
                    await _userManager.AddToRoleAsync(user, roleName);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    
                    // For now, auto-confirm email (you can implement email confirmation later)
                    await _userManager.ConfirmEmailAsync(user, code);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while setting up user profile");
                    ModelState.AddModelError("", "An error occurred while setting up your profile. Please try again.");
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