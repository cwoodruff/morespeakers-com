using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

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

    public async Task<SocialMediaSite?> GetAsync(int primaryKey)
    {
        return await _dataStore.GetAsync(primaryKey);
    }

    public async Task<bool> DeleteAsync(int primaryKey)
    {
        return await _dataStore.DeleteAsync(primaryKey);
    }

    public async Task<SocialMediaSite> SaveAsync(SocialMediaSite entity)
    {
        return await _dataStore.SaveAsync(entity);
    }

    public async Task<List<SocialMediaSite>> GetAllAsync()
    {
        return await _dataStore.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(SocialMediaSite entity)
    {
        return await _dataStore.DeleteAsync(entity);
    }

    public async Task<int> RefCountAsync(int primaryKey)
    {
        return await _dataStore.RefCountAsync(primaryKey);
    }

    public async Task<bool> InUseAsync(int primaryKey)
    {
        return await _dataStore.InUseAsync(primaryKey);
    }
}