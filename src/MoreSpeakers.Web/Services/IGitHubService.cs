using MoreSpeakers.Web.Models.DTOs;

namespace MoreSpeakers.Web.Services;

public interface IGitHubService
{
    Task<IEnumerable<GitHubContributor>> GetContributorsAsync();
}