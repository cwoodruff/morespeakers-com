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


    public async Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm)
    {
        return await _dataStore.SearchExpertiseAsync(searchTerm);
    }

    public async Task<bool> CreateExpertiseAsync(string name, string? description = null)
    {
        var expertise = new Expertise { Name = name, Description = description };
        
        try
        {
            await _dataStore.SaveAsync(expertise);
            return true;
        }
        catch
        {
            return false;
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
            return false;
        }
    }

    public async Task<bool> DeleteExpertiseAsync(int id)
    {
        return await _dataStore.DeleteAsync(id);
    }

    public async Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10)
    {
        return await _dataStore.GetPopularExpertiseAsync(count);
    }
}