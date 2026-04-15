using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Data.MappingProfiles;

using DataCategory = MoreSpeakers.Data.Models.ExpertiseCategory;
using DataExpertise = MoreSpeakers.Data.Models.Expertise;
using DataSector = MoreSpeakers.Data.Models.Sector;
using DataUserExpertise = MoreSpeakers.Data.Models.UserExpertise;

using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Data.Tests;

public class ExpertiseDataStoreResultTests
{
    private static ExpertiseDataStore CreateStore(out MoreSpeakersDbContext context)
    {
        var options = new DbContextOptionsBuilder<MoreSpeakersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new MoreSpeakersDbContext(options);

        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<MoreSpeakersProfile>(), new LoggerFactory());
        return new ExpertiseDataStore(context, mapperConfiguration.CreateMapper(), new Mock<ILogger<ExpertiseDataStore>>().Object);
    }

    private static DataSector AddSector(MoreSpeakersDbContext context, int id, string name, bool isActive = true)
    {
        var sector = new DataSector { Id = id, Name = name, Slug = name.ToLowerInvariant(), IsActive = isActive };
        context.Sectors.Add(sector);
        return sector;
    }

    private static DataCategory AddCategory(MoreSpeakersDbContext context, int id, int sectorId, string name, bool isActive = true)
    {
        var category = new DataCategory { Id = id, Name = name, SectorId = sectorId, IsActive = isActive };
        context.ExpertiseCategory.Add(category);
        return category;
    }

    private static DataExpertise AddExpertise(MoreSpeakersDbContext context, int id, int categoryId, string name, bool isActive = true, string? description = null)
    {
        var expertise = new DataExpertise
        {
            Id = id,
            Name = name,
            Description = description,
            ExpertiseCategoryId = categoryId,
            IsActive = isActive,
            CreatedDate = DateTime.UtcNow
        };
        context.Expertise.Add(expertise);
        return expertise;
    }

    [Fact]
    public async Task GetAsync_should_return_success_for_existing_expertise_and_failure_for_missing_expertise()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddCategory(context, 10, 1, "Platforms");
        var expertise = AddExpertise(context, 100, 10, "Cloud", description: "Azure");
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var success = await store.GetAsync(100);
        var failure = await store.GetAsync(999);

        Assert.True(success.IsSuccess);
        Assert.Equal(expertise.Name, success.Value.Name);
        Assert.True(failure.IsFailure);
        Assert.Equal("expertise.not-found", failure.Error.Code);
    }

    [Fact]
    public async Task SaveAsync_and_CreateExpertiseAsync_should_return_success_and_failure_results()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddCategory(context, 10, 1, "Platforms");
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var saveResult = await store.SaveAsync(new MoreSpeakers.Domain.Models.Expertise { Name = "AI", ExpertiseCategoryId = 10, IsActive = true });
        var createResult = await store.CreateExpertiseAsync("Data", "Desc", 10);

        Assert.True(saveResult.IsSuccess);
        Assert.NotEqual(0, saveResult.Value.Id);
        Assert.True(createResult.IsSuccess);
        Assert.NotEqual(0, createResult.Value);

        context.Dispose();

        var saveFailure = await store.SaveAsync(new MoreSpeakers.Domain.Models.Expertise { Name = "Broken", ExpertiseCategoryId = 10, IsActive = true });
        var createFailure = await store.CreateExpertiseAsync("Broken", null, 10);

        Assert.True(saveFailure.IsFailure);
        Assert.Equal("expertise.save.failed", saveFailure.Error.Code);
        Assert.True(createFailure.IsFailure);
        Assert.Equal("expertise.save.failed", createFailure.Error.Code);
    }

    [Fact]
    public async Task DeleteAsync_by_id_and_entity_should_return_success_and_not_found_failures()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddCategory(context, 10, 1, "Platforms");
        AddExpertise(context, 100, 10, "Cloud");
        AddExpertise(context, 101, 10, "AI");
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var deleteById = await store.DeleteAsync(100);
        var deleteByEntity = await store.DeleteAsync(new MoreSpeakers.Domain.Models.Expertise { Id = 101, Name = "AI" });
        var missingById = await store.DeleteAsync(999);
        var missingByEntity = await store.DeleteAsync(new MoreSpeakers.Domain.Models.Expertise { Id = 998, Name = "Missing" });

        Assert.True(deleteById.IsSuccess);
        Assert.True(deleteByEntity.IsSuccess);
        Assert.True(missingById.IsFailure);
        Assert.Equal("expertise.delete.not-found", missingById.Error.Code);
        Assert.True(missingByEntity.IsFailure);
        Assert.Equal("expertise.delete.not-found", missingByEntity.Error.Code);
    }

    [Fact]
    public async Task SoftDeleteAsync_should_return_success_for_active_expertise_and_fail_for_missing_or_inactive_expertise()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddCategory(context, 10, 1, "Platforms");
        AddExpertise(context, 100, 10, "Cloud", isActive: true);
        AddExpertise(context, 101, 10, "Legacy", isActive: false);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var success = await store.SoftDeleteAsync(100);
        var missing = await store.SoftDeleteAsync(999);
        var inactive = await store.SoftDeleteAsync(101);

        Assert.True(success.IsSuccess);
        Assert.False((await store.GetAsync(100)).Value.IsActive);
        Assert.True(missing.IsFailure);
        Assert.Equal("expertise.soft-delete.not-found", missing.Error.Code);
        Assert.True(inactive.IsFailure);
        Assert.Equal("expertise.soft-delete.inactive", inactive.Error.Code);
    }

    [Fact]
    public async Task DoesExpertiseWithNameExistsAsync_should_return_success_results_and_failure_when_context_is_disposed()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddCategory(context, 10, 1, "Platforms");
        AddExpertise(context, 100, 10, "Cloud");
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exists = await store.DoesExpertiseWithNameExistsAsync(" cloud ");
        var missing = await store.DoesExpertiseWithNameExistsAsync("AI");

        Assert.True(exists.IsSuccess);
        Assert.True(exists.Value);
        Assert.True(missing.IsSuccess);
        Assert.False(missing.Value);

        context.Dispose();
        var failure = await store.DoesExpertiseWithNameExistsAsync("Cloud");

        Assert.True(failure.IsFailure);
        Assert.Equal("expertise.exists.failed", failure.Error.Code);
    }

    [Fact]
    public async Task Expertise_query_operations_should_return_success_results()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddSector(context, 2, "Health");
        AddCategory(context, 10, 1, "Platforms", isActive: true);
        AddCategory(context, 11, 1, "Architecture", isActive: false);
        AddCategory(context, 20, 2, "Devices", isActive: true);
        var cloud = AddExpertise(context, 100, 10, "Cloud", isActive: true, description: "Azure cloud");
        var ai = AddExpertise(context, 101, 10, "AI", isActive: true, description: "ML");
        AddExpertise(context, 102, 11, "Legacy", isActive: false, description: "Old stack");
        AddExpertise(context, 103, 20, "IoT", isActive: true, description: "Devices");
        context.UserExpertise.AddRange(
            new DataUserExpertise { ExpertiseId = cloud.Id, UserId = Guid.NewGuid() },
            new DataUserExpertise { ExpertiseId = cloud.Id, UserId = Guid.NewGuid() },
            new DataUserExpertise { ExpertiseId = ai.Id, UserId = Guid.NewGuid() });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var allResult = await store.GetAllAsync();
        var filteredResult = await store.GetAllExpertisesAsync(TriState.True, "C");
        var popularResult = await store.GetPopularExpertiseAsync(1);
        var fuzzyResult = await store.FuzzySearchForExistingExpertise("clo", 2);
        var byCategoryResult = await store.GetByCategoryIdAsync(10);
        var bySectorResult = await store.GetBySectorIdAsync(1);
        var allCategoriesResult = await store.GetAllCategoriesAsync(TriState.Any, null);
        var activeCategoriesResult = await store.GetAllActiveCategoriesForSector(1);

        Assert.True(allResult.IsSuccess);
        Assert.Equal(4, allResult.Value.Count);
        Assert.True(filteredResult.IsSuccess);
        Assert.Contains(filteredResult.Value, e => e.Name == "Cloud");
        Assert.True(popularResult.IsSuccess);
        Assert.Equal("Cloud", popularResult.Value.Single().Name);
        Assert.True(fuzzyResult.IsSuccess);
        Assert.Contains(fuzzyResult.Value, e => e.Name == "Cloud");
        Assert.True(byCategoryResult.IsSuccess);
        Assert.Equal(2, byCategoryResult.Value.Count);
        Assert.True(bySectorResult.IsSuccess);
        Assert.Equal(2, bySectorResult.Value.Count);
        Assert.True(allCategoriesResult.IsSuccess);
        Assert.Equal(3, allCategoriesResult.Value.Count);
        Assert.True(activeCategoriesResult.IsSuccess);
        Assert.Single(activeCategoriesResult.Value);
    }

    [Fact]
    public async Task Expertise_query_operations_should_return_failure_results_when_context_is_disposed()
    {
        var store = CreateStore(out var context);
        context.Dispose();

        Assert.Equal("expertise.list.failed", (await store.GetAllAsync()).Error.Code);
        Assert.Equal("expertise.filtered-list.failed", (await store.GetAllExpertisesAsync()).Error.Code);
        Assert.Equal("expertise.popular.failed", (await store.GetPopularExpertiseAsync()).Error.Code);
        Assert.Equal("expertise.search.failed", (await store.FuzzySearchForExistingExpertise("cloud")).Error.Code);
        Assert.Equal("expertise.by-category.failed", (await store.GetByCategoryIdAsync(10)).Error.Code);
        Assert.Equal("expertise.by-sector.failed", (await store.GetBySectorIdAsync(1)).Error.Code);
        Assert.Equal("expertise-category.list.failed", (await store.GetAllCategoriesAsync()).Error.Code);
        Assert.Equal("expertise-category.by-sector.failed", (await store.GetAllActiveCategoriesForSector(1)).Error.Code);
    }

    [Fact]
    public async Task Category_operations_should_return_success_results()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddCategory(context, 10, 1, "Platforms", isActive: true);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var getResult = await store.GetCategoryAsync(10);
        var saveResult = await store.SaveCategoryAsync(new MoreSpeakers.Domain.Models.ExpertiseCategory
        {
            Name = "Data",
            SectorId = 1,
            IsActive = true
        });
        var deleteResult = await store.DeleteCategoryAsync(10);

        Assert.True(getResult.IsSuccess);
        Assert.Equal("Platforms", getResult.Value.Name);
        Assert.True(saveResult.IsSuccess);
        Assert.NotEqual(0, saveResult.Value.Id);
        Assert.True(deleteResult.IsSuccess);
    }

    [Fact]
    public async Task Category_operations_should_return_failure_results_for_expected_errors()
    {
        var store = CreateStore(out var context);
        AddSector(context, 1, "Tech");
        AddCategory(context, 10, 1, "Platforms", isActive: true);
        AddExpertise(context, 100, 10, "Cloud");
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var missingCategory = await store.GetCategoryAsync(999);
        var deleteWithExpertises = await store.DeleteCategoryAsync(10);
        var missingDelete = await store.DeleteCategoryAsync(999);

        Assert.True(missingCategory.IsFailure);
        Assert.Equal("expertise-category.not-found", missingCategory.Error.Code);
        Assert.True(deleteWithExpertises.IsFailure);
        Assert.Equal("expertise-category.delete.has-expertises", deleteWithExpertises.Error.Code);
        Assert.True(missingDelete.IsFailure);
        Assert.Equal("expertise-category.delete.not-found", missingDelete.Error.Code);

        context.Dispose();
        var saveFailure = await store.SaveCategoryAsync(new MoreSpeakers.Domain.Models.ExpertiseCategory { Name = "Broken", SectorId = 1, IsActive = true });

        Assert.True(saveFailure.IsFailure);
        Assert.Equal("expertise-category.save.failed", saveFailure.Error.Code);
    }
}


