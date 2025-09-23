using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using morespeakers.Models;
using morespeakers.Services;

namespace morespeakers.Pages;

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

    public async Task OnGetAsync()
    {
        // Load all expertise for filter dropdown
        AllExpertise = await _expertiseService.GetAllExpertiseAsync();

        // Get filtered speakers
        IEnumerable<User> allSpeakers;

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            allSpeakers = await _speakerService.SearchSpeakersAsync(SearchTerm, SpeakerTypeFilter);
        }
        else if (SpeakerTypeFilter.HasValue)
        {
            allSpeakers = SpeakerTypeFilter == 1
                ? await _speakerService.GetNewSpeakersAsync()
                : await _speakerService.GetExperiencedSpeakersAsync();
        }
        else if (ExpertiseFilter.HasValue)
        {
            allSpeakers = await _speakerService.GetSpeakersByExpertiseAsync(ExpertiseFilter.Value);
        }
        else
        {
            var newSpeakers = await _speakerService.GetNewSpeakersAsync();
            var experiencedSpeakers = await _speakerService.GetExperiencedSpeakersAsync();
            allSpeakers = newSpeakers.Concat(experiencedSpeakers);
        }

        // Apply expertise filter if specified and not already applied
        if (ExpertiseFilter.HasValue && string.IsNullOrWhiteSpace(SearchTerm) && !SpeakerTypeFilter.HasValue)
        {
            // Already filtered by expertise above
        }
        else if (ExpertiseFilter.HasValue)
        {
            allSpeakers = allSpeakers.Where(s => s.UserExpertise.Any(ue => ue.ExpertiseId == ExpertiseFilter.Value));
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
        CurrentPage = Math.Max(1, Math.Min(CurrentPage, TotalPages));

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