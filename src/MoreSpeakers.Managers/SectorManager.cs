// File: MoreSpeakers.Managers/SectorManager.cs
using Microsoft.Extensions.Logging;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

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
    public async Task<Sector?> GetSectorWithRelationshipsAsync(int id) => await _dataStore.GetSectorWithRelationshipsAsync(id);
    
    /// <summary>
    /// Gets all sectors, including inactive ones.
    /// </summary>
    /// <returns>A List of <see cref="Sector" /></returns>
    /// <remarks>If you want to filter the list of sectors, use <see cref="GetAllSectorsAsync" /> instead.</remarks>
    public Task<List<Sector>> GetAllAsync() => _dataStore.GetAllAsync();

    /// <summary>
    /// Gets all sectors, optionally including inactive ones, or both.
    /// </summary>
    /// <param name="active">A <see cref="TriState" /> value indicating whether to include active sectors, inactive sectors, or both.</param>
    /// <param name="searchTerm">A search term to filter sectors by name.</param>
    /// <param name="includeCategories">Whether to include related expertise categories in the result.</param>
    /// <returns>A List of <see cref="Sector" /></returns>
    /// <remarks>If you want to get the list of all sectors regardless of IsActive flag, you can use <see cref="GetAllAsync" /> instead.</remarks>
    public Task<List<Sector>> GetAllSectorsAsync(TriState active = TriState.True, string? searchTerm = "",
        bool includeCategories = false) => _dataStore.GetAllSectorsAsync(active, searchTerm, includeCategories);
    
    public Task<Sector> SaveAsync(Sector sector) => _dataStore.SaveAsync(sector);
    public Task<bool> DeleteAsync(int id) => _dataStore.DeleteAsync(id);
    public Task<bool> DeleteAsync(Sector sector) => _dataStore.DeleteAsync(sector);
}