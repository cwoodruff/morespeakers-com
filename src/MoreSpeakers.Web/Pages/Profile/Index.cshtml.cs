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
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        User? identityUser = null;
        try
        {
            identityUser = await userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return Challenge();
            }

            // Load profile with all the user's data
            var userProfile = await userManager.GetAsync(identityUser.Id);
            if (userProfile == null)
            {
                logger.LogError("Error loading profile page. Could not find user. UserId: '{UserId}'", identityUser?.Id);
                return RedirectToPage("/Profile/LoadingProblem",
                    new { UserId = identityUser?.Id ?? Guid.Empty });
            }
            
            ProfileUser = userProfile;
            UserExpertise = await userManager.GetUserExpertisesForUserAsync(identityUser.Id);
            CanEdit = true; // User can always edit their own profile

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading profile page. UserId: '{UserId}'", identityUser?.Id);
            return RedirectToPage("/Profile/LoadingProblem",
                new { UserId = identityUser?.Id ?? Guid.Empty });
        }

        return Page();
    }
}