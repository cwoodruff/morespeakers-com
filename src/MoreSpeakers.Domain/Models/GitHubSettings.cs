namespace MoreSpeakers.Domain.Models;

public class GitHubSettings
{
    public string RepoOwner { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public string CacheKey { get; set; } = "GitHubContributors";
    public int CacheDurationInMinutes { get; set; } = 60;
}