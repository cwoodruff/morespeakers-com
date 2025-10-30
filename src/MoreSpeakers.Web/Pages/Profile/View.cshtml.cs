using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Pages.Profile;

public class ViewModel(
    ISpeakerManager speakerManager
    ) : PageModel
{
    private readonly ISpeakerManager _speakerManager = speakerManager;

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

        ProfileUser = await _speakerManager.GetAsync(Id);

        if (ProfileUser == null)
        {
            return NotFound();
        }

        UserExpertise = await _speakerManager.GetUserExpertisesForUserAsync(Id);

        SocialMedia = await _speakerManager.GetUserSocialMediaForUserAsync(Id);

        return Page();
    }
}
