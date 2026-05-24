using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Managers.Tests;

public class SectorManagerTests
{
    private readonly Mock<ISectorDataStore> _dataStore = new();
    private readonly Mock<ILogger<SectorManager>> _logger = new();

    [Fact]
    public async Task GetAsync_returns_sector_from_datastore()
    {
        var expected = new Sector { Id = 1, Name = "Technology" };
        _dataStore.Setup(ds => ds.GetAsync(1)).ReturnsAsync(Result.Success(expected));

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.GetAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Technology");
    }

    [Fact]
    public async Task GetAllAsync_passes_onlyActive_to_datastore()
    {
        var sectors = new List<Sector> { new() { Id = 1, Name = "Technology", IsActive = true } };
        _dataStore.Setup(ds => ds.GetAllAsync())
            .ReturnsAsync(Result.Success(sectors));

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        _dataStore.Verify(ds => ds.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_calls_datastore_and_returns_saved()
    {
        var input = new Sector { Name = "Technology", Slug = "technology", DisplayOrder = 1, IsActive = true };
        var saved = new Sector { Id = 1, Name = "Technology", Slug = "technology", DisplayOrder = 1, IsActive = true };

        _dataStore.Setup(ds => ds.SaveAsync(It.IsAny<Sector>())).ReturnsAsync(Result.Success(saved));

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.SaveAsync(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Technology");
        _dataStore.Verify(ds => ds.SaveAsync(It.Is<Sector>(s => s.Name == "Technology")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_by_id_calls_datastore()
    {
        _dataStore.Setup(ds => ds.DeleteAsync(1)).ReturnsAsync(Result.Success());

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
        _dataStore.Verify(ds => ds.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetSectorWithRelationshipsAsync_should_delegate()
    {
        var expected = new Sector { Id = 2, Name = "Finance" };
        _dataStore.Setup(ds => ds.GetSectorWithRelationshipsAsync(2)).ReturnsAsync(Result.Success(expected));

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.GetSectorWithRelationshipsAsync(2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expected);
        _dataStore.Verify(ds => ds.GetSectorWithRelationshipsAsync(2), Times.Once);
    }

    [Fact]
    public async Task GetAllSectorsAsync_should_delegate_with_filters()
    {
        var expected = new List<Sector> { new() { Id = 3, Name = "Healthcare" } };
        _dataStore.Setup(ds => ds.GetAllSectorsAsync(TriState.False, "care", true)).ReturnsAsync(Result.Success(expected));

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.GetAllSectorsAsync(TriState.False, "care", true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expected);
        _dataStore.Verify(ds => ds.GetAllSectorsAsync(TriState.False, "care", true), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_by_entity_calls_datastore()
    {
        var sector = new Sector { Id = 5, Name = "Education" };
        _dataStore.Setup(ds => ds.DeleteAsync(sector)).ReturnsAsync(Result.Success());

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.DeleteAsync(sector);

        result.IsSuccess.Should().BeTrue();
        _dataStore.Verify(ds => ds.DeleteAsync(sector), Times.Once);
    }
}
