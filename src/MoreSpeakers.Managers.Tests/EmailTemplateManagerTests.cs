using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Managers.Tests;

public class EmailTemplateManagerTests
{
    private readonly Mock<IEmailTemplateDataStore> _dataStoreMock = new();
    private readonly Mock<ILogger<EmailTemplateManager>> _loggerMock = new();

    private EmailTemplateManager CreateSut() => new(_dataStoreMock.Object, _loggerMock.Object);

    [Fact]
    public async Task GetAsync_should_delegate()
    {
        var id = 1;
        _dataStoreMock.Setup(d => d.GetAsync(id)).ReturnsAsync(new EmailTemplate { Id = id });
        var sut = CreateSut();

        var result = await sut.GetAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        _dataStoreMock.Verify(d => d.GetAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByLocationAsync_should_delegate()
    {
        var location = "Templates/Welcome.cshtml";
        _dataStoreMock.Setup(d => d.GetByLocationAsync(location)).ReturnsAsync(new EmailTemplate { Location = location });
        var sut = CreateSut();

        var result = await sut.GetByLocationAsync(location);

        result.Should().NotBeNull();
        result!.Location.Should().Be(location);
        _dataStoreMock.Verify(d => d.GetByLocationAsync(location), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_should_delegate()
    {
        var id = 1;
        _dataStoreMock.Setup(d => d.DeleteAsync(id)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(id);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_should_delegate()
    {
        var template = new EmailTemplate { Location = "L", Content = "C" };
        _dataStoreMock.Setup(d => d.SaveAsync(template)).ReturnsAsync(template);
        var sut = CreateSut();

        var result = await sut.SaveAsync(template);

        result.Should().BeSameAs(template);
        _dataStoreMock.Verify(d => d.SaveAsync(template), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_should_delegate()
    {
        var expected = new List<EmailTemplate> { new() { Location = "L1" } };
        _dataStoreMock.Setup(d => d.GetAllAsync()).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllTemplatesAsync_with_filters_should_delegate()
    {
        var expected = new List<EmailTemplate> { new() { Location = "L1" } };
        var active = TriState.True;
        var q = "test";
        _dataStoreMock.Setup(d => d.GetAllTemplatesAsync(active, q)).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.GetAllTemplatesAsync(active, q);

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.GetAllTemplatesAsync(active, q), Times.Once);
    }
}