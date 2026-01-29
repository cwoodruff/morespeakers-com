using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Managers;

public partial class GitHubService
{
    [LoggerMessage(LogLevel.Error, "Error getting GitHub contributors from {RepoOwner}/{RepoName}")]
    partial void LogErrorGettingGithubContributors(Exception exception, string repoOwner, string repoName);
}