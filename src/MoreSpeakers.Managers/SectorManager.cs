// File: MoreSpeakers.Managers/SectorManager.cs
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class SectorManager : ISectorManager
{
    private readonly ISectorDataStore _dataStore;
    private readonly ILogger<SectorManager> _logger;

    public SectorManager(ISectorDataStore dataStore, ILogger<SectorManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }

    public Task<Sector?> GetAsync(int id) => _dataStore.GetAsync(id);
    public Task<List<Sector>> GetAllAsync(bool onlyActive = true) => _dataStore.GetAllAsync(onlyActive);
    public Task<Sector> SaveAsync(Sector sector) => _dataStore.SaveAsync(sector);
    public Task<bool> DeleteAsync(int id) => _dataStore.DeleteAsync(id);
    public Task<bool> DeleteAsync(Sector sector) => _dataStore.DeleteAsync(sector);
}