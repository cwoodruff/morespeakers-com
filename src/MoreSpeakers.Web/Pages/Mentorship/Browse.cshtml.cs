using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

using MoreSpeakers.Web.Models.ViewModels;

namespace MoreSpeakers.Web.Pages.Mentorship;

[Authorize]
public class BrowseModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IMentoringManager _mentoringManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<BrowseModel> _logger;
    private readonly TelemetryClient _telemetryClient;

    public BrowseModel(
        IUserManager userManager,
        IExpertiseManager expertiseManager,
        IMentoringManager mentorshipService,
        IEmailSender emailSender,
        TelemetryClient telemetryClient, ILogger<BrowseModel> logger)
    {
        _userManager = userManager;
        _expertiseManager = expertiseManager;       
        _mentoringManager = mentorshipService;
        _emailSender = emailSender;
        _telemetryClient = telemetryClient;
        _logger = logger;
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
        AvailableExpertise = await _expertiseManager.GetAllAsync();

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

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var targetUser = await _mentoringManager.GetMentorAsync(mentorId);

            if (targetUser == null) return NotFound();

            // Get expertise areas for the target user
            var expertise = targetUser.UserExpertise
                .Select(ue => ue.Expertise)
                .OrderBy(e => e.Name)
                .ToList();

            var viewModel = new MentorshipRequestViewModel
            {
                Mentor = targetUser,
                Mentee = currentUser,
                MentorshipType = type,
                AvailableExpertise = expertise
            };

            return Partial("/Views/Mentorship/_RequestModal.cshtml", viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading mentor request modal");
            Console.WriteLine(e);
            throw;
        }
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
        AvailableExpertise = await _expertiseManager.GetAllAsync();

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
        string? requestMessage, List<int>? selectedExpertiseIds, string? preferredFrequency)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var targetMentorUser = await _userManager.GetAsync(targetId);

        // Check if can request
        var canRequest = await _mentoringManager.CanRequestMentorshipAsync(currentUser.Id, targetId);
        if (!canRequest)
        {
            return Content(
                "<div class='alert alert-warning'>You already have a pending or active connection with this person.</div>");
        }

        var mentorship = await _mentoringManager.RequestMentorshipWithDetailsAsync(
            currentUser.Id, targetId, type, requestMessage, selectedExpertiseIds, preferredFrequency);

        if (mentorship == null)
        {
            return Content("<div class='alert alert-danger'>Failed to send request. Please try again.</div>");
        }

        //var requestUrl = Url.Page("/Mentorship/Requests/");

        await _emailSender.QueueEmail(
            new System.Net.Mail.MailAddress(targetMentorUser.Email,
                $"{targetMentorUser.FirstName} {targetMentorUser.LastName}"),
            "You have a new MoreSpeaker.com Mentorship Request",
            $"Please review and confirm the mentoring request at MoreSpeakers.com.");

        _telemetryClient.TrackEvent("MentorRequestEmailSent", new Dictionary<string, string>
        {
            { "UserId", targetMentorUser.Id.ToString() },
            { "Email", targetMentorUser.Email }
        });

        return Partial("_RequestSuccess", mentorship);
    }

    private async Task LoadMentors(BrowseMentorsViewModel model)
    {
        var mentors = await _mentoringManager.GetMentorsExceptForUserAsync(model.CurrentUser.Id, model.MentorshipType,
            model.SelectedExpertise, model.AvailableNow);
        model.Mentors = mentors;
    }
}