using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;
using morespeakers.Models.ViewModels;
using morespeakers.Services;

namespace morespeakers.Controllers;

[Authorize]
[Route("Mentorship")]
public class MentorshipController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ISpeakerService _speakerService;

    public MentorshipController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ISpeakerService speakerService)
    {
        _context = context;
        _userManager = userManager;
        _speakerService = speakerService;
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
            AvailableExpertise = await _context.Expertise.OrderBy(e => e.Name).ToListAsync()
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
            AvailableExpertise = await _context.Expertise.OrderBy(e => e.Name).ToListAsync()
        };

        await LoadMentors(model);
        return PartialView("_MentorResults", model);
    }

    [HttpGet("RequestModal/{mentorId:guid}")]
    public async Task<IActionResult> RequestModal(Guid mentorId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentor = await _context.Users
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .FirstOrDefaultAsync(u => u.Id == mentorId);

        if (mentor == null) return NotFound();

        // Get shared expertise between current user and mentor
        var currentUserExpertise = await _context.UserExpertise
            .Where(ue => ue.UserId == currentUser.Id)
            .Select(ue => ue.ExpertiseId)
            .ToListAsync();

        var sharedExpertise = mentor.UserExpertise
            .Where(ue => currentUserExpertise.Contains(ue.ExpertiseId))
            .Select(ue => ue.Expertise)
            .ToList();

        var model = new MentorshipRequestViewModel
        {
            Mentor = mentor,
            SharedExpertise = sharedExpertise,
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

        var mentor = await _context.Users.FindAsync(mentorId);
        if (mentor == null) return NotFound();

        // Check if request already exists
        var existingRequest = await _context.Mentorships
            .FirstOrDefaultAsync(m => m.MentorId == mentorId && 
                                    m.MenteeId == currentUser.Id && 
                                    m.Status == MentorshipStatus.Pending);

        if (existingRequest != null)
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

        _context.Mentorships.Add(mentorship);
        await _context.SaveChangesAsync();

        // Add focus areas
        if (model.SelectedExpertiseIds?.Any() == true)
        {
            var focusAreas = model.SelectedExpertiseIds.Select((int expertiseId) => new MentorshipExpertise
            {
                MentorshipId = mentorship.Id,
                ExpertiseId = expertiseId
            });

            _context.MentorshipExpertise.AddRange(focusAreas);
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, message = "Your mentorship request has been sent!" });
    }

    [HttpGet("Requests")]
    public async Task<IActionResult> Requests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var model = new MentorshipRequestsViewModel
        {
            IncomingRequests = await GetIncomingRequests(currentUser.Id),
            OutgoingRequests = await GetOutgoingRequests(currentUser.Id)
        };

        return View(model);
    }

    [HttpPost("RespondToRequest/{requestId:guid}")]
    public async Task<IActionResult> RespondToRequest(Guid requestId, bool accept, string? responseMessage = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _context.Mentorships
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .FirstOrDefaultAsync(m => m.Id == requestId && m.MentorId == currentUser.Id);

        if (mentorship == null) return NotFound();

        mentorship.Status = accept ? MentorshipStatus.Active : MentorshipStatus.Declined;
        mentorship.ResponseMessage = responseMessage;
        mentorship.ResponsedAt = DateTime.UtcNow;
        mentorship.UpdatedAt = DateTime.UtcNow;

        if (accept)
        {
            mentorship.StartedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

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

        var activeMentorships = await _context.Mentorships
            .Include(m => m.Mentor)
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => (m.MentorId == currentUser.Id || m.MenteeId == currentUser.Id) && 
                       m.Status == MentorshipStatus.Active)
            .OrderBy(m => m.StartedAt)
            .ToListAsync();

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
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Json(new { count = 0 });

        var pendingCount = await _context.Mentorships
            .CountAsync(m => m.MentorId == currentUser.Id && m.Status == MentorshipStatus.Pending);

        if (pendingCount > 0)
        {
            return PartialView("_NotificationBadge", pendingCount);
        }

        return Content("");
    }

    [HttpGet("PollRequests")]
    public async Task<IActionResult> PollRequests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var incomingRequests = await GetIncomingRequests(currentUser.Id);
        return PartialView("_IncomingRequests", incomingRequests);
    }

    [HttpPost("CancelRequest/{requestId:guid}")]
    public async Task<IActionResult> CancelRequest(Guid requestId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _context.Mentorships
            .FirstOrDefaultAsync(m => m.Id == requestId && m.MenteeId == currentUser.Id);

        if (mentorship == null) return NotFound();

        mentorship.Status = MentorshipStatus.Cancelled;
        mentorship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Content("<div class='alert alert-info'>Request cancelled successfully.</div>");
    }

    [HttpPost("CompleteMentorship/{mentorshipId:guid}")]
    public async Task<IActionResult> CompleteMentorship(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _context.Mentorships
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && 
                               (m.MentorId == currentUser.Id || m.MenteeId == currentUser.Id));

        if (mentorship == null) return NotFound();

        mentorship.Status = MentorshipStatus.Completed;
        mentorship.CompletedAt = DateTime.UtcNow;
        mentorship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Content("<div class='alert alert-success'>Mentorship marked as completed!</div>");
    }

    [HttpPost("CancelMentorship/{mentorshipId:guid}")]
    public async Task<IActionResult> CancelMentorship(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _context.Mentorships
            .FirstOrDefaultAsync(m => m.Id == mentorshipId && 
                               (m.MentorId == currentUser.Id || m.MenteeId == currentUser.Id));

        if (mentorship == null) return NotFound();

        mentorship.Status = MentorshipStatus.Cancelled;
        mentorship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Content("<div class='alert alert-warning'>Mentorship cancelled.</div>");
    }

    private async Task LoadMentors(BrowseMentorsViewModel model)
    {
        var query = _context.Users
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.Id != model.CurrentUser.Id); // Exclude current user

        // Filter by speaker type based on mentorship type
        if (model.MentorshipType == MentorshipType.NewToExperienced)
        {
            query = query.Where(u => u.SpeakerTypeId == 2); // Experienced speakers only
        }
        else
        {
            query = query.Where(u => u.SpeakerTypeId == 2); // Both can mentor each other, but for now experienced only
        }

        // Filter by expertise
        if (model.SelectedExpertise.Any())
        {
            var expertiseIds = await _context.Expertise
                .Where(e => model.SelectedExpertise.Contains(e.Name))
                .Select(e => e.Id)
                .ToListAsync();

            query = query.Where(u => u.UserExpertise.Any(ue => expertiseIds.Contains(ue.ExpertiseId)));
        }

        // Filter by availability
        if (model.AvailableNow == true)
        {
            query = query.Where(u => u.IsAvailableForMentoring);
        }

        model.Mentors = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    private async Task<List<Mentorship>> GetIncomingRequests(Guid userId)
    {
        return await _context.Mentorships
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MentorId == userId && m.Status == MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    private async Task<List<Mentorship>> GetOutgoingRequests(Guid userId)
    {
        return await _context.Mentorships
            .Include(m => m.Mentor)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MenteeId == userId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }
}