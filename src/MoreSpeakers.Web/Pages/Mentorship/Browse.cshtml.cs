using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Mentorship;

[Authorize]
public class BrowseModel : PageModel
{
    private readonly IUserManager _userManager;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IMentoringManager _mentoringManager;
    private readonly ITemplatedEmailSender _templatedEmailSender;
    private readonly ILogger<BrowseModel> _logger;

    public BrowseModel(
        IUserManager userManager,
        IExpertiseManager expertiseManager,
        IMentoringManager mentorshipService,
        ITemplatedEmailSender templatedEmailSender,
        ILogger<BrowseModel> logger)
    {
        _userManager = userManager;
        _expertiseManager = expertiseManager;       
        _mentoringManager = mentorshipService;
        _templatedEmailSender = templatedEmailSender;
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
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            MentorshipType = type;
            SelectedExpertise = expertise?.Split(',').ToList() ?? [];
            AvailableNow = availableNow;
            AvailableExpertise = await _expertiseManager.GetAllExpertisesAsync();

            ViewModel = new BrowseMentorsViewModel
            {
                CurrentUser = currentUser,
                MentorshipType = type,
                SelectedExpertise = SelectedExpertise,
                AvailableNow = availableNow,
                AvailableExpertise = AvailableExpertise
            };

            // Load mentors based on filters
            ViewModel.Mentors = await _mentoringManager.GetMentorsExceptForUserAsync(ViewModel.CurrentUser.Id, ViewModel.MentorshipType,
                ViewModel.SelectedExpertise, ViewModel.AvailableNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading mentorship requests for user '{User}'", currentUser?.Id);       
        }

        return Page();
    }

    public async Task<IActionResult> OnGetRequestModalAsync(Guid mentorId, MentorshipType type)
    {
        User? currentUser = null;
        
        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

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

            return Partial("~/Pages/Shared/_RequestModal.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading mentor request modal for user '{User}'", currentUser?.Id);
            throw;
        }
    }

    public async Task<IActionResult> OnGetSearchMentorsAsync(
        MentorshipType mentorshipType = MentorshipType.NewToExperienced,
        List<string>? expertise = null,
        bool? availability = null)
    {
        
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Update model properties
            MentorshipType = mentorshipType;
            SelectedExpertise = expertise ?? new List<string>();
            AvailableNow = availability;
            AvailableExpertise = await _expertiseManager.GetAllExpertisesAsync();
            
            var mentors = await _mentoringManager.GetMentorsExceptForUserAsync(currentUser.Id, mentorshipType,
                SelectedExpertise, AvailableNow);
            
            var viewModel = new SpeakerResultsViewModel()
            {
                CurrentUser = currentUser,
                SearchType = SearchType.Mentors,
                Speakers = mentors
            };

            return Partial("_SpeakerResults", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading mentorship requests for user '{User}'", currentUser?.Id);
            throw;       
        }
    }

    public async Task<IActionResult> OnPostSubmitRequestAsync(Guid targetId, MentorshipType type,
        string? requestMessage, List<int>? selectedExpertiseIds, string? preferredFrequency)
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

            // Check if the user can request
            var canRequest = await _mentoringManager.CanRequestMentorshipAsync(currentUser.Id, targetId);
            if (!canRequest)
            {
                return Partial("~/Pages/Shared/_AlertDialog.cshtml",
                    new AlertDialogViewModel
                    {
                        AlertType = AlertTypeEnum.Warning,
                        Message = "You already have a pending or active connection with this person."
                    });
            }

            mentorship = await _mentoringManager.RequestMentorshipWithDetailsAsync(
                currentUser.Id, targetId, type, requestMessage, selectedExpertiseIds, preferredFrequency);

            if (mentorship == null)
            {
                return Partial("~/Pages/Shared/_AlertDialog.cshtml",
                    new AlertDialogViewModel
                    {
                        AlertType = AlertTypeEnum.Danger,
                        Message = "Failed to send request. Please try again."
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending mentorship request for user '{User}'", currentUser?.Id);
            throw;
        }
        
        // Send emails to both mentee and mentor
        var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestFromMentee.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipRequested,
            "Your mentorship request was sent", mentorship.Mentee, mentorship);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship request email to mentee");
            // TODO: Create a visual indicator that the email was not sent
        }

        emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestToMentor.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipRequested,
            "A mentorship was requested", mentorship.Mentor, mentorship);
        if (!emailSent)
        {
            _logger.LogError("Failed to send mentorship request email to mentor");
            // TODO: Create a visual indicator that the email was not sent
        }

        return Partial("_RequestSuccess", mentorship);

    }

    public async Task<IActionResult> OnPostCancelRequestAsync(Guid mentorshipId)
    {
        User? currentUser = null;

        try
        {
            currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var mentorshipCancelled =
                await _mentoringManager.CancelMentorshipRequestAsync(mentorshipId, currentUser.Id);

            if (!mentorshipCancelled)
            {
                return NotFound();
            }

            return Partial("~/Pages/Shared/_AlertDialog.cshtml",
                new AlertDialogViewModel
                {
                    AlertType = AlertTypeEnum.Info,
                    Message = "Request cancelled successfully."
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling mentorship request for user '{User}'", currentUser?.Id);
            return Partial("~/Pages/Shared/_AlertDialog.cshtml",
                new AlertDialogViewModel
                {
                    AlertType = AlertTypeEnum.Danger,
                    Message = "An error happened while cancelling the request."
                });
        }
    }
}