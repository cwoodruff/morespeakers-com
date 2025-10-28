using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IExpertiseDataStore _expertiseDataStore;
    private readonly IMentorshipService _mentorshipService;
    private readonly ISpeakerDataStore _speakerDataStore;

    public IndexModel(
        IExpertiseDataStore expertiseDataStore,
        IMentorshipService mentorshipService,
        ISpeakerDataStore speakerDataStore
        )
    {
        _expertiseDataStore = expertiseDataStore;
        _mentorshipService = mentorshipService;
        _speakerDataStore = speakerDataStore;   
    }

    public int NewSpeakersCount { get; set; }
    public int ExperiencedSpeakersCount { get; set; }
    public int ActiveMentorshipsCount { get; set; }
    public IEnumerable<User> FeaturedSpeakers { get; set; } = new List<User>();
    public IEnumerable<Expertise> PopularExpertise { get; set; } = new List<Expertise>();

    public async Task OnGetAsync()
    {
        // Get statistics
        var newSpeakers = await _speakerDataStore.GetNewSpeakersAsync();
        var experiencedSpeakers = await _speakerDataStore.GetExperiencedSpeakersAsync();
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
        PopularExpertise = await _expertiseDataStore.GetPopularExpertiseAsync(8);
    }
}