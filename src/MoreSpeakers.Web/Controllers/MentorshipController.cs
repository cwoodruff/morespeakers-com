using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;

using MoreSpeakers.Web.Models.ViewModels;

namespace MoreSpeakers.Web.Controllers;

[Authorize]
[Route("Mentorship")]
public class MentorshipController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IMentoringManager _mentoringManager;
    private readonly ISpeakerManager _speakerManager;
    
    public MentorshipController(
        UserManager<User> userManager,
        IExpertiseManager expertiseManager,
        IMentoringManager mentoringManager,
        ISpeakerManager speakerManager
        )
    {
        _userManager = userManager;
        _expertiseManager = expertiseManager;
        _mentoringManager = mentoringManager;
        _speakerManager = speakerManager;
    }

    [HttpGet("Browse")]
    public async Task<IActionResult> Browse(MentorshipType type = MentorshipType.NewToExperienced, 
        string? expertise = null, bool? availableNow = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var model = new BrowseMentorsViewModel
        {
            CurrentUser = currentUser,
            MentorshipType = type,
            SelectedExpertise = expertise?.Split(',').ToList() ?? new List<string>(),
            AvailableNow = availableNow,
            AvailableExpertise = await _expertiseManager.GetAllAsync()
        };

        // Load mentors based on filters
        await LoadMentors(model);

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return PartialView("_MentorResults", model);
        }

        return View(model);
    }

    [HttpGet("SearchMentors")]
    public async Task<IActionResult> SearchMentors([FromQuery] MentorSearchFilters filters)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var model = new BrowseMentorsViewModel
        {
            CurrentUser = currentUser,
            MentorshipType = filters.Type,
            SelectedExpertise = filters.Expertise?.Split(',').ToList() ?? new List<string>(),
            AvailableNow = filters.AvailableNow,
            AvailableExpertise = await _expertiseManager.GetAllAsync()
        };

        await LoadMentors(model);
        return PartialView("_MentorResults", model);
    }

    [HttpGet("RequestModal/{mentorId:guid}")]
    public async Task<IActionResult> RequestModal(Guid mentorId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentor = await _speakerManager.GetAsync(mentorId);

        // Get shared expertise between current user and mentor
        var sharedExpertises = await _mentoringManager.GetSharedExpertisesAsync(mentor, currentUser);

        var model = new MentorshipRequestViewModel
        {
            Mentor = mentor,
            AvailableExpertise = sharedExpertises,
            MentorshipType = currentUser.IsNewSpeaker ? 
                MentorshipType.NewToExperienced : 
                MentorshipType.ExperiencedToExperienced
        };

        return PartialView("_RequestModal", model);
    }

    [HttpPost("SendRequest/{mentorId:guid}")]
    public async Task<IActionResult> SendRequest(Guid mentorId, MentorshipRequestModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentor = await _speakerManager.GetAsync(mentorId);

        // Check if request already exists
        var existingRequest = await _mentoringManager.DoesMentorshipRequestsExistsAsync(mentor, currentUser);

        if (existingRequest)
        {
            return Json(new { success = false, message = "You already have a pending request with this mentor." });
        }

        var mentorship = new Mentorship
        {
            MentorId = mentorId,
            MenteeId = currentUser.Id,
            Type = currentUser.IsNewSpeaker ? 
                MentorshipType.NewToExperienced : 
                MentorshipType.ExperiencedToExperienced,
            RequestMessage = model.RequestMessage,
            PreferredFrequency = model.PreferredFrequency,
            Status = MentorshipStatus.Pending
        };

        await _mentoringManager.CreateMentorshipRequestAsync(mentorship, model.SelectedExpertiseIds);

        return Json(new { success = true, message = "Your mentorship request has been sent!" });
    }

    [HttpGet("Requests")]
    public async Task<IActionResult> Requests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var model = new MentorshipRequestsViewModel
        {
            IncomingRequests = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id),
            OutgoingRequests = await _mentoringManager.GetOutgoingMentorshipRequests(currentUser.Id)
        };

        return View(model);
    }

    [HttpPost("RespondToRequest/{requestId:guid}")]
    public async Task<IActionResult> RespondToRequest(Guid requestId, bool accept, string? responseMessage = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _mentoringManager.RespondToRequestAsync(requestId, currentUser.Id, accept, responseMessage);
        
        if (mentorship == null) return NotFound();

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return PartialView("_RequestCard", mentorship);
        }

        return RedirectToAction("Requests");
    }

    [HttpGet("Active")]
    public async Task<IActionResult> Active()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var activeMentorships = await _mentoringManager.GetActiveMentorshipsForUserAsync(currentUser.Id);

        return View(activeMentorships);
    }

    [HttpGet("CloseModal")]
    public IActionResult CloseModal()
    {
        return Content("");
    }

    [HttpGet("NotificationCount")]
    public async Task<IActionResult> NotificationCount()
    {
        // TODO: Change this to include pending inbound and outbound requests
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Json(new { count = 0 });

        var pendingCount = await _mentoringManager.GetNumberOfMentorshipsPending(currentUser.Id);

        if (pendingCount.inboundCount > 0)
        {
            return PartialView("_NotificationBadge", pendingCount.inboundCount);
        }

        return Content("");
    }

    [HttpGet("PollRequests")]
    public async Task<IActionResult> PollRequests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var incomingRequests = await _mentoringManager.GetIncomingMentorshipRequests(currentUser.Id);
        return PartialView("_IncomingRequests", incomingRequests);
    }

    [HttpPost("CancelRequest/{requestId:guid}")]
    public async Task<IActionResult> CancelRequest(Guid requestId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorshipCancelled = await _mentoringManager.CancelMentorshipRequestAsync(requestId, currentUser.Id);

        if (!mentorshipCancelled) return NotFound();

        return Content("<div class='alert alert-info'>Request cancelled successfully.</div>");
    }

    [HttpPost("CompleteMentorship/{mentorshipId:guid}")]
    public async Task<IActionResult> CompleteMentorship(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorCompleted = await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);

        if (!mentorCompleted) return NotFound();

        return Content("<div class='alert alert-success'>Mentorship marked as completed!</div>");
    }

    [HttpPost("CancelMentorship/{mentorshipId:guid}")]
    public async Task<IActionResult> CancelMentorship(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorshipCancelled = await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);

        if (!mentorshipCancelled) return NotFound();

        return Content("<div class='alert alert-warning'>Mentorship cancelled.</div>");
    }

    private async Task LoadMentors(BrowseMentorsViewModel model)
    {
        var mentors = await _mentoringManager.GetMentorsExceptForUserAsync(model.CurrentUser.Id, model.MentorshipType,
            model.SelectedExpertise, model.AvailableNow);
        model.Mentors = mentors;
    }
}