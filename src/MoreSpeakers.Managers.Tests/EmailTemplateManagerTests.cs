using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Managers.Tests;

public class EmailTemplateManagerTests
{
    private readonly Mock<IEmailTemplateDataStore> _dataStoreMock = new();
    private readonly Mock<ILogger<EmailTemplateManager>> _loggerMock = new();

    private EmailTemplateManager CreateSut() => new(_dataStoreMock.Object, _loggerMock.Object);

    [Fact]
    public async Task GetAsync_should_delegate()
    {
        var location = "Templates/Welcome.cshtml";
        _dataStoreMock.Setup(d => d.GetAsync(location)).ReturnsAsync(new EmailTemplate { Location = location });
        var sut = CreateSut();

        var result = await sut.GetAsync(location);

        result.Should().NotBeNull();
        result!.Location.Should().Be(location);
        _dataStoreMock.Verify(d => d.GetAsync(location), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_should_delegate()
    {
        var location = "Templates/Old.cshtml";
        _dataStoreMock.Setup(d => d.DeleteAsync(location)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteAsync(location);

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.DeleteAsync(location), Times.Once);
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
}