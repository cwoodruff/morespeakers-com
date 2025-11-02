using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages.Speakers;

public class IndexModel : PageModel
{
    private const int PageSize = 12;
    private readonly IExpertiseManager _expertiseManager;
    private readonly IUserManager _userManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IExpertiseManager expertiseManager, IUserManager userManager, ILogger<IndexModel> logger)
    {
        _expertiseManager = expertiseManager;
        _userManager = userManager;
        _logger = logger;
    }

    public IEnumerable<User> Speakers { get; set; } = new List<User>();
    public IEnumerable<Expertise> AllExpertise { get; set; } = new List<Expertise>();

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)] public int? SpeakerTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)] public int? ExpertiseFilter { get; set; }

    [BindProperty(SupportsGet = true)] public SpeakerSearchOrderBy SortBy { get; set; } = SpeakerSearchOrderBy.Name;

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Load all expertise for filter dropdown
        AllExpertise = await _expertiseManager.GetAllAsync();
        
        await LoadSpeakersAsync();

        // Check if this is an HTMX request for just the speakers container
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return Partial("_SpeakersContainer", this);
        }

        return Page();
    }
    
    private async Task<IActionResult> OnGetBrowseSpeakersAsync()
    {
        // Load all expertise for filter dropdown
        AllExpertise = await _expertiseManager.GetAllAsync();
        
        await LoadSpeakersAsync();
        
        // Check if this is an HTMX request for just the speakers container
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return Partial("_SpeakersContainer", this);
        }

        return Page();
    }

    private async Task LoadSpeakersAsync()
    {
        var searchResults = await _userManager.SearchSpeakersAsync(SearchTerm, SpeakerTypeFilter, ExpertiseFilter, SortBy, CurrentPage, PageSize);

        TotalCount = searchResults.RowCount;
        TotalPages = searchResults.TotalPages;
        CurrentPage = searchResults.CurrentPage;
        Speakers = searchResults.Speakers;
    }

    public async Task<IActionResult> OnPostRequestMentorshipAsync(Guid mentorId)
    {
        if (!User.Identity?.IsAuthenticated == true)
            return new JsonResult(new { error = "Please log in to request mentorship." });

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
