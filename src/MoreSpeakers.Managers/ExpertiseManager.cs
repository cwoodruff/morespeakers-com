using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

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

    public async Task<int> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 1)
    {
        var expertise = new Expertise { Name = name, Description = description, ExpertiseCategoryId = expertiseCategoryId };
        
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

    public async Task<List<Expertise>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dataStore.GetByCategoryIdAsync(categoryId);
    }

    public async Task<List<Expertise>>
        GetAllExpertisesAsync(TriState active = TriState.True, string? searchTerm = "") =>
        await _dataStore.GetAllExpertisesAsync(active, searchTerm);

    public async Task<bool> SoftDeleteAsync(int id)
    {
        try
        {
            var result = await _dataStore.SoftDeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Soft-deleted expertise with id {Id}", id);
            }
            else
            {
                _logger.LogWarning("Soft delete returned false for expertise id {Id}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete expertise id {Id}", id);
            return false;
        }
    }

    public async Task<ExpertiseCategory?> GetCategoryAsync(int id)
    {
        return await _dataStore.GetCategoryAsync(id);
    }

    public async Task<ExpertiseCategory> SaveCategoryAsync(ExpertiseCategory category)
    {
        return await _dataStore.SaveCategoryAsync(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _dataStore.DeleteCategoryAsync(id);
    }

    public async Task<List<ExpertiseCategory>> GetAllCategoriesAsync(TriState active = TriState.True, string? searchTerm = "")
    {
        return await _dataStore.GetAllCategoriesAsync(active, searchTerm);
    }
}