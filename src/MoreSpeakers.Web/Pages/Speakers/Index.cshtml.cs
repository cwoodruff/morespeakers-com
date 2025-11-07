using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Speakers;

public class IndexModel : PageModel
{
    private const int PageSize = 12;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IUserManager _userManager;
    private readonly IRazorPartialToStringRenderer _partialRenderer;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IExpertiseManager expertiseManager, IUserManager userManager, IRazorPartialToStringRenderer partialRenderer, ILogger<IndexModel> logger)
    {
        _expertiseManager = expertiseManager;
        _userManager = userManager;
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
    
    [BindProperty(SupportsGet = true)] public Models.ViewModels.SearchResultCountViewModel SearchResultsCount { get; set; } = new Models.ViewModels.SearchResultCountViewModel();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Load all expertise for filter dropdown
            AllExpertise = await _expertiseManager.GetAllAsync();

            // Search for the speakers
            var searchResults = await _userManager.SearchSpeakersAsync(SearchTerm, SpeakerTypeFilter, ExpertiseFilter,
                SortBy, CurrentPage, PageSize);

            TotalCount = searchResults.RowCount;
            TotalPages = searchResults.TotalPages;
            CurrentPage = searchResults.CurrentPage;
            Speakers = searchResults.Speakers;

            var searchResultsModel = new Models.ViewModels.SearchResultCountViewModel
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
                    await _partialRenderer.RenderPartialToStringAsync(HttpContext,
                        "~/Pages/Speakers/_SearchResultCountPartial.cshtml", SearchResultsCount);
                var speakerContainerHtml =
                    await _partialRenderer.RenderPartialToStringAsync(HttpContext, "_SpeakersContainer", this);

                return Content(searchResultContainerHtml + speakerContainerHtml, "text/html");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading speakers");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRequestMentorshipAsync(Guid mentorId)
    {
        if (!User.Identity?.IsAuthenticated == true)
            return new JsonResult(new { error = "Please log in to request mentorship." });

        // TODO: Implement Request Membership logic here
        
        try
        {
            // Get current user ID (you'll need to implement this)
            var currentUserId = await _userManager.GetUserIdAsync(User);
            if (currentUserId == null)
            {
                return new JsonResult(new { error = "Unable to identify current user." });
            }

            // Here you would call your mentorship service
            // var success = await _mentorshipService.RequestMentorshipAsync(currentUserId, mentorId);

            // For now, return success (implement actual logic)
            return new JsonResult(new { success = "Mentorship request sent successfully!" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = "An error occurred while sending the mentorship request." });
        }
    }
}
