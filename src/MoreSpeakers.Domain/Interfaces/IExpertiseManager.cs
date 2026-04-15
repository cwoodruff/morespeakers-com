using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Domain.Interfaces;

public interface IExpertiseManager
{
    public Task<Result<Expertise>> GetAsync(int primaryKey);
    public Task<Result> DeleteAsync(int primaryKey);
    public Task<Result> DeleteAsync(Expertise entity);
    public Task<Result<Expertise>> SaveAsync(Expertise entity);
    public Task<Result<List<Expertise>>> GetAllAsync();
    Task<Result<int>> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 0);
    Task<Result<IEnumerable<Expertise>>> GetPopularExpertiseAsync(int count = 10);
    Task<Result<bool>> DoesExpertiseWithNameExistsAsync(string expertiseName);
    Task<Result<IEnumerable<Expertise>>> FuzzySearchForExistingExpertise(string name, int count = 3);
    Task<Result> SoftDeleteAsync(int id);
    Task<Result<List<Expertise>>> GetAllExpertisesAsync(TriState active = TriState.True, string? searchTerm = "");

    // Expertise queries by category
    Task<Result<List<Expertise>>> GetByCategoryIdAsync(int categoryId);

    // Category operations
    Task<Result<ExpertiseCategory>> GetCategoryAsync(int id);
    Task<Result<ExpertiseCategory>> SaveCategoryAsync(ExpertiseCategory category);
    Task<Result> DeleteCategoryAsync(int id);
    Task<Result<List<ExpertiseCategory>>> GetAllCategoriesAsync(TriState active = TriState.True, string? searchTerm = "");
    Task<Result<List<ExpertiseCategory>>> GetAllActiveCategoriesForSector(int sectorId);
    Task<Result<List<Expertise>>> GetBySectorIdAsync(int sectorFilter);
}
