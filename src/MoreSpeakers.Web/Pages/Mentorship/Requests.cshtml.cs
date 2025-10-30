using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Models.ViewModels;

namespace MoreSpeakers.Web.Pages.Mentorship;


[Authorize]
public class RequestsModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IMentoringManager _mentoringManager;

    public RequestsModel(
        IMentoringManager mentoringManager,
        UserManager<User> userManager)
    {
        _mentoringManager = mentoringManager;       
        _userManager = userManager;
    }

    public List<Domain.Models.Mentorship> IncomingRequests { get; set; } = new();
    public List<Domain.Models.Mentorship> OutgoingRequests { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        IncomingRequests = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id);
        OutgoingRequests = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);

        return Page();
    }

    public async Task<IActionResult> OnGetDeclineModalAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _mentoringManager.GetAsync(mentorshipId);
        if (mentorship.MentorId != currentUser.Id)
            return NotFound();

        var viewModel = new DeclineMentorshipViewModel
        {
            Mentorship = mentorship
        };

        return Partial("_DeclineModal", viewModel);
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _mentoringManager.RespondToRequestAsync(mentorshipId, currentUser.Id, true, string.Empty);
        

        if (mentorship != null)
        {
            // Notify the client via HTMX events so both lists can refresh and any listeners can react
            Response.Headers["HX-Trigger"] = "{\"mentorship:accepted\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";
            // Return an OOB toast (partial renders as OOB) and remove the card by swapping empty content
            return Partial("_AcceptSuccess", mentorship);
        }

        return BadRequest();
    }

    public async Task<IActionResult> OnPostDeclineAsync(Guid mentorshipId, string? declineReason)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _mentoringManager.RespondToRequestAsync(mentorshipId, currentUser.Id, false, declineReason);
        
        if (mentorship != null)
        {
            Response.Headers["HX-Trigger"] = "{\"mentorship:declined\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";
            return Partial("_DeclineSuccess", mentorship);
        }

        return BadRequest();
    }

    public async Task<IActionResult> OnGetNotificationCountAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Content(string.Empty);

        var (incoming, outgoing) = await _mentoringManager.GetNumberOfMentorshipsPending(currentUser.Id);

        if (incoming > 0)
        {
            return Content($"<span class='badge bg-danger ms-1'>{incoming}</span>");
        }

        return Content(string.Empty);
    }

    public async Task<IActionResult> OnGetPollIncomingAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        IncomingRequests = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id);

        if (!IncomingRequests.Any())
        {
            return Content(@"
                <div class='text-center py-4'>
                    <i class='bi bi-inbox display-4 text-muted'></i>
                    <h6 class='mt-3'>No incoming requests</h6>
                    <p class='text-muted'>You'll see mentorship requests here when they arrive.</p>
                </div>");
        }

        return Partial("_IncomingRequests", IncomingRequests);
    }

    public async Task<IActionResult> OnGetPollOutgoingAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        OutgoingRequests = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);

        if (!OutgoingRequests.Any())
        {
            return Content(@"
                <div class='text-center py-4'>
                    <i class='bi bi-send display-4 text-muted'></i>
                    <h6 class='mt-3'>No outgoing requests</h6>
                    <p class='text-muted'>You haven't sent any mentorship requests yet.</p>
                    <a href='/Mentorship/Browse' class='btn btn-primary'>
                        <i class='bi bi-search me-2'></i>Find a Mentor
                    </a>
                </div>");
        }

        return Partial("_OutgoingRequests", OutgoingRequests);
    }

    public async Task<IActionResult> OnPostCancelRequestAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        // Ensure the current user is the mentee who created the request
        var wasCanceled = await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);
        
        if (!wasCanceled)
        {
            return BadRequest();
        }

        // Trigger updates so both lists can refresh
        Response.Headers["HX-Trigger"] = "{\"mentorship:cancelled\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";

        // Return empty content so hx-swap=\"outerHTML\" removes the card from the DOM
        return Content(string.Empty);
    }
}