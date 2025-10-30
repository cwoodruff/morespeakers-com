using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Profile;

[Authorize]
public class IndexModel(
    ISpeakerManager speakerManager,
    UserManager<User> userManager) : PageModel
{
    private readonly ISpeakerManager _speakerManager = speakerManager;
    private readonly UserManager<User> _userManager = userManager;

    public User ProfileUser { get; set; } = null!;
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMedia> SocialMedia { get; set; } = new List<SocialMedia>();
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        ProfileUser = await _speakerManager.GetAsync(currentUser.Id);
        UserExpertise = await _speakerManager.GetUserExpertisesForUserAsync(currentUser.Id);
        SocialMedia = await _speakerManager.GetUserSocialMediaForUserAsync(currentUser.Id);
        CanEdit = true; // User can always edit their own profile

        return Page();
    }
}