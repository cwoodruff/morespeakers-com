using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseManager
{
    public Task<Expertise> GetAsync(int primaryKey);
    public Task<bool> DeleteAsync(int primaryKey);
    public Task<bool> DeleteAsync(Expertise entity);
    public Task<Expertise> SaveAsync(Expertise entity);
    public Task<List<Expertise>> GetAllAsync();
    Task<int> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 0);
    Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10);
    Task<bool> DoesExpertiseWithNameExistsAsync(string expertiseName);
    Task<IEnumerable<Expertise>> FuzzySearchForExistingExpertise(string name, int count = 3);
    Task<bool> SoftDeleteAsync(int id);
    Task<List<Expertise>> GetAllExpertisesAsync(TriState active = TriState.True, string? searchTerm = "");

    // Expertise queries by category
    Task<List<Expertise>> GetByCategoryIdAsync(int categoryId);

    // Category operations
    Task<ExpertiseCategory?> GetCategoryAsync(int id);
    Task<ExpertiseCategory> SaveCategoryAsync(ExpertiseCategory category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<List<ExpertiseCategory>> GetAllCategoriesAsync(TriState active = TriState.True, string? searchTerm = "");
    Task<List<ExpertiseCategory>> GetAllActiveCategoriesForSector(int sectorId);
    Task<List<Expertise>> GetBySectorIdAsync(int sectorFilter);
}