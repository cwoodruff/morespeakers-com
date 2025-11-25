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
    private readonly ITemplatedEmailSender _templatedEmailSender;
    private readonly IRazorPartialToStringRenderer _partialRenderer;
    private readonly ILogger<RequestsModel> _logger;

    public RequestsModel(
        IMentoringManager mentoringManager,
        IUserManager userManager,
        ITemplatedEmailSender  templatedEmailSender,
        IRazorPartialToStringRenderer partialRenderer,       
        ILogger<RequestsModel> logger       
        )
    {
        _mentoringManager = mentoringManager;       
        _userManager = userManager;
        _templatedEmailSender = templatedEmailSender;          
        _partialRenderer = partialRenderer;      
        _logger = logger;
    }

    public List<Domain.Models.Mentorship> IncomingRequests { get; set; } = [];
    public List<Domain.Models.Mentorship> OutgoingRequests { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            IncomingRequests = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id);
            OutgoingRequests = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading mentorship requests for user '{User}'", User.Identity?.Name);       
        }

        return Page();
    }

    public async Task<IActionResult> OnGetDeclineModalAsync(Guid mentorshipId)
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var mentorship = await _mentoringManager.GetAsync(mentorshipId);
            if (mentorship.MentorId != currentUser.Id)
            {
                return NotFound();
            }

            var viewModel = new DeclineMentorshipViewModel { Mentorship = mentorship };
            return Partial("_DeclineModal", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading mentorship request for user '{User}'", currentUser?.Id);
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid mentorshipId)
    {
        User? currentUser = null;
        Domain.Models.Mentorship? mentorship;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            mentorship =
                await _mentoringManager.RespondToRequestAsync(mentorshipId, currentUser.Id, true, string.Empty);

            // Send emails to both mentee and mentor
            if (mentorship == null)
            {
                _logger.LogError("Could not find mentorship with ID {MentorshipId}", mentorshipId);
                return BadRequest();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting mentorship request for user '{User}'", currentUser?.Id);
            return BadRequest();
        }

        var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestAcceptedFromMentee.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipAccepted,
            "Your mentorship request was accepted", mentorship.Mentee, mentorship
            );
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship accepted email to mentee");
            // TODO: Create a visual indicator that the email was not sent
        }

        emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestAcceptedToMentor.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipAccepted,
            "A mentorship was accepted", mentorship.Mentor, mentorship
            );
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship accepted email to mentor");
            // TODO: Create a visual indicator that the email was not sent
        }

        // Notify the client via HTMX events so both lists can refresh and any listeners can react
        Response.Headers["HX-Trigger"] = "{\"mentorship:accepted\":{\"id\":\"" + mentorshipId +
                                         "\"},\"mentorship:updated\":true}";
        // Return an OOB toast (partial renders as OOB) and remove the card by swapping empty content
        return Partial("_AcceptSuccess", mentorship);
    }

    public async Task<IActionResult> OnPostDeclineAsync(Guid mentorshipId, string? declineReason)
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Unauthorized(); }

            var mentorship =
                await _mentoringManager.RespondToRequestAsync(mentorshipId, currentUser.Id, false, declineReason);

            // Send emails to both mentee and mentor
            if (mentorship == null)
            {
                _logger.LogError("Could not find mentorship with ID {MentorshipId}", mentorshipId);
                return BadRequest();
            }

            var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestDeclinedFromMentee.cshtml",
                Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipDeclined,
                "Your mentorship request was declined", mentorship.Mentee, mentorship
                );
            if (!emailSent)
            {
                _logger.LogError("Failed to send mentorship declined email to mentee");
                // TODO: Create a visual indicator that the email was not sent
            }

            emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestDeclinedToMentor.cshtml",
                Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipDeclined,
                "A mentorship request was declined", mentorship.Mentor, mentorship
                );
            if (!emailSent)
            {
                _logger.LogError("Failed to send mentorship declined email to mentor");
                // TODO: Create a visual indicator that the email was not sent
            }

            Response.Headers["HX-Trigger"] = "{\"mentorship:declined\":{\"id\":\"" + mentorshipId +
                                             "\"},\"mentorship:updated\":true}";
            return Partial("_DeclineSuccess", mentorship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declining mentorship request for user '{User}'", currentUser?.Id);
            return BadRequest();
        }
    }

    public async Task<IActionResult> OnGetNotificationCountAsync()
    {
        
        User? user = null;

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Content(string.Empty);

            var (outbound, inbound) = await _mentoringManager.GetNumberOfMentorshipsPending(currentUser.Id);
            
            var html = string.Empty;

            // Incoming requests: Require user attention (Red badge)
            if (inbound > 0)
            {
                html += $"<span class='badge bg-danger ms-1' title='{inbound} Incoming Request(s)'><i class='bi bi-inbox-fill me-1'></i>{inbound}</span>";
            }
            
            // Outgoing requests: Awaiting external response (Gray badge)
            if (outbound > 0)
            {
                html += $"<span class='badge bg-secondary ms-1' title='{outbound} Outgoing Request(s)'><i class='bi bi-send-fill me-1'></i>{outbound}</span>";
            }

            return Content(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notification count for user '{User}'", user?.Id);       
        }

        return Content(string.Empty);
    }

    public async Task<IActionResult> OnGetPollIncomingAsync()
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            IncomingRequests = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id);

            if (IncomingRequests.Count == 0)
            {
                return Partial("_IncomingRequests_NoRequestsFound.cshtml", null);
            }
            
            // Check if this is an HTMX request for just the speakers container
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return await SwapInboundAsync();
            }
            
            // Load the inbound notifications to update the header count
            return Partial("_IncomingRequests", IncomingRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling for the incoming request for user '{User}'", currentUser?.Id);
            return BadRequest();
        }
    }

    public async Task<IActionResult> OnGetPollOutgoingAsync()
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            OutgoingRequests = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);

            if (OutgoingRequests.Count == 0)
            {
                return Partial("_OutgoingRequests_NoRequestsFound.cshtml", null);
            }

            // Check if this is an HTMX request for just the speakers container
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                var outboundHeaderContainerHtml =
                    await _partialRenderer.RenderPartialToStringAsync(
                        "~/Pages/Mentorship/_RequestsNotificationBadge.cshtml",
                        new RequestNotificationBadgeViewModel()
                        {
                            Count = OutgoingRequests.Count,
                            NotificationDirection = RequestNotificationDirection.Outbound
                        });
                var outboundContainerHtml =
                    await _partialRenderer.RenderPartialToStringAsync("~/Pages/Mentorship/_OutgoingRequests.cshtml", OutgoingRequests);

                return Content(outboundHeaderContainerHtml + outboundContainerHtml, "text/html");
            }
            
            return Partial("_OutgoingRequests", OutgoingRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling for the outbound request for user '{User}'", currentUser?.Id);
            return BadRequest();
        }
    }

    
    private async Task<IActionResult> SwapInboundAsync()
    {
        var inboundHeaderContainerHtml =
            await _partialRenderer.RenderPartialToStringAsync(
                "~/Pages/Mentorship/_RequestsNotificationBadge.cshtml",
                new RequestNotificationBadgeViewModel()
                {
                    Count = IncomingRequests.Count,
                    NotificationDirection = RequestNotificationDirection.Inbound
                });
        var inboundContainerHtml =
            await _partialRenderer.RenderPartialToStringAsync("~/Pages/Mentorship/_OutgoingRequests.cshtml", IncomingRequests);

        return Content(inboundHeaderContainerHtml + inboundContainerHtml, "text/html");
    }

    public async Task<IActionResult> OnPostCancelRequestAsync(Guid mentorshipId)
    {
        User? currentUser = null;
        Domain.Models.Mentorship? mentorship;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Since we need to send an email to the mentee and mentor, load the mentorship record first since
            //  cancel will delete the record.
            mentorship = await _mentoringManager.GetMentorshipWithRelationships(mentorshipId);
            if (mentorship == null)
            {
                _logger.LogError("Could not find mentorship with ID {MentorshipId}", mentorshipId);
                return BadRequest();
            }
            
            // Ensure the current user is the mentee who created the request
            var wasCanceled = await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);
            if (!wasCanceled)
            {
                return BadRequest();
            }
            // Refresh the outgoing requests list
            OutgoingRequests = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling mentorship request for user '{User}'", currentUser?.Id);
            return BadRequest();
        }

        // Send emails to both mentee and mentor
        var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestCancelledFromMentee.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipCancelled,
            "Your mentorship request was cancelled", mentorship.Mentee, mentorship);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship cancelled email to mentee");
            // TODO: Create a visual indicator that the email was not sent
        }

        emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestCancelledToMentor.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipCancelled,
            "A mentorship request was cancelled", mentorship.Mentor, mentorship);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship cancelled email to mentor");
            // TODO: Create a visual indicator that the email was not sent
        }

        // Update the header count and swap the container        
        var outboundHeaderContainerHtml =
            await _partialRenderer.RenderPartialToStringAsync(
                "~/Pages/Mentorship/_RequestsNotificationBadge.cshtml",
                new RequestNotificationBadgeViewModel()
                {
                    Count = OutgoingRequests.Count,
                    NotificationDirection = RequestNotificationDirection.Outbound   
                });
        var outboundContainerHtml =
            await _partialRenderer.RenderPartialToStringAsync("~/Pages/Mentorship/_OutgoingRequests.cshtml",
                OutgoingRequests);
        
        return Content(outboundHeaderContainerHtml + outboundContainerHtml, "text/html");       
    }
}