using System.Net.Http.Json;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.DTOs;

namespace MoreSpeakers.Managers;

public partial class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ISettings _settings;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(HttpClient httpClient, IMemoryCache cache, ISettings settings, ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _settings = settings;
        _logger = logger;
    }

    public async Task<IEnumerable<GitHubContributor>> GetContributorsAsync()
    {
        var cacheKey = _settings.GitHub.CacheKey;
        if (_cache.TryGetValue(cacheKey, out IEnumerable<GitHubContributor>? contributors))
        {
            return contributors ?? [];
        }

        try
        {
            // GitHub API requires a User-Agent header
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("MoreSpeakers-App");

            var url = $"https://api.github.com/repos/{_settings.GitHub.RepoOwner}/{_settings.GitHub.RepoName}/contributors";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            contributors = await response.Content.ReadFromJsonAsync<IEnumerable<GitHubContributor>>();

            if (contributors != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.GitHub.CacheDurationInMinutes));

                _cache.Set(cacheKey, contributors, cacheEntryOptions);
                return contributors;
            }
        }
        catch (Exception ex)
        {
            LogErrorGettingGithubContributors(ex, _settings.GitHub.RepoOwner, _settings.GitHub.RepoName);
        }

        return [];
    }
}