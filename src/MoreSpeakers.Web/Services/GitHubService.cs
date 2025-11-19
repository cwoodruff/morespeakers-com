using Microsoft.Extensions.Caching.Memory;
using MoreSpeakers.Web.Models.DTOs;
using System.Text.Json;

namespace MoreSpeakers.Web.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GitHubService> _logger;
    private const string CacheKey = "GitHubContributors";
    // Based on repository context: cwoodruff/morespeakers-com
    private const string RepoOwner = "cwoodruff";
    private const string RepoName = "morespeakers-com";

    public GitHubService(HttpClient httpClient, IMemoryCache cache, ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;

        // GitHub API requires a User-Agent header
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MoreSpeakers.com-Web");
    }

    public async Task<IEnumerable<GitHubContributor>> GetContributorsAsync()
    {
        if (_cache.TryGetValue(CacheKey, out IEnumerable<GitHubContributor>? cachedContributors))
        {
            return cachedContributors ?? Enumerable.Empty<GitHubContributor>();
        }

        try
        {
            // Using the public GitHub API
            var response = await _httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/contributors");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var contributors = JsonSerializer.Deserialize<IEnumerable<GitHubContributor>>(json);

                if (contributors != null)
                {
                    // Cache for 1 hour to avoid rate limiting
                    _cache.Set(CacheKey, contributors, TimeSpan.FromHours(1));
                    return contributors;
                }
            }
            else
            {
                _logger.LogWarning("GitHub API returned status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contributors from GitHub");
        }

        return Enumerable.Empty<GitHubContributor>();
    }
}