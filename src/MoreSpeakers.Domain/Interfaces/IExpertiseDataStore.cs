using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseDataStore: IDataStorePrimaryKeyInt<Expertise>
{
    Task<int> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 0);
    Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10);
    Task<bool> DoesExpertiseWithNameExistsAsync(string expertiseName);
    Task<IEnumerable<Expertise>> FuzzySearchForExistingExpertise(string name, int count = 3);
    Task<List<Expertise>> GetByCategoryIdAsync(int categoryId);

    // Expertise Category operations
    Task<ExpertiseCategory?> GetCategoryAsync(int id);
    Task<ExpertiseCategory> SaveCategoryAsync(ExpertiseCategory category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<List<ExpertiseCategory>> GetAllCategoriesAsync(bool onlyActive = true);
}