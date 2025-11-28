using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public class ExpertiseManager: IExpertiseManager
{
    private readonly IExpertiseDataStore _dataStore;
    private readonly ILogger<ExpertiseManager> _logger;

    public ExpertiseManager(IExpertiseDataStore dataStore, ILogger<ExpertiseManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }

    public async Task<Expertise> GetAsync(int primaryKey)
    {
        return await _dataStore.GetAsync(primaryKey);
    }

    public async Task<bool> DeleteAsync(int primaryKey)
    {
        return await _dataStore.DeleteAsync(primaryKey);
    }

    public async Task<Expertise> SaveAsync(Expertise entity)
    {
        return await _dataStore.SaveAsync(entity);
    }

    public async Task<List<Expertise>> GetAllAsync()
    {
        return await _dataStore.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(Expertise entity)
    {
        return await _dataStore.DeleteAsync(entity);
    }

    public async Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm)
    {
        return await _dataStore.SearchExpertiseAsync(searchTerm);
    }

    public async Task<int> CreateExpertiseAsync(string name, string? description = null)
    {
        var expertise = new Expertise { Name = name, Description = description };
        
        try
        {
            await _dataStore.SaveAsync(expertise);
            return expertise.Id;  
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to create expertise '{Name}'", name);
            return 0;
        }
    }

    public async Task<bool> UpdateExpertiseAsync(int id, string name, string? description = null)
    {
        var expertise = new Expertise { Id = id, Name = name, Description = description };
        
        try
        {
            await _dataStore.SaveAsync(expertise);
            return true;
        }
        catch
        {
            _logger.LogError(
                "Failed to update expertise with the id of '{Id}'. Name:'{Name}', Description:'{Description}", id, name,
                description);
            return false;
        }
    }

    public async Task<Expertise?> SearchForExpertiseExistsAsync(string name)
    {
        return await _dataStore.SearchForExpertiseExistsAsync(name);   
    }

    public async Task<bool> DeleteExpertiseAsync(int id)
    {
        return await _dataStore.DeleteAsync(id);
    }

    public async Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10)
    {
        return await _dataStore.GetPopularExpertiseAsync(count);
    }

    public async Task<List<Expertise?>> FuzzySearchForExistingExpertise(string name, int count = 3)
    {
        return await _dataStore.FuzzySearchForExistingExpertise(name, count);
    }
}