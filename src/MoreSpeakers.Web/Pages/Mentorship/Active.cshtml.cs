using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Mentorship;

[Authorize]
public class ActiveModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly IMentoringManager _mentoringManager;
    private readonly ILogger<ActiveModel> _logger;

    public ActiveModel(
        IUserManager userManager,
        IMentoringManager mentorshipManager,
        ILogger<ActiveModel> logger)
    {
        _userManager = userManager;
        _mentoringManager = mentorshipManager;
        _logger = logger;       
    }

    public List<Domain.Models.Mentorship> ActiveMentorships { get; set; } = [];
    public Guid CurrentUserId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            CurrentUserId = currentUser.Id;

            ActiveMentorships = await _mentoringManager.GetActiveMentorshipsForUserAsync(CurrentUserId);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load active mentorships for user '{User}'", currentUser?.Id);
            return BadRequest();       
        }
    }

    public async Task<IActionResult> OnPostCompleteMentorshipAsync(Guid mentorshipId)
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            await _mentoringManager.CompleteMentorshipRequestAsync(mentorshipId, currentUser.Id);

            // Let the client know lists/pages can refresh if listening
            Response.Headers["HX-Trigger"] = "{\"mentorship:completed\":{\"id\":\"" + mentorshipId +
                                             "\"},\"mentorship:updated\":true}";

            // Remove the card from the DOM; page summary is static until next full load
            return Content(string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete mentorship request for user '{User}'", currentUser?.Id);
            return BadRequest();       
        }       
    }

    public async Task<IActionResult> OnPostCancelMentorshipAsync(Guid mentorshipId)
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);

            Response.Headers["HX-Trigger"] = "{\"mentorship:cancelled\":{\"id\":\"" + mentorshipId +
                                             "\"},\"mentorship:updated\":true}";

            return Content(string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel mentorship request for user '{User}'", currentUser?.Id);
            return BadRequest();       
        }       
    }
}