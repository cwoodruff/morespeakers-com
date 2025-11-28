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

    public async Task<int> CreateExpertiseAsync(string name, string? description = null)
    {
        var expertise = new Expertise { Name = name, Description = description };
        
        try
        {
            var savedExpertise = await _dataStore.SaveAsync(expertise);
            return savedExpertise.Id;  
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to create expertise '{Name}'", name);
            return 0;
        }
    }
    
    public async Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10)
    {
        return await _dataStore.GetPopularExpertiseAsync(count);
    }

    public async Task<bool> DoesExpertiseWithNameExistsAsync(string expertiseName)
    {
        return await _dataStore.DoesExpertiseWithNameExistsAsync(expertiseName);
    }
    
    public async Task<IEnumerable<Expertise>> FuzzySearchForExistingExpertise(string name, int count = 3)
    {
        return await _dataStore.FuzzySearchForExistingExpertise(name, count);
    }
}