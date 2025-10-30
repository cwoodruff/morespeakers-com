using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Mentorship;

[Authorize]
public class ActiveModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IMentoringManager _mentoringManager;

    public ActiveModel(
        UserManager<User> userManager,
        IMentoringManager mentorshipManager)
    {
        _userManager = userManager;
        _mentoringManager = mentorshipManager;       
    }

    public List<Domain.Models.Mentorship> ActiveMentorships { get; set; } = new();
    public Guid CurrentUserId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        CurrentUserId = currentUser.Id;

        ActiveMentorships = await _mentoringManager.GetActiveMentorshipsForUserAsync(CurrentUserId);

        return Page();
    }

    public async Task<IActionResult> OnPostCompleteMentorshipAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        await _mentoringManager.CompleteMentorshipRequestAsync(mentorshipId, currentUser.Id);
        
        // Let the client know lists/pages can refresh if listening
        Response.Headers["HX-Trigger"] = "{\"mentorship:completed\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";

        // Remove the card from the DOM; page summary is static until next full load
        return Content(string.Empty);
    }

    public async Task<IActionResult> OnPostCancelMentorshipAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);

        Response.Headers["HX-Trigger"] = "{\"mentorship:cancelled\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";

        return Content(string.Empty);
    }
}