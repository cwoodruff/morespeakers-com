using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IExpertiseDataStore _expertiseDataStore;
    private readonly IMentoringManager _mentoringManager;
    private readonly IUserDataStore _userDataStore;

    public IndexModel(
        IExpertiseDataStore expertiseDataStore,
        IMentoringManager mentoringManager,
        IUserDataStore userDataStore
        )
    {
        _expertiseDataStore = expertiseDataStore;
        _mentoringManager = mentoringManager;      
        _userDataStore = userDataStore;   
    }

    public int NewSpeakersCount { get; set; }
    public int ExperiencedSpeakersCount { get; set; }
    public int ActiveMentorshipsCount { get; set; }
    public IEnumerable<User> FeaturedSpeakers { get; set; } = new List<User>();
    public IEnumerable<Expertise> PopularExpertise { get; set; } = new List<Expertise>();

    public async Task OnGetAsync()
    {
        // Get statistics
        var stats = await _userDataStore.GetStatisticsForApplicationAsync();
        
        NewSpeakersCount = stats.newSpeakers;
        ExperiencedSpeakersCount = stats.experiencedSpeakers;
        ActiveMentorshipsCount = stats.activeMentorships;

        // Get featured speakers (experienced speakers with profiles)
        FeaturedSpeakers = await _userDataStore.GetFeaturedSpeakersAsync(6);

        // Get popular expertise areas
        PopularExpertise = await _expertiseDataStore.GetPopularExpertiseAsync(8);
    }
}