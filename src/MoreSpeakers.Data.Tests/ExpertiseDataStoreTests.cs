using FluentAssertions;

namespace MoreSpeakers.Data.Tests;

public class ExpertiseDataStoreTests : DataStoreTestBase
{
    [Fact]
    public async Task GetAsync_WhenExpertiseExists_ShouldReturnSuccess()
    {
        var sector = await AddSectorAsync(x => x.Name = "Technology");
        var category = await AddCategoryAsync(sector, x => x.Name = "Platforms");
        var expertise = await AddExpertiseAsync(category, name: "Cloud", description: "Azure workloads");

        var sut = CreateExpertiseStore();

        var result = await sut.GetAsync(expertise.Id);

        var value = result.ShouldSucceed();
        value.Id.Should().Be(expertise.Id);
        value.Name.Should().Be("Cloud");
        value.Description.Should().Be("Azure workloads");
    }

    [Fact]
    public async Task GetAsync_WhenExpertiseDoesNotExist_ShouldReturnFailure()
    {
        var sut = CreateExpertiseStore();

        var result = await sut.GetAsync(999);

        result.ShouldFail("expertise.not-found");
    }

    [Fact]
    public async Task GetAllAsync_WhenExpertisesExist_ShouldReturnOrderedResults()
    {
        var sector = await AddSectorAsync();
        var category = await AddCategoryAsync(sector, x => x.Name = "Backend");

        await AddExpertiseAsync(category, name: "Zebra");
        await AddExpertiseAsync(category, name: "Alpha");

        var sut = CreateExpertiseStore();

        var result = await sut.GetAllAsync();

        var values = result.ShouldSucceed();
        values.Select(x => x.Name).Should().ContainInOrder("Alpha", "Zebra");
    }

    [Fact]
    public async Task SaveAsync_WhenExpertiseIsNew_ShouldPersistAndReturnSuccess()
    {
        var sector = await AddSectorAsync();
        var category = await AddCategoryAsync(sector);
        var sut = CreateExpertiseStore();

        var result = await sut.SaveAsync(new MoreSpeakers.Domain.Models.Expertise
        {
            Name = "Observability",
            Description = "Tracing and metrics",
            ExpertiseCategoryId = category.Id,
            IsActive = true
        });

        var value = result.ShouldSucceed();
        value.Id.Should().BeGreaterThan(0);

        var persisted = await Context.Expertise.FindAsync([value.Id], CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Observability");
    }

    [Fact]
    public async Task SaveAsync_WhenExpertiseExists_ShouldUpdateExistingRecord()
    {
        var sector = await AddSectorAsync();
        var category = await AddCategoryAsync(sector);
        var expertise = await AddExpertiseAsync(category, name: "Cloud", description: "Before");
        Context.ChangeTracker.Clear();

        var sut = CreateExpertiseStore();

        var result = await sut.SaveAsync(new MoreSpeakers.Domain.Models.Expertise
        {
            Id = expertise.Id,
            Name = "Cloud Native",
            Description = "After",
            ExpertiseCategoryId = category.Id,
            IsActive = true
        });

        var value = result.ShouldSucceed();
        value.Name.Should().Be("Cloud Native");

        var persisted = await Context.Expertise.FindAsync([expertise.Id], CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Cloud Native");
        persisted.Description.Should().Be("After");
    }

    [Fact]
    public async Task DeleteAsync_WhenExpertiseExists_ShouldRemoveRecord()
    {
        var sector = await AddSectorAsync();
        var category = await AddCategoryAsync(sector);
        var expertise = await AddExpertiseAsync(category);
        var sut = CreateExpertiseStore();

        var result = await sut.DeleteAsync(expertise.Id);

        result.ShouldSucceed();
        Context.Expertise.Should().NotContain(x => x.Id == expertise.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenExpertiseDoesNotExist_ShouldReturnFailure()
    {
        var sut = CreateExpertiseStore();

        var result = await sut.DeleteAsync(404);

        result.ShouldFail("expertise.delete.not-found");
    }
}
