using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers;

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
    public async Task SearchExpertiseAsync_should_delegate()
    {
        var expected = new List<Expertise> { new() { Id = 3 } };
        _dataStoreMock.Setup(d => d.SearchExpertiseAsync("cloud")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.SearchExpertiseAsync("cloud");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.SearchExpertiseAsync("cloud"), Times.Once);
    }

    [Fact]
    public async Task CreateExpertiseAsync_should_return_id_on_success()
    {
        Expertise? saved = null;
        _dataStoreMock.Setup(d => d.SaveAsync(It.IsAny<Expertise>()))
            .Callback<Expertise>(e => { e.Id = 42; saved = e; })
            .ReturnsAsync((Expertise e) => e);
        var sut = CreateSut();

        var id = await sut.CreateExpertiseAsync("AI", "desc");

        id.Should().Be(42);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("AI");
        saved.Description.Should().Be("desc");
        _dataStoreMock.Verify(d => d.SaveAsync(It.IsAny<Expertise>()), Times.Once);
    }

    [Fact]
    public async Task CreateExpertiseAsync_should_return_zero_on_exception()
    {
        _dataStoreMock.Setup(d => d.SaveAsync(It.IsAny<Expertise>())).ThrowsAsync(new Exception("boom"));
        var sut = CreateSut();

        var id = await sut.CreateExpertiseAsync("AI");

        id.Should().Be(0);
    }

    [Fact]
    public async Task UpdateExpertiseAsync_should_return_true_on_success()
    {
        _dataStoreMock.Setup(d => d.SaveAsync(It.Is<Expertise>(e => e.Id == 7 && e.Name == "AI"))).ReturnsAsync(new Expertise());
        var sut = CreateSut();

        var result = await sut.UpdateExpertiseAsync(7, "AI", "desc");

        result.Should().BeTrue();
        _dataStoreMock.Verify(d => d.SaveAsync(It.IsAny<Expertise>()), Times.Once);
    }

    [Fact]
    public async Task UpdateExpertiseAsync_should_return_false_on_exception()
    {
        _dataStoreMock.Setup(d => d.SaveAsync(It.IsAny<Expertise>())).ThrowsAsync(new Exception("boom"));
        var sut = CreateSut();

        var result = await sut.UpdateExpertiseAsync(8, "ML");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchForExpertiseExistsAsync_should_delegate()
    {
        var expected = new Expertise { Id = 1, Name = "AI" };
        _dataStoreMock.Setup(d => d.SearchForExpertiseExistsAsync("AI")).ReturnsAsync(expected);
        var sut = CreateSut();

        var result = await sut.SearchForExpertiseExistsAsync("AI");

        result.Should().BeSameAs(expected);
        _dataStoreMock.Verify(d => d.SearchForExpertiseExistsAsync("AI"), Times.Once);
    }

    [Fact]
    public async Task DeleteExpertiseAsync_should_delegate()
    {
        _dataStoreMock.Setup(d => d.DeleteAsync(10)).ReturnsAsync(true);
        var sut = CreateSut();

        var result = await sut.DeleteExpertiseAsync(10);

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
}
