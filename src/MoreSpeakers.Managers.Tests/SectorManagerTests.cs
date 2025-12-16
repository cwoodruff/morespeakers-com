// File: MoreSpeakers.Managers.Tests/SectorManagerTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers;
using Xunit;

namespace MoreSpeakers.Managers.Tests;

public class SectorManagerTests
{
    private readonly Mock<ISectorDataStore> _dataStore = new();
    private readonly Mock<ILogger<SectorManager>> _logger = new();

    [Fact]
    public async Task GetAsync_returns_sector_from_datastore()
    {
        var expected = new Sector { Id = 1, Name = "Technology" };
        _dataStore.Setup(ds => ds.GetAsync(1)).ReturnsAsync(expected);

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.GetAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Technology");
    }

    [Fact]
    public async Task GetAllAsync_passes_onlyActive_to_datastore()
    {
        _dataStore.Setup(ds => ds.GetAllAsync(true))
            .ReturnsAsync(new List<Sector> { new() { Id = 1, Name = "Technology", IsActive = true } });

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.GetAllAsync(onlyActive: true);

        result.Should().HaveCount(1);
        _dataStore.Verify(ds => ds.GetAllAsync(true), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_calls_datastore_and_returns_saved()
    {
        var input = new Sector { Name = "Technology", Slug = "technology", DisplayOrder = 1, IsActive = true };
        var saved = new Sector { Id = 1, Name = "Technology", Slug = "technology", DisplayOrder = 1, IsActive = true };

        _dataStore.Setup(ds => ds.SaveAsync(It.IsAny<Sector>())).ReturnsAsync(saved);

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.SaveAsync(input);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Technology");
        _dataStore.Verify(ds => ds.SaveAsync(It.Is<Sector>(s => s.Name == "Technology")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_by_id_calls_datastore()
    {
        _dataStore.Setup(ds => ds.DeleteAsync(1)).ReturnsAsync(true);

        var manager = new SectorManager(_dataStore.Object, _logger.Object);
        var result = await manager.DeleteAsync(1);

        result.Should().BeTrue();
        _dataStore.Verify(ds => ds.DeleteAsync(1), Times.Once);
    }
}