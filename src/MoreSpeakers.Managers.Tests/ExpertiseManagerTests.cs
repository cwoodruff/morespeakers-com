using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Managers.Tests;

public class ExpertiseManagerTests
{
    private readonly Mock<IExpertiseDataStore> _dataStoreMock = new();
    private readonly Mock<ILogger<ExpertiseManager>> _loggerMock = new();

    private ExpertiseManager CreateSut() => new(_dataStoreMock.Object, _loggerMock.Object);

    [Fact]
    public async Task GetAsync_should_return_success_result_from_data_store()
    {
        var expected = new Expertise { Id = 5, Name = "Cloud" };
        _dataStoreMock.Setup(d => d.GetAsync(5)).ReturnsAsync(Result.Success(expected));
        var sut = CreateSut();

        var result = await sut.GetAsync(5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task CreateExpertiseAsync_should_trim_input_and_delegate_to_data_store()
    {
        _dataStoreMock
            .Setup(d => d.SaveAsync(It.IsAny<Expertise>())).ReturnsAsync((Expertise e) => { e.Id = 42; return Result.Success(e); });
        var sut = CreateSut();

        var result = await sut.CreateExpertiseAsync("  AI  ", "  desc  ", 7);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        _dataStoreMock.Verify(d => d.SaveAsync(It.Is<Expertise>(e => e.Name == "AI" && e.Description == "desc" && e.ExpertiseCategoryId == 7)), Times.Once);
    }

    [Fact]
    public async Task CreateExpertiseAsync_should_return_failure_when_name_is_blank()
    {
        var sut = CreateSut();

        var result = await sut.CreateExpertiseAsync("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("expertise.validation.name-required");
        _dataStoreMock.Verify(d => d.SaveAsync(It.IsAny<Expertise>()), Times.Never);
    }

    [Fact]
    public async Task CreateExpertiseAsync_should_propagate_failure_from_data_store()
    {
        var error = new Error("expertise.save.failed", "Unable to save expertise.");
        _dataStoreMock
            .Setup(d => d.SaveAsync(It.IsAny<Expertise>())).ReturnsAsync(Result.Failure<Expertise>(error));
        var sut = CreateSut();

        var result = await sut.CreateExpertiseAsync("AI", null, 3);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task Save_and_delete_operations_should_delegate_result_contracts()
    {
        var entity = new Expertise { Id = 9, Name = "Kubernetes" };
        _dataStoreMock.Setup(d => d.SaveAsync(entity)).ReturnsAsync(Result.Success(entity));
        _dataStoreMock.Setup(d => d.DeleteAsync(9)).ReturnsAsync(Result.Success());
        _dataStoreMock.Setup(d => d.DeleteAsync(entity)).ReturnsAsync(Result.Success());
        var sut = CreateSut();

        var saveResult = await sut.SaveAsync(entity);
        var deleteByIdResult = await sut.DeleteAsync(9);
        var deleteByEntityResult = await sut.DeleteAsync(entity);

        saveResult.IsSuccess.Should().BeTrue();
        saveResult.Value.Should().BeSameAs(entity);
        deleteByIdResult.IsSuccess.Should().BeTrue();
        deleteByEntityResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Read_operations_should_delegate_success_results()
    {
        var all = new List<Expertise> { new() { Id = 1, Name = "AI" } };
        var filtered = new List<Expertise> { new() { Id = 2, Name = "Cloud" } };
        var byCategory = new List<Expertise> { new() { Id = 3, ExpertiseCategoryId = 4 } };
        var bySector = new List<Expertise> { new() { Id = 4, Name = "Data" } };
        IEnumerable<Expertise> popular = new List<Expertise> { new() { Id = 5, Name = ".NET" } };
        IEnumerable<Expertise> fuzzy = new List<Expertise> { new() { Id = 6, Name = "Azure" } };
        _dataStoreMock.Setup(d => d.GetAllAsync()).ReturnsAsync(Result.Success(all));
        _dataStoreMock.Setup(d => d.GetAllExpertisesAsync(TriState.False, "search")).ReturnsAsync(Result.Success(filtered));
        _dataStoreMock.Setup(d => d.GetByCategoryIdAsync(4)).ReturnsAsync(Result.Success(byCategory));
        _dataStoreMock.Setup(d => d.GetBySectorIdAsync(8)).ReturnsAsync(Result.Success(bySector));
        _dataStoreMock.Setup(d => d.GetPopularExpertiseAsync(3)).ReturnsAsync(Result.Success(popular));
        _dataStoreMock.Setup(d => d.FuzzySearchForExistingExpertise("az", 4)).ReturnsAsync(Result.Success(fuzzy));
        _dataStoreMock.Setup(d => d.DoesExpertiseWithNameExistsAsync("cloud")).ReturnsAsync(Result.Success(true));
        var sut = CreateSut();

        var allResult = await sut.GetAllAsync();
        var filteredResult = await sut.GetAllExpertisesAsync(TriState.False, "search");
        var byCategoryResult = await sut.GetByCategoryIdAsync(4);
        var bySectorResult = await sut.GetBySectorIdAsync(8);
        var popularResult = await sut.GetPopularExpertiseAsync(3);
        var fuzzyResult = await sut.FuzzySearchForExistingExpertise("az", 4);
        var existsResult = await sut.DoesExpertiseWithNameExistsAsync("cloud");

        allResult.Value.Should().BeSameAs(all);
        filteredResult.Value.Should().BeSameAs(filtered);
        byCategoryResult.Value.Should().BeSameAs(byCategory);
        bySectorResult.Value.Should().BeSameAs(bySector);
        popularResult.Value.Should().BeSameAs(popular);
        fuzzyResult.Value.Should().BeSameAs(fuzzy);
        existsResult.Value.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeleteAsync_should_return_success_when_data_store_succeeds()
    {
        _dataStoreMock.Setup(d => d.SoftDeleteAsync(5)).ReturnsAsync(Result.Success());
        var sut = CreateSut();

        var result = await sut.SoftDeleteAsync(5);

        result.IsSuccess.Should().BeTrue();
        _dataStoreMock.Verify(d => d.SoftDeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_should_propagate_failure_result()
    {
        var error = new Error("expertise.soft-delete.failed", "Unable to archive expertise.");
        _dataStoreMock.Setup(d => d.SoftDeleteAsync(6)).ReturnsAsync(Result.Failure(error));
        var sut = CreateSut();

        var result = await sut.SoftDeleteAsync(6);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task Category_operations_should_delegate_result_contracts()
    {
        var category = new ExpertiseCategory { Id = 2, Name = "Tech", SectorId = 1 };
        var categories = new List<ExpertiseCategory> { category };
        _dataStoreMock.Setup(d => d.GetCategoryAsync(2)).ReturnsAsync(Result.Success(category));
        _dataStoreMock.Setup(d => d.SaveCategoryAsync(category)).ReturnsAsync(Result.Success(category));
        _dataStoreMock.Setup(d => d.DeleteCategoryAsync(2)).ReturnsAsync(Result.Success());
        _dataStoreMock.Setup(d => d.GetAllCategoriesAsync(TriState.True, "")).ReturnsAsync(Result.Success(categories));
        _dataStoreMock.Setup(d => d.GetAllActiveCategoriesForSector(1)).ReturnsAsync(Result.Success(categories));
        var sut = CreateSut();

        var getResult = await sut.GetCategoryAsync(2);
        var saveResult = await sut.SaveCategoryAsync(category);
        var deleteResult = await sut.DeleteCategoryAsync(2);
        var listResult = await sut.GetAllCategoriesAsync(TriState.True);
        var activeForSectorResult = await sut.GetAllActiveCategoriesForSector(1);

        getResult.Value.Should().BeSameAs(category);
        saveResult.Value.Should().BeSameAs(category);
        deleteResult.IsSuccess.Should().BeTrue();
        listResult.Value.Should().BeSameAs(categories);
        activeForSectorResult.Value.Should().BeSameAs(categories);
    }
}

