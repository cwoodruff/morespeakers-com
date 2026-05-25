using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Models.DTOs;

namespace MoreSpeakers.Domain.Interfaces;

public interface IGitHubService
{
    Task<Result<IEnumerable<GitHubContributor>>> GetContributorsAsync();
}