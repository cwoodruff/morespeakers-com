using Microsoft.Extensions.Logging;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers;

public partial class ExpertiseManager : IExpertiseManager
{
    private readonly IExpertiseDataStore _dataStore;
    private readonly ILogger<ExpertiseManager> _logger;

    public ExpertiseManager(IExpertiseDataStore dataStore, ILogger<ExpertiseManager> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }

    public Task<Result<Expertise>> GetAsync(int primaryKey) => _dataStore.GetAsync(primaryKey);

    public Task<Result> DeleteAsync(int primaryKey) => _dataStore.DeleteAsync(primaryKey);

    public Task<Result> DeleteAsync(Expertise entity) => _dataStore.DeleteAsync(entity);

    public Task<Result<List<Expertise>>> GetAllAsync() => _dataStore.GetAllAsync();

    public async Task<Result<Expertise>> SaveAsync(Expertise entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            return Result.Failure<Expertise>(CreateExpertiseNameRequiredError());
        }

        entity.Name = entity.Name.Trim();
        entity.Description = NormalizeOptionalText(entity.Description);

        return await _dataStore.SaveAsync(entity);
    }

    public async Task<Result<int>> CreateExpertiseAsync(string name, string? description = null, int expertiseCategoryId = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<int>(CreateExpertiseNameRequiredError());
        }

        var saveResult = await _dataStore.SaveAsync(new Expertise
        {
            Name = name.Trim(),
            Description = NormalizeOptionalText(description),
            ExpertiseCategoryId = expertiseCategoryId
        });

        return saveResult.IsFailure
            ? Result.Failure<int>(saveResult.Error)
            : Result.Success(saveResult.Value.Id);
    }

    public Task<Result<IEnumerable<Expertise>>> GetPopularExpertiseAsync(int count = 10) =>
        _dataStore.GetPopularExpertiseAsync(count);

    public Task<Result<IEnumerable<Expertise>>> FuzzySearchForExistingExpertise(string name, int count = 3) =>
        _dataStore.FuzzySearchForExistingExpertise(name, count);

    public Task<Result<List<Expertise>>> GetByCategoryIdAsync(int categoryId) =>
        _dataStore.GetByCategoryIdAsync(categoryId);

    public async Task<Result<bool>> DoesExpertiseWithNameExistsAsync(string expertiseName)
    {
        if (string.IsNullOrWhiteSpace(expertiseName))
        {
            return Result.Failure<bool>(CreateExpertiseNameRequiredError());
        }

        return await _dataStore.DoesExpertiseWithNameExistsAsync(expertiseName.Trim());
    }

    public Task<Result<List<Expertise>>> GetAllExpertisesAsync(
        MoreSpeakers.Domain.Models.AdminUsers.TriState active = MoreSpeakers.Domain.Models.AdminUsers.TriState.True,
        string? searchTerm = "") =>
        _dataStore.GetAllExpertisesAsync(active, searchTerm);

    public async Task<Result> SoftDeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure(CreateExpertiseIdInvalidError(id));
        }

        var result = await _dataStore.SoftDeleteAsync(id);
        if (result.IsSuccess)
        {
            LogSoftDeletedExpertiseWithIdId(id);
        }
        else
        {
            LogSoftDeleteFailedForExpertiseIdId(id, result.Error.Code);
        }

        return result;
    }

    public Task<Result<ExpertiseCategory>> GetCategoryAsync(int id) => _dataStore.GetCategoryAsync(id);

    public async Task<Result<ExpertiseCategory>> SaveCategoryAsync(ExpertiseCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return Result.Failure<ExpertiseCategory>(CreateCategoryNameRequiredError());
        }

        if (category.SectorId <= 0)
        {
            return Result.Failure<ExpertiseCategory>(CreateCategorySectorRequiredError());
        }

        category.Name = category.Name.Trim();
        category.Description = NormalizeOptionalText(category.Description);

        return await _dataStore.SaveCategoryAsync(category);
    }

    public Task<Result> DeleteCategoryAsync(int id) => _dataStore.DeleteCategoryAsync(id);

    public Task<Result<List<ExpertiseCategory>>> GetAllCategoriesAsync(
        MoreSpeakers.Domain.Models.AdminUsers.TriState active = MoreSpeakers.Domain.Models.AdminUsers.TriState.True,
        string? searchTerm = "") =>
        _dataStore.GetAllCategoriesAsync(active, searchTerm);

    public Task<Result<List<ExpertiseCategory>>> GetAllActiveCategoriesForSector(int sectorId) =>
        _dataStore.GetAllActiveCategoriesForSector(sectorId);

    public Task<Result<List<Expertise>>> GetBySectorIdAsync(int sectorFilter) =>
        _dataStore.GetBySectorIdAsync(sectorFilter);

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Error CreateExpertiseNameRequiredError() =>
        new("expertise.validation.name-required", "Expertise name is required.");

    private static Error CreateExpertiseIdInvalidError(int id) =>
        new("expertise.validation.invalid-id", $"Expertise id '{id}' is invalid.");

    private static Error CreateCategoryNameRequiredError() =>
        new("expertise-category.validation.name-required", "Expertise category name is required.");

    private static Error CreateCategorySectorRequiredError() =>
        new("expertise-category.validation.sector-required", "Expertise category sector is required.");
}

