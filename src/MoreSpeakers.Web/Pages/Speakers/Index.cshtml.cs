using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Speakers;

public partial class IndexModel : PageModel
{
    private const int PageSize = 12;
    private readonly IExpertiseManager _expertiseManager;
    private readonly ISectorManager _sectorManager;
    private readonly IUserManager _userManager;
    private readonly IMentoringManager _mentoringManager;
    private readonly ITemplatedEmailSender _templatedEmailSender;
    private readonly IRazorPartialToStringRenderer _partialRenderer;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IExpertiseManager expertiseManager, ISectorManager sectorManager, IUserManager userManager,
        IMentoringManager mentoringManager, ITemplatedEmailSender templatedEmailSender,
        IRazorPartialToStringRenderer partialRenderer, ILogger<IndexModel> logger)
    {
        _expertiseManager = expertiseManager;
        _sectorManager = sectorManager;
        _userManager = userManager;
        _mentoringManager = mentoringManager;
        _templatedEmailSender = templatedEmailSender;
        _partialRenderer = partialRenderer;
        _logger = logger;
    }

    public IEnumerable<User> Speakers { get; set; } = [];
    public IEnumerable<Expertise> AllExpertise { get; set; } = [];
    public IEnumerable<Sector> Sectors { get; set; } = [];
    public IEnumerable<ExpertiseCategory> Categories { get; set; } = [];

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)] public int? SpeakerTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)] public List<int>? ExpertiseFilter { get; set; }

    [BindProperty(SupportsGet = true)] public int? SectorFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryFilter { get; set; }

    [BindProperty(SupportsGet = true)] public SpeakerSearchOrderBy SortBy { get; set; } = SpeakerSearchOrderBy.Name;

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)] public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public int TotalPages { get; set; }

    [BindProperty(SupportsGet = true)] public SpeakerResultsViewType ViewType { get; set; } = SpeakerResultsViewType.CardView;

    [BindProperty(SupportsGet = true)] public SearchResultCountViewModel SearchResultsCount { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Load all active sectors for filter dropdown
            Sectors = await _sectorManager.GetAllAsync();

            if (SectorFilter.HasValue)
            {
                Categories = await _expertiseManager.GetAllActiveCategoriesForSector(SectorFilter.Value);
            }

            if (CategoryFilter.HasValue)
            {
                AllExpertise = await _expertiseManager.GetByCategoryIdAsync(CategoryFilter.Value);
            }
            else
            {
                // Load all active expertise for filter dropdown by default or if no category is selected
                AllExpertise = await _expertiseManager.GetAllExpertisesAsync();
            }

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
                    !string.IsNullOrEmpty(SearchTerm) || (ExpertiseFilter != null && ExpertiseFilter.Count != 0) || SpeakerTypeFilter.HasValue || SectorFilter.HasValue || CategoryFilter.HasValue,
                TotalResults = searchResults.RowCount
            };
            SearchResultsCount = searchResultsModel;

            // Check if this is an HTMX request for just the speakers container
            if (Request.Headers.ContainsKey("HX-Request") && !Request.Headers.ContainsKey("HX-Target-Select"))
            {
                var searchResultContainerHtml =
                    await _partialRenderer.RenderPartialToStringAsync(
                        "~/Pages/Speakers/_SearchResultCountPartial.cshtml", SearchResultsCount);
                var speakerContainerHtml =
                    await _partialRenderer.RenderPartialToStringAsync("~/Pages/Speakers/_SpeakerResults.cshtml", new SpeakerResultsViewModel
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = TotalPages,
                        Speakers = Speakers,
                        SearchTerm = SearchTerm,
                        SpeakerTypeFilter = SpeakerTypeFilter,
                        ExpertiseFilter = ExpertiseFilter,
                        ViewType = ViewType,
                        SortBy = SortBy
                    });

                return Content(searchResultContainerHtml + speakerContainerHtml, "text/html");
            }

        }
        catch (Exception ex)
        {
            LogErrorLoadingSpeakers(ex);
        }

        return Page();
    }

    public async Task<IActionResult> OnGetCategoriesAsync(int sectorFilter)
    {
        IEnumerable<ExpertiseCategory> categories;
        IEnumerable<Expertise> expertises;

        if (sectorFilter > 0)
        {
            categories = await _expertiseManager.GetAllActiveCategoriesForSector(sectorFilter);
            expertises = await _expertiseManager.GetBySectorIdAsync(sectorFilter);
        }
        else
        {
            categories = await _expertiseManager.GetAllCategoriesAsync();
            expertises = await _expertiseManager.GetAllExpertisesAsync();
        }

        var categoryOptionsHtml = await _partialRenderer.RenderPartialToStringAsync("~/Pages/Speakers/_CategoryOptions.cshtml", categories);
        var expertiseCheckboxesHtml = await _partialRenderer.RenderPartialToStringAsync("~/Pages/Speakers/_ExpertiseCheckboxes.cshtml", expertises);

        // Wrap expertise checkboxes with OOB swap
        var expertiseOobHtml = $"<div id=\"expertise-list-container bg-body\" hx-swap-oob=\"true\">{expertiseCheckboxesHtml}</div>";

        return Content(categoryOptionsHtml + expertiseOobHtml, "text/html");
    }

    public async Task<IActionResult> OnGetExpertisesAsync(int categoryFilter, int sectorFilter)
    {
        return Partial("_ExpertiseCheckboxes",
            categoryFilter > 0
                ? await _expertiseManager.GetByCategoryIdAsync(categoryFilter)
                : sectorFilter > 0
                    ? await _expertiseManager.GetBySectorIdAsync(sectorFilter)
                    : await _expertiseManager.GetAllExpertisesAsync());
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

            return Partial("_RequestModal", viewModel);
        }
        catch (Exception ex)
        {
            LogErrorLoadingMentorRequestModel(ex, currentUser?.Id);
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
            LogErrorSendingMentorshipRequest(ex, currentUser?.Id);
            throw;
        }

        // Send emails to both mentee and mentor
        var emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestFromMentee.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipRequested,
            "Your mentorship request was sent", mentorship.Mentee, mentorship);
        if (!emailSent)
        {
            LogFailedToSendMentorshipRequestEmailToMentee();
            // TODO: Create a visual indicator that the email was not sent
        }

        emailSent = await _templatedEmailSender.SendTemplatedEmail("~/EmailTemplates/MentorshipRequestToMentor.cshtml",
            Domain.Constants.TelemetryEvents.EmailGenerated.MentorshipRequested,
            "A mentorship was requested", mentorship.Mentor, mentorship);
        if (!emailSent)
        {
            LogFailedToSendMentorshipRequestEmailToMentor();
            // TODO: Create a visual indicator that the email was not sent
        }

        return Partial("_RequestSuccess", mentorship);

    }

}