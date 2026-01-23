using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers.Tests;

public class SocialMediaSiteManagerTests
{
    private readonly Mock<ISocialMediaSiteDataStore> _dataStoreMock = new();
    private readonly Mock<ILogger<SocialMediaSiteManager>> _loggerMock = new();

    private SocialMediaSiteManager CreateSut() => new(_dataStoreMock.Object, _loggerMock.Object, GetInMemoryTelemetryClient());
        
    private TelemetryClient GetInMemoryTelemetryClient()
    {
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(),
            DisableTelemetry = true // Prevents sending data
        };
        return new TelemetryClient(telemetryConfiguration);
    }
    
    [Fact]
    public async Task GetAsync_should_delegate()
    {
        var id = 42;
        var expected = new SocialMediaSite { Id = id, Name = "X/Twitter", Icon = "x", UrlFormat = "https://twitter.com/{0}" };
        _dataStoreMock.Setup(d => d.GetAsync(id)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAsync(id);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_by_id_should_delegate()
    {
        var id = 7;
        _dataStoreMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(id);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_should_delegate()
    {
        var entity = new SocialMediaSite { Id = 5, Name = "LinkedIn", Icon = "in", UrlFormat = "https://linkedin.com/in/{0}" };
        _dataStoreMock.Setup(d => d.SaveAsync(entity)).ReturnsAsync(entity);
        var sut = CreateSut();

        var result = await sut.SaveAsync(entity);

        result.Should().BeSameAs(entity);
        _dataStoreMock.Verify(d => d.SaveAsync(entity), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_should_delegate()
    {
        var expected = new List<SocialMediaSite> { new() { Id = 1, Name = "YouTube", Icon = "yt", UrlFormat = "https://youtube.com/@{0}" } };
        _dataStoreMock.Setup(d => d.GetAllAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_by_entity_should_delegate()
    {
        var entity = new SocialMediaSite { Id = 9, Name = "Mastodon", Icon = "mstdn", UrlFormat = "https://mastodon.social/@{0}" };
        _dataStoreMock.Setup(d => d.DeleteAsync(entity)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(entity);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task RefCountAsync_should_delegate()
    {
        _dataStoreMock.Setup(d => d.RefCountAsync(3)).ReturnsAsync(2);
        var sut = CreateSut();

        var result = await sut.RefCountAsync(3);

        result.Should().Be(2);
        _dataStoreMock.Verify(d => d.RefCountAsync(3), Times.Once);
    }

    [Fact]
    public async Task InUseAsync_should_delegate()
    {
        _dataStoreMock.Setup(d => d.InUseAsync(4)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.InUseAsync(4);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.InUseAsync(4), Times.Once);
    }
}
