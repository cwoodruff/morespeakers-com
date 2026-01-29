using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Pages;

public partial class IndexModel : PageModel
{
    private readonly IExpertiseManager _expertiseManager;
    private readonly IUserManager _userManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IExpertiseManager expertiseManager,
        IUserManager userManager,
        ILogger<IndexModel> logger
        )
    {
        _expertiseManager = expertiseManager;
        _userManager = userManager;
        _logger = logger;
    }

    public int NewSpeakersCount { get; set; }
    public int ExperiencedSpeakersCount { get; set; }
    public int ActiveMentorshipsCount { get; set; }
    public IEnumerable<User> FeaturedSpeakers { get; set; } = [];
    public IEnumerable<Expertise> PopularExpertise { get; set; } = [];

    public async Task OnGetAsync()
    {
        try
        {
            // Get statistics
            (int newSpeakers, int experiencedSpeakers, int activeMentorships)
                = await _userManager.GetStatisticsForApplicationAsync();

            NewSpeakersCount = newSpeakers;
            ExperiencedSpeakersCount = experiencedSpeakers;
            ActiveMentorshipsCount = activeMentorships;

            // Get featured speakers (experienced speakers with profiles)
            FeaturedSpeakers = await _userManager.GetFeaturedSpeakersAsync(3);

            // Get popular expertise areas
            PopularExpertise = await _expertiseManager.GetPopularExpertiseAsync(8);
        }
        catch (Exception ex)
        {
            LogErrorLoadingIndexPage(ex);
            // TODO: Show toast?
            throw;
        }
    }
}