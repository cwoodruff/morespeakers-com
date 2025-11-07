using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Profile;

[Authorize]
public class IndexModel(IUserManager userManager, ILogger<IndexModel> logger) : PageModel
{

    public User ProfileUser { get; set; } = null!;
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMedia> SocialMedia { get; set; } = new List<SocialMedia>();
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        User? currentUser = null;
        try
        {
            currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            ProfileUser = await userManager.GetAsync(currentUser.Id);
            UserExpertise = await userManager.GetUserExpertisesForUserAsync(currentUser.Id);
            SocialMedia = await userManager.GetUserSocialMediaForUserAsync(currentUser.Id);
            CanEdit = true; // User can always edit their own profile

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading profile page. UserId: '{UserId}'", currentUser?.Id);
            return RedirectToPage("/Profile/LoadingProblem", new { userId = User });
        }

        return Page();
    }
}