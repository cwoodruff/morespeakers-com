using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.DTOs;

namespace MoreSpeakers.Web.Pages;

public class AboutModel : PageModel
{
    private readonly IGitHubService _gitHubService;

    public AboutModel(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    public IEnumerable<GitHubContributor> Contributors { get; set; } = new List<GitHubContributor>();

    public async Task OnGet()
    {
        Contributors = await _gitHubService.GetContributorsAsync();
    }
}