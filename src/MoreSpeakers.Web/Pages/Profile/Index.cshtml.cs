using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Profile;

public class IndexModel(IUserManager userManager, ILogger<IndexModel> logger) : PageModel
{

    public User UserProfile { get; set; } = null!;
    public bool CanEdit { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; } = null;

    public async Task<IActionResult> OnGetAsync()
    {
        // There are two possible views for this page
        // - A person viewing another persons profile. (default)
        //    This requires just an Id query parameter
        // - A logged in user that is visiting their profile
        //    This happens if a person to be signed in and no Id query parameter passed
        //    This happens if a person to be signed in and their Id passed
        
        User? identityUser = null;

        try
        {
            identityUser = await userManager.GetUserAsync(User);

            User? userProfile;
            if (Id.HasValue && Id.Value != Guid.Empty)
            {
                userProfile = await userManager.GetAsync(Id.Value);    
                if (userProfile == null)
                {
                    logger.LogError("Error loading profile page. Could not find user. UserId: '{UserId}'", identityUser?.Id);
                    return RedirectToPage("/Profile/LoadingProblem",
                        new { UserId = Id});
                }

                if (userProfile.Id == identityUser?.Id)
                {
                    CanEdit = true;
                }
                UserProfile = userProfile;    
            }
            else
            {
                if (identityUser is not null)
                {
                    userProfile = await userManager.GetAsync(identityUser.Id);    
                    if (userProfile == null)
                    {
                        logger.LogError("Error loading profile page. Could not find user. UserId: '{UserId}'", identityUser.Id);
                        return RedirectToPage("/Profile/LoadingProblem",
                            new { UserId = identityUser.Id});
                    }
                    UserProfile = userProfile; 
                    if (userProfile.Id == identityUser?.Id)
                    {
                        CanEdit = true;
                    }
                }
                else
                {
                    return RedirectToPage("/Profile/LoadingProblem",
                        new { UserId = Guid.Empty });
                }
            }
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