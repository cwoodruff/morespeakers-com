using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Speakers;

public class IndexModel : PageModel
{
    private const int PageSize = 12;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IUserManager _userManager;
    private readonly IMentoringManager _mentoringManager;
    private readonly ITemplatedEmailSender _templatedEmailSender;
    private readonly IRazorPartialToStringRenderer _partialRenderer;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IExpertiseManager expertiseManager, IUserManager userManager, 
        IMentoringManager mentoringManager, ITemplatedEmailSender templatedEmailSender,
        IRazorPartialToStringRenderer partialRenderer, ILogger<IndexModel> logger)
    {
        _expertiseManager = expertiseManager;
        _userManager = userManager;
        _mentoringManager = mentoringManager;
        _templatedEmailSender = templatedEmailSender;      
        _partialRenderer = partialRenderer;       
        _logger = logger;
    }

    public IEnumerable<User> Speakers { get; set; } = new List<User>();
    public IEnumerable<Expertise> AllExpertise { get; set; } = new List<Expertise>();

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)] public int? SpeakerTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)] public int? ExpertiseFilter { get; set; }

    [BindProperty(SupportsGet = true)] public SpeakerSearchOrderBy SortBy { get; set; } = SpeakerSearchOrderBy.Name;

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)] public int TotalCount { get; set; }
    
    [BindProperty(SupportsGet = true)] public int TotalPages { get; set; }
    
    [BindProperty(SupportsGet = true)] public SearchResultCountViewModel SearchResultsCount { get; set; } = new Models.ViewModels.SearchResultCountViewModel();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Load all active expertise for filter dropdown
            AllExpertise = await _expertiseManager.GetAllExpertisesAsync();

            // Search for the speakers
            var searchResults = await _userManager.SearchSpeakersAsync(SearchTerm, SpeakerTypeFilter, ExpertiseFilter,
                SortBy, CurrentPage, PageSize);

            TotalCount = searchResults.RowCount;
            TotalPages = searchResults.TotalPages;
            CurrentPage = searchResults.CurrentPage;
            Speakers = searchResults.Speakers;

            var searchResultsModel = new SearchResultCountViewModel
            {
                AreFiltersApplied =
                    !string.IsNullOrEmpty(SearchTerm) || ExpertiseFilter.HasValue || SpeakerTypeFilter.HasValue,
                TotalResults = searchResults.RowCount
            };
            SearchResultsCount = searchResultsModel;

            // Check if this is an HTMX request for just the speakers container
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                var searchResultContainerHtml =
                    await _partialRenderer.RenderPartialToStringAsync(
                        "~/Pages/Speakers/_SearchResultCountPartial.cshtml", SearchResultsCount);
                var speakerContainerHtml =
                    await _partialRenderer.RenderPartialToStringAsync("_SpeakerResults", new SpeakerResultsViewModel
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = TotalPages,
                        SearchType = SearchType.Speakers,
                        Speakers = Speakers
                    });

                return Content(searchResultContainerHtml + speakerContainerHtml, "text/html");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading speakers");
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

            // Check if can request
            var canRequest = await _mentoringManager.CanRequestMentorshipAsync(currentUser.Id, targetId);
            if (!canRequest)
            {
                return Content(
                    "<div class='alert alert-warning'>You already have a pending or active connection with this person.</div>");
            }

            mentorship = await _mentoringManager.RequestMentorshipWithDetailsAsync(
                currentUser.Id, targetId, type, requestMessage, selectedExpertiseIds, preferredFrequency);

            if (mentorship == null)
            {
                return Content("<div class='alert alert-danger'>Failed to send request. Please try again.</div>");
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
}
