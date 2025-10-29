using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Account;

[Authorize]
public partial class IndexModel : PageModel
{
    private readonly ISpeakerManager _speakerManager;
    private readonly IExpertiseManager _expertiseManager;
    private readonly UserManager<User> _userManager;

    public IndexModel(
        ISpeakerManager speakerManager,
        IExpertiseManager expertiseManager,
        UserManager<User> userManager
        )
    {
        _speakerManager = speakerManager;
        _expertiseManager = expertiseManager;
        _userManager = userManager;
    }

    [BindProperty] public EditAccountModel Input { get; set; } = new();

    public User CurrentUser { get; set; } = null!;
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMedia> SocialMedia { get; set; } = new List<SocialMedia>();
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

            await _userManager.UpdateAsync(CurrentUser);
            await LoadUserDataAsync(); // Reload to get updated data

            ViewData["Field"] = field;
            ViewData["PageModel"] = this;
            return Partial("_DisplayField");
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An error occurred while updating your information.");
            ViewData["Field"] = field;
            ViewData["PageModel"] = this;
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
        var userIdString = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new InvalidOperationException("Invalid user ID");

        CurrentUser = await _speakerManager.GetAsync(userId);

        UserExpertise = await _speakerManager.GetUserExpertisesForUserAsync(userId);

        SocialMedia = await _speakerManager.GetUserSocialMediaForUserAsync(userId);

        AvailableExpertise = await _expertiseManager.GetAllAsync();
        SpeakerType = CurrentUser.SpeakerType;

        // Populate input model with current values
        Input.FirstName = CurrentUser.FirstName;
        Input.LastName = CurrentUser.LastName;
        Input.Email = CurrentUser.Email;
        Input.PhoneNumber = CurrentUser.PhoneNumber;
        Input.Bio = CurrentUser.Bio;
        Input.Goals = CurrentUser.Goals;
        Input.SessionizeUrl = CurrentUser.SessionizeUrl;
        Input.HeadshotUrl = CurrentUser.HeadshotUrl;
        Input.SpeakerTypeId = CurrentUser.SpeakerTypeId;
    }
}