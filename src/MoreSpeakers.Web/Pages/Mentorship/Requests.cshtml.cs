using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Mentorship;

[Authorize]
public partial class RequestsModel : PageModel
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

            var incomingResult = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id);
            if (incomingResult.IsFailure)
            {
                _logger.LogWarning("Failed to load incoming mentorship requests for user {UserId}: {Error}", currentUser.Id, incomingResult.Error.Message);
                TempData["ErrorMessage"] = incomingResult.Error.Message;
                IncomingRequests = [];
            }
            else
            {
                IncomingRequests = incomingResult.Value;
            }

            var outgoingResult = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);
            if (outgoingResult.IsFailure)
            {
                _logger.LogWarning("Failed to load outgoing mentorship requests for user {UserId}: {Error}", currentUser.Id, outgoingResult.Error.Message);
                TempData["ErrorMessage"] = outgoingResult.Error.Message;
                OutgoingRequests = [];
            }
            else
            {
                OutgoingRequests = outgoingResult.Value;
            }
        }
        catch (Exception ex)
        {
            LogErrorLoadingMentorshipRequests(ex, User.Identity?.Name);
            IncomingRequests = [];
            OutgoingRequests = [];
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

            var mentorshipResult = await _mentoringManager.GetAsync(mentorshipId);
            if (mentorshipResult.IsFailure)
            {
                _logger.LogWarning("Failed to load mentorship {MentorshipId} for decline modal: {Error}", mentorshipId, mentorshipResult.Error.Message);
                TempData["ErrorMessage"] = mentorshipResult.Error.Message;
                return NotFound();
            }

            var mentorship = mentorshipResult.Value;
            if (mentorship.MentorId != currentUser.Id)
            {
                return NotFound();
            }

            var viewModel = new DeclineMentorshipViewModel { Mentorship = mentorship };
            return Partial("_DeclineModal", viewModel);
        }
        catch (Exception ex)
        {
            LogErrorLoadingMentorshipRequest(ex, currentUser?.Id);
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid mentorshipId)
    {
        User? currentUser = null;
        Domain.Models.Mentorship mentorship;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var mentorshipResult =
                await _mentoringManager.RespondToRequestAsync(mentorshipId, currentUser.Id, true, string.Empty);

            if (mentorshipResult.IsFailure)
            {
                _logger.LogWarning("Failed to accept mentorship request {MentorshipId} for user {UserId}: {Error}", mentorshipId, currentUser.Id, mentorshipResult.Error.Message);
                TempData["ErrorMessage"] = mentorshipResult.Error.Message;
                return BadRequest();
            }

            mentorship = mentorshipResult.Value;
        }
        catch (Exception ex)
        {
            LogErrorAcceptingMentorshipRequest(ex, currentUser?.Id);
            return BadRequest();
        }

        var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestAcceptedFromMentee.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipAccepted,
            "Your mentorship request was accepted", mentorship.Mentee, mentorship
            );
        if (!emailSent)
        {
            LogFailedToSendMentorshipAcceptedEmailToMentee();
            // TODO: Create a visual indicator that the email was not sent
        }

        emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestAcceptedToMentor.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipAccepted,
            "A mentorship was accepted", mentorship.Mentor, mentorship
            );
        if (!emailSent)
        {
            LogFailedToSendMentorshipAcceptedEmailToMentor();
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

            var mentorshipResult =
                await _mentoringManager.RespondToRequestAsync(mentorshipId, currentUser.Id, false, declineReason);

            if (mentorshipResult.IsFailure)
            {
                _logger.LogWarning("Failed to decline mentorship request {MentorshipId} for user {UserId}: {Error}", mentorshipId, currentUser.Id, mentorshipResult.Error.Message);
                TempData["ErrorMessage"] = mentorshipResult.Error.Message;
                return BadRequest();
            }

            var mentorship = mentorshipResult.Value;

            var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestDeclinedFromMentee.cshtml",
                Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipDeclined,
                "Your mentorship request was declined", mentorship.Mentee, mentorship
                );
            if (!emailSent)
            {
                LogFailedToSendMentorshipDeclinedEmailToMentee();
                // TODO: Create a visual indicator that the email was not sent
            }

            emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestDeclinedToMentor.cshtml",
                Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipDeclined,
                "A mentorship request was declined", mentorship.Mentor, mentorship
                );
            if (!emailSent)
            {
                LogFailedToSendMentorshipDeclinedEmailToMentor();
                // TODO: Create a visual indicator that the email was not sent
            }

            Response.Headers["HX-Trigger"] = "{\"mentorship:declined\":{\"id\":\"" + mentorshipId +
                                             "\"},\"mentorship:updated\":true}";
            return Partial("_DeclineSuccess", mentorship);
        }
        catch (Exception ex)
        {
            LogErrorDecliningMentorshipRequestForUser(ex, currentUser?.Id);
            return BadRequest();
        }
    }

    public async Task<IActionResult> OnGetNotificationCountAsync()
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Content(string.Empty);

            var pendingResult = await _mentoringManager.GetNumberOfMentorshipsPending(currentUser.Id);
            if (pendingResult.IsFailure)
            {
                _logger.LogWarning("Failed to load mentorship notification count for user {UserId}: {Error}", currentUser.Id, pendingResult.Error.Message);
                return Content(string.Empty);
            }

            var (outbound, inbound) = pendingResult.Value;

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
            LogErrorLoadingNotificationCountForUser(ex, currentUser?.Id);
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

            var incomingResult = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id);
            if (incomingResult.IsFailure)
            {
                _logger.LogWarning("Failed to poll incoming mentorship requests for user {UserId}: {Error}", currentUser.Id, incomingResult.Error.Message);
                IncomingRequests = [];
            }
            else
            {
                IncomingRequests = incomingResult.Value;
            }

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
            LogErrorPollingForTheIncomingRequest(ex, currentUser?.Id);
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

            var outgoingResult = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);
            if (outgoingResult.IsFailure)
            {
                _logger.LogWarning("Failed to poll outgoing mentorship requests for user {UserId}: {Error}", currentUser.Id, outgoingResult.Error.Message);
                OutgoingRequests = [];
            }
            else
            {
                OutgoingRequests = outgoingResult.Value;
            }

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
            LogErrorPollingForTheOutboundRequest(ex, currentUser?.Id);
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
        Domain.Models.Mentorship mentorship;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Since we need to send an email to the mentee and mentor, load the mentorship record first since
            //  cancel will delete the record.
            var mentorshipResult = await _mentoringManager.GetMentorshipWithRelationships(mentorshipId);
            if (mentorshipResult.IsFailure)
            {
                _logger.LogWarning("Failed to load mentorship {MentorshipId} before cancellation: {Error}", mentorshipId, mentorshipResult.Error.Message);
                TempData["ErrorMessage"] = mentorshipResult.Error.Message;
                return BadRequest();
            }

            mentorship = mentorshipResult.Value;

            var cancelResult = await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);
            if (cancelResult.IsFailure)
            {
                _logger.LogWarning("Failed to cancel mentorship request {MentorshipId} for user {UserId}: {Error}", mentorshipId, currentUser.Id, cancelResult.Error.Message);
                TempData["ErrorMessage"] = cancelResult.Error.Message;
                return BadRequest();
            }

            var outgoingResult = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id);
            if (outgoingResult.IsFailure)
            {
                _logger.LogWarning("Failed to reload outgoing mentorship requests for user {UserId}: {Error}", currentUser.Id, outgoingResult.Error.Message);
                TempData["ErrorMessage"] = outgoingResult.Error.Message;
                OutgoingRequests = [];
            }
            else
            {
                OutgoingRequests = outgoingResult.Value;
            }
        }
        catch (Exception ex)
        {
            LogErrorCancellingMentorshipRequest(ex, currentUser?.Id);
            return BadRequest();
        }

        // Send emails to both mentee and mentor
        var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestCancelledFromMentee.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipCancelled,
            "Your mentorship request was cancelled", mentorship.Mentee, mentorship);
        if (!emailSent)
        {
            LogFailedToSendMentorshipCancelledEmailToMentee();
            // TODO: Create a visual indicator that the email was not sent
        }

        emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestCancelledToMentor.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipCancelled,
            "A mentorship request was cancelled", mentorship.Mentor, mentorship);
        if (!emailSent)
        {
            LogFailedToSendMentorshipCancelledEmailToMentor();
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