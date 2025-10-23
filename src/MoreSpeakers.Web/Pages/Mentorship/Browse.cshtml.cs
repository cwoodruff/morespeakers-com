using System.Text.Encodings.Web;
using Microsoft.ApplicationInsights;
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
public class BrowseModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMentorshipService _mentorshipService;
    private readonly Domain.Interfaces.IEmailSender _emailSender;
    private readonly TelemetryClient _telemetryClient;

    public BrowseModel(ApplicationDbContext context, UserManager<User> userManager,
        IMentorshipService mentorshipService, Domain.Interfaces.IEmailSender emailSender,
        TelemetryClient telemetryClient)
    {
        _context = context;
        _userManager = userManager;
        _mentorshipService = mentorshipService;
        _emailSender = emailSender;
        _telemetryClient = telemetryClient;
    }

    public BrowseMentorsViewModel ViewModel { get; set; } = null!;
    public MentorshipType MentorshipType { get; set; }
    public List<string> SelectedExpertise { get; set; } = new();
    public bool? AvailableNow { get; set; }
    public List<Expertise> AvailableExpertise { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        MentorshipType type = MentorshipType.NewToExperienced,
        string? expertise = null,
        bool? availableNow = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        MentorshipType = type;
        SelectedExpertise = expertise?.Split(',').ToList() ?? new List<string>();
        AvailableNow = availableNow;
        AvailableExpertise = await _context.Expertise.OrderBy(e => e.Name).ToListAsync();

        ViewModel = new BrowseMentorsViewModel
        {
            CurrentUser = currentUser,
            MentorshipType = type,
            SelectedExpertise = SelectedExpertise,
            AvailableNow = availableNow,
            AvailableExpertise = AvailableExpertise
        };

        // Load mentors based on filters
        await LoadMentors(ViewModel);

        return Page();
    }

    public async Task<IActionResult> OnGetRequestModalAsync(Guid mentorId, MentorshipType type)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var targetUser = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .FirstOrDefaultAsync(u => u.Id == mentorId);

        if (targetUser == null) return NotFound();

        // Get expertise areas for the target user
        var expertise = targetUser.UserExpertise
            .Select(ue => ue.Expertise)
            .OrderBy(e => e.Name)
            .ToList();

        var viewModel = new RequestMentorshipViewModel
        {
            TargetUser = targetUser,
            CurrentUser = currentUser,
            Type = type,
            AvailableExpertise = expertise
        };

        return Partial("_RequestModal", viewModel);
    }

    public async Task<IActionResult> OnGetSearchMentorsAsync(
        MentorshipType mentorshipType = MentorshipType.NewToExperienced,
        List<string>? expertise = null,
        bool? availability = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        // Update model properties
        MentorshipType = mentorshipType;
        SelectedExpertise = expertise ?? new List<string>();
        AvailableNow = availability;
        AvailableExpertise = await _context.Expertise.OrderBy(e => e.Name).ToListAsync();

        var viewModel = new BrowseMentorsViewModel
        {
            CurrentUser = currentUser,
            MentorshipType = mentorshipType,
            SelectedExpertise = SelectedExpertise,
            AvailableNow = availability,
            AvailableExpertise = AvailableExpertise
        };

        // Load mentors based on filters
        await LoadMentors(viewModel);

        return Partial("~/Views/Mentorship/_MentorResults.cshtml", viewModel);
    }

    public async Task<IActionResult> OnPostSubmitRequestAsync(Guid targetId, MentorshipType type,
        string? requestMessage, List<int>? focusAreaIds, string? preferredFrequency)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var targetMentorUser = await _userManager.FindByIdAsync(targetId.ToString());
        if (targetMentorUser == null) return Unauthorized();

        // Check if can request
        var canRequest = await _mentorshipService.CanRequestMentorshipAsync(currentUser.Id, targetId);
        if (!canRequest)
        {
            return Content(
                "<div class='alert alert-warning'>You already have a pending or active connection with this person.</div>");
        }

        var mentorship = await _mentorshipService.RequestMentorshipWithDetailsAsync(
            currentUser.Id, targetId, type, requestMessage, focusAreaIds, preferredFrequency);

        if (mentorship == null)
        {
            return Content("<div class='alert alert-danger'>Failed to send request. Please try again.</div>");
        }

        //var requestUrl = Url.Page("/Mentorship/Requests/");

        await _emailSender.QueueEmail(
            new System.Net.Mail.MailAddress(targetMentorUser.Email!,
                $"{targetMentorUser.FirstName} {targetMentorUser.LastName}"),
            "You have a new MoreSpeaker.com Mentorship Request",
            $"Please review and confirm the mentoring request at MoreSpeakers.com.");

        _telemetryClient.TrackEvent("MentorRequestEmailSent", new Dictionary<string, string>
        {
            { "UserId", targetMentorUser.Id.ToString() },
            { "Email", targetMentorUser.Email ?? string.Empty }
        });

        return Partial("_RequestSuccess", mentorship);
    }

    private async Task LoadMentors(BrowseMentorsViewModel model)
    {
        var query = _context.Users
            .Include(u => u.SpeakerType)
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

        // Filter by expertise - users must have ALL selected expertise areas
        if (model.SelectedExpertise.Any())
        {
            var expertiseIds = await _context.Expertise
                .Where(e => model.SelectedExpertise.Contains(e.Name))
                .Select(e => e.Id)
                .ToListAsync();

            // User must have all selected expertise areas
            foreach (var expertiseId in expertiseIds)
            {
                query = query.Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId));
            }
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
}