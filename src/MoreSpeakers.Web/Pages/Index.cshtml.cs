using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IExpertiseManager _expertiseManager;
    private readonly IUserManager _userManager;
    
    public IndexModel(
        IExpertiseManager expertiseManager,
        IUserManager userManager
        )
    {
        _expertiseManager = expertiseManager;
        _userManager = userManager;
    }

    public int NewSpeakersCount { get; set; }
    public int ExperiencedSpeakersCount { get; set; }
    public int ActiveMentorshipsCount { get; set; }
    public IEnumerable<User> FeaturedSpeakers { get; set; } = new List<User>();
    public IEnumerable<Expertise> PopularExpertise { get; set; } = new List<Expertise>();

    public async Task OnGetAsync()
    {
        // Get statistics
        var stats = await _userManager.GetStatisticsForApplicationAsync();
        
        NewSpeakersCount = stats.newSpeakers;
        ExperiencedSpeakersCount = stats.experiencedSpeakers;
        ActiveMentorshipsCount = stats.activeMentorships;

        // Get featured speakers (experienced speakers with profiles)
        FeaturedSpeakers = await _userManager.GetFeaturedSpeakersAsync(6);

        // Get popular expertise areas
        PopularExpertise = await _expertiseManager.GetPopularExpertiseAsync(8);
    }
}