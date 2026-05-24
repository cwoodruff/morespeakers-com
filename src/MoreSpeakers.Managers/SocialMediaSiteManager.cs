using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class SocialMediaSiteManager: ISocialMediaSiteManager
{
    private readonly ISocialMediaSiteDataStore _dataStore;
    private readonly ILogger<SocialMediaSiteManager> _logger;
    private readonly TelemetryClient _telemetryClient;

    public SocialMediaSiteManager(ISocialMediaSiteDataStore dataStore, ILogger<SocialMediaSiteManager> logger, TelemetryClient telemetryClient)
    {
        _dataStore = dataStore;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public Task<Result<SocialMediaSite>> GetAsync(int primaryKey) => _dataStore.GetAsync(primaryKey);

    public Task<Result> DeleteAsync(int primaryKey) => _dataStore.DeleteAsync(primaryKey);

    public async Task<Result<SocialMediaSite>> SaveAsync(SocialMediaSite entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            return Result.Failure<SocialMediaSite>(new Error("social-media-site.validation.name-required", "Social media site name is required."));
        }

        return await _dataStore.SaveAsync(entity);
    }

    public Task<Result<List<SocialMediaSite>>> GetAllAsync() => _dataStore.GetAllAsync();

    public Task<Result> DeleteAsync(SocialMediaSite entity) => _dataStore.DeleteAsync(entity);

    public Task<Result<int>> RefCountAsync(int primaryKey) => _dataStore.RefCountAsync(primaryKey);

    public Task<Result<bool>> InUseAsync(int primaryKey) => _dataStore.InUseAsync(primaryKey);
}