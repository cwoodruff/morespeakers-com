using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Mentorship;


[Authorize]
public class RequestsModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly IMentoringManager _mentoringManager;
    private readonly IEmailSender _emailSender;
    private readonly IRazorPartialToStringRenderer _stringRenderer;
    private readonly ILogger<RequestsModel> _logger;
    private readonly TelemetryClient _telemetryClient;

    public RequestsModel(
        IMentoringManager mentoringManager,
        IUserManager userManager,
        IEmailSender emailSender,
        IRazorPartialToStringRenderer stringRenderer,
        ILogger<RequestsModel> logger,
        TelemetryClient telemetryClient       
        )
    {
        _mentoringManager = mentoringManager;       
        _userManager = userManager;
        _emailSender = emailSender;
        _stringRenderer = stringRenderer;       
        _logger = logger;
        _telemetryClient = telemetryClient;       
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
        
        // Send emails to both mentee and mentor
        if (mentorship == null)
        {
            _logger.LogError("Could not find mentorship with ID {MentorshipId}", mentorshipId);
            return BadRequest();
        }

        var emailSent = await SendEmail("~/EmailTemplates/MentorshipRequestAccepted-FromMentee.cshtml", "Your mentorship request was accepted", mentorship.Mentee, mentorship, Domain.Constants.TelemetryEvents.MentorshipAccepted);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship accepted email to mentee");
            // TODO: Create a visual indicator that the email was not sent
        }
        emailSent = await SendEmail("~/EmailTemplates/MentorshipRequestAccepted-ToMentor.cshtml", "A mentorship was accepted", mentorship.Mentor, mentorship, Domain.Constants.TelemetryEvents.MentorshipAccepted);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship accepted email to mentor");
            // TODO: Create a visual indicator that the email was not sent
        }
        
        // Notify the client via HTMX events so both lists can refresh and any listeners can react
        Response.Headers["HX-Trigger"] = "{\"mentorship:accepted\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";
        // Return an OOB toast (partial renders as OOB) and remove the card by swapping empty content
        return Partial("_AcceptSuccess", mentorship);
    }

    public async Task<IActionResult> OnPostDeclineAsync(Guid mentorshipId, string? declineReason)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _mentoringManager.RespondToRequestAsync(mentorshipId, currentUser.Id, false, declineReason);
        
        // Send emails to both mentee and mentor
        if (mentorship == null)
        {
            _logger.LogError("Could not find mentorship with ID {MentorshipId}", mentorshipId);
            return BadRequest();
        }

        var emailSent = await SendEmail("~/EmailTemplates/MentorshipRequestDeclined-FromMentee.cshtml", "Your mentorship request was cancelled", mentorship.Mentee, mentorship, Domain.Constants.TelemetryEvents.MentorshipDeclined);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship declined email to mentee");
            // TODO: Create a visual indicator that the email was not sent
        }
        emailSent = await SendEmail("~/EmailTemplates/MentorshipRequestDeclined-ToMentor.cshtml", "A mentorship request was cancelled", mentorship.Mentor, mentorship, Domain.Constants.TelemetryEvents.MentorshipDeclined);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship declined email to mentor");
            // TODO: Create a visual indicator that the email was not sent
        }
        
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
        
        // Send emails to both mentee and mentor
        var mentorship = await _mentoringManager.GetMentorshipWithRelationships(mentorshipId);
        if (mentorship == null)
        {
            _logger.LogError("Could not find mentorship with ID {MentorshipId}", mentorshipId);
            return BadRequest();
        }

        var emailSent = await SendEmail("~/EmailTemplates/MentorshipRequestCancelled-FromMentee.cshtml", "Your mentorship request was cancelled", mentorship.Mentee, mentorship, Domain.Constants.TelemetryEvents.MentorshipCancelled);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship cancelled email to mentee");
            // TODO: Create a visual indicator that the email was not sent
        }
        emailSent = await SendEmail("~/EmailTemplates/MentorshipRequestCancelled-ToMentor.cshtml", "A mentorship request was cancelled", mentorship.Mentor, mentorship, Domain.Constants.TelemetryEvents.MentorshipCancelled);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship cancelled email to mentor");
            // TODO: Create a visual indicator that the email was not sent
        }
        
        // Trigger updates so both lists can refresh
        Response.Headers["HX-Trigger"] = "{\"mentorship:cancelled\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";

        // Return empty content so hx-swap=\"outerHTML\" removes the card from the DOM
        return Content(string.Empty);
    }

    private async Task<bool> SendEmail(string emailTemplate, string subject, User toUser, Domain.Models.Mentorship mentorship, string eventName)
    {

        if (string.IsNullOrWhiteSpace(emailTemplate))
        {
            throw new ArgumentException("Email template cannot be null or whitespace", nameof(emailTemplate));
        }
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Email subject cannot be null or whitespace", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name cannot be null or whitespace", nameof(eventName));
        }
        
        if (mentorship == null)
        {
            throw new ArgumentException("Mentorship cannot be null", nameof(mentorship));       
        }

        try
        {
            var emailBody = await _stringRenderer.RenderPartialToStringAsync(HttpContext, "~/EmailTemplates/WelcomeEmail.cshtml", mentorship);
            await _emailSender.QueueEmail(new System.Net.Mail.MailAddress(toUser.Email!, $"{toUser.FirstName} {toUser.LastName}"),
                subject, emailBody);

            _telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                { "UserId", toUser.Id.ToString() },
                { "Email", toUser.Email! }
            });
            _logger.LogInformation("{EventName} email was successfully sent to {Email}", eventName, toUser.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {EventName} email to {Email}", eventName, toUser.Email);
            return false;
        }

        return true;
    }
}