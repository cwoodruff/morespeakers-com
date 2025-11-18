using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Account;

[Authorize]
public partial class IndexModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly IExpertiseManager _expertiseManager;
    private readonly ISocialMediaSiteManager _socialMediaSiteManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IUserManager userManager,
        IExpertiseManager expertiseManager,
        ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _expertiseManager = expertiseManager;
        _logger = logger;      
    }

    [BindProperty] public EditAccountModel Input { get; set; } = new();

    public User CurrentUser { get; set; } = null!;
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<Expertise> AvailableExpertise { get; set; } = new List<Expertise>();
    public SpeakerType? SpeakerType { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadUserDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEditFieldAsync(string field)
    {
        await LoadUserDataAsync();

        if (!ModelState.IsValid)
        {
            ViewData["Field"] = field;
            ViewData["PageModel"] = this;
            return Partial("_EditField", this);
        }

        try
        {
            // Update the specific field
            switch (field.ToLower())
            {
                case "firstname":
                    CurrentUser.FirstName = Input.FirstName;
                    break;
                case "lastname":
                    CurrentUser.LastName = Input.LastName;
                    break;
                case "email":
                    CurrentUser.Email = Input.Email;
                    break;
                case "phonenumber":
                    CurrentUser.PhoneNumber = Input.PhoneNumber;
                    break;
                case "bio":
                    CurrentUser.Bio = Input.Bio;
                    break;
                case "goals":
                    CurrentUser.Goals = Input.Goals;
                    break;
                case "sessionizeurl":
                    CurrentUser.SessionizeUrl = Input.SessionizeUrl;
                    break;
                case "headshoturl":
                    CurrentUser.HeadshotUrl = Input.HeadshotUrl;
                    break;
                case "speakertypeid":
                    CurrentUser.SpeakerTypeId = Input.SpeakerTypeId;
                    break;
            }

            await _userManager.SaveAsync(CurrentUser);
            await LoadUserDataAsync(); // Reload to get updated data

            ViewData["Field"] = field;
            ViewData["PageModel"] = this;

            return Partial("_DisplayField");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while updating your information.");
            ViewData["Field"] = field;
            ViewData["PageModel"] = this;
            
            _logger.LogError(ex, "Error updating user information for user '{UserId}'", CurrentUser.Id);
            return Partial("_EditField", this);
        }
    }

    public async Task<IActionResult> OnGetEditFieldAsync(string field)
    {
        await LoadUserDataAsync();
        ViewData["Field"] = field;
        ViewData["PageModel"] = this;
        return Partial("_EditField", this);
    }

    public async Task<IActionResult> OnGetCancelEditAsync(string field)
    {
        await LoadUserDataAsync();
        ViewData["Field"] = field;
        ViewData["PageModel"] = this;
        return Partial("_DisplayField");
    }

    private async Task LoadUserDataAsync()
    {
        User? user = null;

        try
        {
            user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                RedirectToPage("/Profile/LoadingProblem", new { UserId = Guid.Empty });
                return;
            }
            
            var currentUser = await _userManager.GetAsync(user.Id);
            if (currentUser == null)
            {
                RedirectToPage("/Profile/LoadingProblem", new { UserId = Guid.Empty });
                return;
            }

            CurrentUser = currentUser;
            UserExpertise = await _userManager.GetUserExpertisesForUserAsync(user.Id);
            AvailableExpertise = await _expertiseManager.GetAllAsync();
            SpeakerType = CurrentUser.SpeakerType;

            // Populate the input model with current values
            Input.FirstName = CurrentUser.FirstName;
            Input.LastName = CurrentUser.LastName;
            Input.Email = CurrentUser.Email!;
            Input.PhoneNumber = CurrentUser.PhoneNumber;
            Input.Bio = CurrentUser.Bio;
            Input.Goals = CurrentUser.Goals;
            Input.SessionizeUrl = CurrentUser.SessionizeUrl;
            Input.HeadshotUrl = CurrentUser.HeadshotUrl;
            Input.SpeakerTypeId = CurrentUser.SpeakerTypeId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user data for user '{User}'", user?.Id);
        }       
    }
}