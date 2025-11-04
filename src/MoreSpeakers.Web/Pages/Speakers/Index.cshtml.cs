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

    public async Task<IActionResult> OnGetAsync()
    {
        // Load all expertise for filter dropdown
        AllExpertise = await _expertiseManager.GetAllAsync();
        
        // Search for the speakers
        var searchResults = await _userManager.SearchSpeakersAsync(SearchTerm, SpeakerTypeFilter, ExpertiseFilter, SortBy, CurrentPage, PageSize);

        TotalCount = searchResults.RowCount;
        TotalPages = searchResults.TotalPages;
        CurrentPage = searchResults.CurrentPage;
        Speakers = searchResults.Speakers;

        // Check if this is an HTMX request for just the speakers container
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var html = new StringWriter();
            await html.WriteAsync(await RazorPartialToString.RenderPartialViewToString(HttpContext, "_SpeakersContainer", this));
            await html.WriteAsync(await RazorPartialToString.RenderPartialViewToString(HttpContext, "_SearchResultCountPartial", searchResults.RowCount));
            
            return Content(html.ToString(), "text/html");

            //return Partial("_SpeakersContainer", this);
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
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty) return new JsonResult(new { error = "Unable to identify current user." });

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

    private Guid GetCurrentUserId()
    {
        // Implement logic to get current user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)) return userId;
        return Guid.Empty;
    }
}
