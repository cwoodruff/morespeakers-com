using FluentAssertions;

using Microsoft.Extensions.Logging;
using Moq;
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
    public async Task GetAsync_should_delegate()
    {
        _dataStoreMock.Setup(d => d.GetAsync(5)).ReturnsAsync(new Expertise { Id = 5 });
        var sut = CreateSut();

        var result = await sut.GetAsync(5);

        result.Id.Should().Be(5);
        _dataStoreMock.Verify(d => d.GetAsync(5), Times.Once);
    }

    [Fact]
    public async Task Delete_by_id_should_delegate()
    {
        _dataStoreMock.Setup(d => d.DeleteAsync(7)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(7);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(7), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_should_delegate()
    {
        var entity = new Expertise { Id = 1, Name = "x" };
        _dataStoreMock.Setup(d => d.SaveAsync(entity)).ReturnsAsync(entity);
        var sut = CreateSut();

        var result = await sut.SaveAsync(entity);

        result.Should().BeSameAs(entity);
        _dataStoreMock.Verify(d => d.SaveAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_should_delegate()
    {
        var expected = new List<Expertise> { new() { Id = 2 } };
        _dataStoreMock.Setup(d => d.GetAllAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_by_entity_should_delegate()
    {
        var entity = new Expertise { Id = 9 };
        _dataStoreMock.Setup(d => d.DeleteAsync(entity)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(entity);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DoesExpertiseWithNameExistsAsync_should_delegate()
    {
        _dataStoreMock.Setup(d => d.DoesExpertiseWithNameExistsAsync("cloud")).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DoesExpertiseWithNameExistsAsync("cloud");

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DoesExpertiseWithNameExistsAsync("cloud"), Times.Once);
    }

    [Fact]
    public async Task CreateExpertiseAsync_should_return_id_on_success()
    {
        Expertise? saved = null;
        _dataStoreMock.Setup(d => d.SaveAsync(It.IsAny<Expertise>()))
            .Callback<Expertise>(e => { e.Id = 42; saved = e; })
            .ReturnsAsync((Expertise e) => e);
        var sut = CreateSut();

        var id = await sut.CreateExpertiseAsync("AI", "desc", 7);

        id.Should().Be(42);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("AI");
        saved.Description.Should().Be("desc");
        saved.ExpertiseCategoryId.Should().Be(7);
        _dataStoreMock.Verify(d => d.SaveAsync(It.IsAny<Expertise>()), Times.Once);
    }

    [Fact]
    public async Task CreateExpertiseAsync_should_return_zero_on_exception()
    {
        _dataStoreMock.Setup(d => d.SaveAsync(It.IsAny<Expertise>())).ThrowsAsync(new Exception("boom"));
        var sut = CreateSut();

        var id = await sut.CreateExpertiseAsync("AI", null, 3);

        id.Should().Be(0);
    }

    [Fact]
    public async Task DoesExpertiseWithNameExistsAsync_should_delegate_for_different_input()
    {
        _dataStoreMock.Setup(d => d.DoesExpertiseWithNameExistsAsync("AI")).ReturnsAsync(false);
        var sut = CreateSut();

        var result = await sut.DoesExpertiseWithNameExistsAsync("AI");

        result.Should().BeFalse();
        _dataStoreMock.Verify(d => d.DoesExpertiseWithNameExistsAsync("AI"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_by_id_should_delegate_again()
    {
        _dataStoreMock.Setup(d => d.DeleteAsync(10)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(10);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetPopularExpertiseAsync_should_delegate()
    {
        var expected = new List<Expertise> { new() { Id = 1 } };
        _dataStoreMock.Setup(d => d.GetPopularExpertiseAsync(3)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetPopularExpertiseAsync(3);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetPopularExpertiseAsync(3), Times.Once);
    }

    [Fact]
    public async Task FuzzySearchForExistingExpertise_should_delegate()
    {
        var expected = new List<Expertise> { new() { Id = 2 } };
        _dataStoreMock.Setup(d => d.FuzzySearchForExistingExpertise("AI", 4)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.FuzzySearchForExistingExpertise("AI", 4);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.FuzzySearchForExistingExpertise("AI", 4), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_should_delegate_and_return_true()
    {
        _dataStoreMock.Setup(d => d.SoftDeleteAsync(5)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.SoftDeleteAsync(5);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.SoftDeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_should_delegate_and_return_false()
    {
        _dataStoreMock.Setup(d => d.SoftDeleteAsync(6)).ReturnsAsync(false);
        var sut = CreateSut();

        var result = await sut.SoftDeleteAsync(6);

        result.Should().BeFalse();
        _dataStoreMock.Verify(d => d.SoftDeleteAsync(6), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_should_return_false_on_exception()
    {
        _dataStoreMock.Setup(d => d.SoftDeleteAsync(12)).ThrowsAsync(new InvalidOperationException("boom"));
        var sut = CreateSut();

        var result = await sut.SoftDeleteAsync(12);

        result.Should().BeFalse();
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to soft delete expertise")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllExpertisesAsync_should_delegate()
    {
        var expected = new List<Expertise> { new() { Id = 3, Name = "AI" } };
        _dataStoreMock.Setup(d => d.GetAllExpertisesAsync(TriState.False, "search")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllExpertisesAsync(TriState.False, "search");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllExpertisesAsync(TriState.False, "search"), Times.Once);
    }

    [Fact]
    public async Task GetAllActiveCategoriesForSector_should_delegate()
    {
        var expected = new List<ExpertiseCategory> { new() { Id = 1, Name = "Tech" } };
        _dataStoreMock.Setup(d => d.GetAllActiveCategoriesForSector(2)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllActiveCategoriesForSector(2);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllActiveCategoriesForSector(2), Times.Once);
    }

    [Fact]
    public async Task GetBySectorIdAsync_should_delegate()
    {
        var expected = new List<Expertise> { new() { Id = 8, Name = "Cloud" } };
        _dataStoreMock.Setup(d => d.GetBySectorIdAsync(4)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetBySectorIdAsync(4);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetBySectorIdAsync(4), Times.Once);
    }
}

public class ExpertiseManagerCategoryTests
{
    private readonly Mock<IExpertiseDataStore> _dataStoreMock = new();
    private readonly Mock<ILogger<ExpertiseManager>> _loggerMock = new();

    private ExpertiseManager CreateSut() => new(_dataStoreMock.Object, _loggerMock.Object);

    [Fact]
    public async Task GetByCategoryIdAsync_should_delegate()
    {
        var expected = new List<Expertise> { new() { Id = 1, ExpertiseCategoryId = 5 } };
        _dataStoreMock.Setup(d => d.GetByCategoryIdAsync(5)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetByCategoryIdAsync(5);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetByCategoryIdAsync(5), Times.Once);
    }

    [Fact]
    public async Task Category_Get_Save_Delete_List_should_delegate()
    {
        var category = new ExpertiseCategory { Id = 2, Name = "Tech" };
        var categories = new List<ExpertiseCategory> { category };

        _dataStoreMock.Setup(d => d.GetCategoryAsync(2)).ReturnsAsync(category);
        _dataStoreMock.Setup(d => d.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()))
            .ReturnsAsync((ExpertiseCategory c) => c);
        _dataStoreMock.Setup(d => d.DeleteCategoryAsync(2)).ReturnsAsync(true);
        _dataStoreMock.Setup(d => d.GetAllCategoriesAsync(TriState.True)).ReturnsAsync(categories);

        var sut = CreateSut();

        var got = await sut.GetCategoryAsync(2);
        got.Should().BeSameAs(category);
        _dataStoreMock.Verify(d => d.GetCategoryAsync(2), Times.Once);

        var saved = await sut.SaveCategoryAsync(category);
        saved.Should().BeSameAs(category);
        _dataStoreMock.Verify(d => d.SaveCategoryAsync(category), Times.Once);

        var deleted = await sut.DeleteCategoryAsync(2);
        deleted.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteCategoryAsync(2), Times.Once);

        var list = await sut.GetAllCategoriesAsync(TriState.True);
        list.Should().BeSameAs(categories);
        _dataStoreMock.Verify(d => d.GetAllCategoriesAsync(TriState.True), Times.Once);
    }
}
