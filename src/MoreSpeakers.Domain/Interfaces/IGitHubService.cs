using MoreSpeakers.Domain.Models.DTOs;

namespace MoreSpeakers.Domain.Interfaces;

public interface IGitHubService
{
    Task<IEnumerable<GitHubContributor>> GetContributorsAsync();
}