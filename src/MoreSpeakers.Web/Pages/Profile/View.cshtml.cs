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

    public User ProfileUser { get; set; } = new();
    
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id == Guid.Empty)
        {
            return RedirectToPage("/Profile/LoadingProblem", new { UserId = Guid.Empty });
        }

        try
        {
            var profileUser = await userManager.GetAsync(Id);
            if (profileUser is null)
            {
                return RedirectToPage("/Profile/LoadingProblem", new { UserId = Id });
            }

            ProfileUser = profileUser;
            UserExpertise = await userManager.GetUserExpertisesForUserAsync(Id);

            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while loading the profile page. UserId: '{UserId}'", Id);
            return RedirectToPage("/Profile/LoadingProblem", new { UserId = Id });
        }
    }
}
