using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Pages.Profile;

public class ViewModel(
    IUserManager userManager,
    ILogger<ViewModel> logger
    ) : PageModel
{

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public User? ProfileUser { get; set; }
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMedia> SocialMedia { get; set; } = new List<SocialMedia>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id == Guid.Empty)
        {
            return NotFound();
        }

        try
        {
            ProfileUser = await userManager.GetAsync(Id);

            if (ProfileUser == null)
            {
                return NotFound();
            }

            UserExpertise = await userManager.GetUserExpertisesForUserAsync(Id);

            SocialMedia = await userManager.GetUserSocialMediaForUserAsync(Id);

            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while loading the profile page. UserId: '{UserId}'", Id);
            return RedirectToPage("/Profile/LoadingProblem", new { userId = Id });
        }
    }
}
