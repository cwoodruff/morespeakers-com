using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Web.Data;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Mentorship;


[Authorize]
public class RequestsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMentorshipService _mentorshipService;

    public RequestsModel(ApplicationDbContext context, UserManager<User> userManager, IMentorshipService mentorshipService)
    {
        _context = context;
        _userManager = userManager;
        _mentorshipService = mentorshipService;
    }

    public List<Models.Mentorship> IncomingRequests { get; set; } = new();
    public List<Models.Mentorship> OutgoingRequests { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        IncomingRequests = await GetIncomingRequests(currentUser.Id);
        OutgoingRequests = await GetOutgoingRequests(currentUser.Id);

        return Page();
    }

    public async Task<IActionResult> OnGetDeclineModalAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _mentorshipService.GetMentorshipByIdAsync(mentorshipId);
        if (mentorship == null || mentorship.MentorId != currentUser.Id)
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

        var mentorship = await _mentorshipService.GetMentorshipByIdAsync(mentorshipId);
        if (mentorship == null || mentorship.MentorId != currentUser.Id)
            return NotFound();

        var success = await _mentorshipService.AcceptMentorshipAsync(mentorshipId);

        if (success)
        {
            var updatedMentorship = await _mentorshipService.GetMentorshipByIdAsync(mentorshipId);
            // Notify the client via HTMX events so both lists can refresh and any listeners can react
            Response.Headers["HX-Trigger"] = "{\"mentorship:accepted\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";
            // Return an OOB toast (partial renders as OOB) and remove the card by swapping empty content
            return Partial("_AcceptSuccess", updatedMentorship);
        }

        return BadRequest();
    }

    public async Task<IActionResult> OnPostDeclineAsync(Guid mentorshipId, string? declineReason)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _mentorshipService.GetMentorshipByIdAsync(mentorshipId);
        if (mentorship == null || mentorship.MentorId != currentUser.Id)
            return NotFound();

        var success = await _mentorshipService.DeclineMentorshipAsync(mentorshipId, declineReason);

        if (success)
        {
            var updatedMentorship = await _mentorshipService.GetMentorshipByIdAsync(mentorshipId);
            Response.Headers["HX-Trigger"] = "{\"mentorship:declined\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";
            return Partial("_DeclineSuccess", updatedMentorship);
        }

        return BadRequest();
    }

    public async Task<IActionResult> OnGetNotificationCountAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Content(string.Empty);

        var (incoming, outgoing) = await _mentorshipService.GetPendingCountsAsync(currentUser.Id);

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

        IncomingRequests = await GetIncomingRequests(currentUser.Id);

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

        OutgoingRequests = await GetOutgoingRequests(currentUser.Id);

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

    private async Task<List<Models.Mentorship>> GetIncomingRequests(Guid userId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentee)
                .ThenInclude(m => m.SpeakerType)
            .Include(m => m.FocusAreas)
                .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MentorId == userId && m.Status == MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    private async Task<List<Models.Mentorship>> GetOutgoingRequests(Guid userId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
                .ThenInclude(m => m.SpeakerType)
            .Include(m => m.FocusAreas)
                .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MenteeId == userId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCancelRequestAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        // Ensure the current user is the mentee who created the request
        var mentorship = await _context.Mentorship.FirstOrDefaultAsync(m => m.Id == mentorshipId);
        if (mentorship == null || mentorship.MenteeId != currentUser.Id)
        {
            return NotFound();
        }

        if (mentorship.Status != MentorshipStatus.Pending)
        {
            return BadRequest("Only pending requests can be cancelled.");
        }

        var success = await _mentorshipService.CancelMentorshipAsync(mentorshipId);
        if (!success)
        {
            return BadRequest();
        }

        // Trigger updates so both lists can refresh
        Response.Headers["HX-Trigger"] = "{\"mentorship:cancelled\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";

        // Return empty content so hx-swap=\"outerHTML\" removes the card from the DOM
        return Content(string.Empty);
    }
}