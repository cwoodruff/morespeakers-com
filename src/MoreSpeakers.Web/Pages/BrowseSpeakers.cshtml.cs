using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages;

public class BrowseSpeakersModel : PageModel
{
    private const int PageSize = 12;
    private readonly IExpertiseService _expertiseService;
    private readonly ISpeakerService _speakerService;

    public BrowseSpeakersModel(ISpeakerService speakerService, IExpertiseService expertiseService)
    {
        _speakerService = speakerService;
        _expertiseService = expertiseService;
    }

    public IEnumerable<User> Speakers { get; set; } = new List<User>();
    public IEnumerable<Expertise> AllExpertise { get; set; } = new List<Expertise>();

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)] public int? SpeakerTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)] public int? ExpertiseFilter { get; set; }

    [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "name";

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
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
        // Load all expertise for filter dropdown
        AllExpertise = await _expertiseService.GetAllExpertiseAsync();

        // Start with all speakers
        var newSpeakers = await _speakerService.GetNewSpeakersAsync();
        IEnumerable<User> experiencedSpeakers;
        // Apply expertise filter
		if (ExpertiseFilter.HasValue)
        {
	        experiencedSpeakers = await _speakerService.GetSpeakersByExpertiseAsync(ExpertiseFilter.Value);
        }
		else
		{
			experiencedSpeakers = await _speakerService.GetExperiencedSpeakersAsync();
		}

		IEnumerable<User> allSpeakers = newSpeakers.Concat(experiencedSpeakers);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
			allSpeakers = await _speakerService.SearchSpeakersAsync(SearchTerm, SpeakerTypeFilter);
        }

        // Apply speaker type filter
        if (SpeakerTypeFilter.HasValue)
        {
            allSpeakers = SpeakerTypeFilter == 1
                ? allSpeakers.Where(s => s.SpeakerTypeId == 1)
                : allSpeakers.Where(s => s.SpeakerTypeId == 2);
        }

        // Apply sorting
        allSpeakers = SortBy switch
        {
            "newest" => allSpeakers.OrderByDescending(s => s.CreatedDate),
            "expertise" => allSpeakers.OrderByDescending(s => s.UserExpertise.Count),
            _ => allSpeakers.OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
        };

        // Calculate pagination
        TotalCount = allSpeakers.Count();
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
        CurrentPage = Math.Max(1, Math.Min(CurrentPage, TotalPages == 0 ? 1 : TotalPages));

        // Apply pagination
        Speakers = allSpeakers
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
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
