using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IExpertiseService _expertiseService;
    private readonly IMentorshipService _mentorshipService;
    private readonly ISpeakerService _speakerService;

    public IndexModel(
        ISpeakerService speakerService,
        IMentorshipService mentorshipService,
        IExpertiseService expertiseService)
    {
        _speakerService = speakerService;
        _mentorshipService = mentorshipService;
        _expertiseService = expertiseService;
    }

    public int NewSpeakersCount { get; set; }
    public int ExperiencedSpeakersCount { get; set; }
    public int ActiveMentorshipsCount { get; set; }
    public IEnumerable<User> FeaturedSpeakers { get; set; } = new List<User>();
    public IEnumerable<Expertise> PopularExpertise { get; set; } = new List<Expertise>();

    public async Task OnGetAsync()
    {
        // Get statistics
        var newSpeakers = await _speakerService.GetNewSpeakersAsync();
        var experiencedSpeakers = await _speakerService.GetExperiencedSpeakersAsync();
        var activeMentorships = await _mentorshipService.GetActiveMentorshipsAsync();

        NewSpeakersCount = newSpeakers.Count();
        ExperiencedSpeakersCount = experiencedSpeakers.Count();
        ActiveMentorshipsCount = activeMentorships.Count();

        // Get featured speakers (experienced speakers with profiles)
        FeaturedSpeakers = experiencedSpeakers
            .Where(s => !string.IsNullOrEmpty(s.Bio) && s.UserExpertise.Any())
            .OrderByDescending(s => s.UserExpertise.Count)
            .Take(6)
            .ToList();

        // Get popular expertise areas
        PopularExpertise = await _expertiseService.GetPopularExpertiseAsync(8);
    }
}