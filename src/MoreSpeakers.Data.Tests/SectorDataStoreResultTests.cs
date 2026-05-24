using FluentAssertions;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Data.Tests;

public class SectorDataStoreResultTests : DataStoreTestBase
{
    private SectorDataStore CreateStore() => new(Context, Mapper, CreateLogger<SectorDataStore>().Object);

    [Fact]
    public async Task GetAsync_should_return_success_for_existing_sector_and_failure_for_missing_sector()
    {
        var store = CreateStore();
        var sector = await AddSectorAsync(s => s.Name = "Technology");

        var success = await store.GetAsync(sector.Id);
        var failure = await store.GetAsync(999);

        success.ShouldSucceed().Name.Should().Be("Technology");
        failure.ShouldFail("sector.not-found");
    }

    [Fact]
    public async Task GetSectorWithRelationshipsAsync_should_return_success_with_categories_for_existing_sector_and_failure_for_missing()
    {
        var store = CreateStore();
        var sector = await AddSectorAsync(s => s.Name = "Technology");
        await AddCategoryAsync(sector, c => c.Name = "Platforms");

        var success = await store.GetSectorWithRelationshipsAsync(sector.Id);
        var failure = await store.GetSectorWithRelationshipsAsync(999);

        var successValue = success.ShouldSucceed();
        successValue.Name.Should().Be("Technology");
        successValue.ExpertiseCategories.Should().HaveCount(1);
        failure.ShouldFail("sector.not-found");
    }

    [Fact]
    public async Task GetAllAsync_should_return_success_with_active_sectors_only()
    {
        var store = CreateStore();
        await AddSectorAsync(s => { s.Name = "Tech"; s.IsActive = true; });
        await AddSectorAsync(s => { s.Name = "Health"; s.IsActive = false; });

        var result = await store.GetAllAsync();

        var sectors = result.ShouldSucceed();
        sectors.Should().HaveCount(1);
        sectors[0].Name.Should().Be("Tech");
    }

    [Fact]
    public async Task GetAllSectorsAsync_should_return_success_and_filter_by_active_state()
    {
        var store = CreateStore();
        await AddSectorAsync(s => { s.Name = "Tech"; s.IsActive = true; });
        await AddSectorAsync(s => { s.Name = "Health"; s.IsActive = false; });

        var activeResult = await store.GetAllSectorsAsync(TriState.True);
        var inactiveResult = await store.GetAllSectorsAsync(TriState.False);
        var allResult = await store.GetAllSectorsAsync(TriState.Any);

        activeResult.ShouldSucceed().Should().HaveCount(1).And.Contain(s => s.Name == "Tech");
        inactiveResult.ShouldSucceed().Should().HaveCount(1).And.Contain(s => s.Name == "Health");
        allResult.ShouldSucceed().Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllSectorsAsync_should_return_success_and_filter_by_search_term()
    {
        var store = CreateStore();
        await AddSectorAsync(s => s.Name = "Technology");
        await AddSectorAsync(s => s.Name = "Healthcare");

        var result = await store.GetAllSectorsAsync(TriState.Any, "Tech");

        var sectors = result.ShouldSucceed();
        sectors.Should().HaveCount(1);
        sectors[0].Name.Should().Be("Technology");
    }

    [Fact]
    public async Task SaveAsync_should_return_success_for_new_and_existing_sectors()
    {
        var store = CreateStore();

        var createResult = await store.SaveAsync(new MoreSpeakers.Domain.Models.Sector
        {
            Name = "Tech",
            Slug = "tech",
            IsActive = true
        });

        var created = createResult.ShouldSucceed();
        created.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be("Tech");

        created.Name = "Technology";
        var updateResult = await store.SaveAsync(created);

        var updated = updateResult.ShouldSucceed();
        updated.Id.Should().Be(created.Id);
        updated.Name.Should().Be("Technology");
    }

    [Fact]
    public async Task SaveAsync_should_bubble_exceptions_for_disposed_context()
    {
        var store = CreateStore();
        await Context.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            store.SaveAsync(new MoreSpeakers.Domain.Models.Sector { Name = "Tech", IsActive = true }));
    }

    [Fact]
    public async Task DeleteAsync_by_id_should_return_success_for_existing_sector_and_not_found_for_missing()
    {
        var store = CreateStore();
        var sector = await AddSectorAsync(s => s.Name = "Tech");

        var successResult = await store.DeleteAsync(sector.Id);
        var missingResult = await store.DeleteAsync(999);

        successResult.ShouldSucceed();
        missingResult.ShouldFail("sector.delete.not-found");
    }

    [Fact]
    public async Task DeleteAsync_by_entity_should_return_success_for_existing_sector()
    {
        var store = CreateStore();
        var sector = await AddSectorAsync(s => s.Name = "Tech");

        var result = await store.DeleteAsync(new MoreSpeakers.Domain.Models.Sector { Id = sector.Id, Name = sector.Name });

        result.ShouldSucceed();
    }

    [Fact]
    public async Task DeleteAsync_should_fail_when_sector_has_categories()
    {
        var store = CreateStore();
        var sector = await AddSectorAsync(s => s.Name = "Tech");
        await AddCategoryAsync(sector, c => c.Name = "Platforms");

        var result = await store.DeleteAsync(sector.Id);

        result.ShouldFail("sector.delete.has-categories");
    }

    [Fact]
    public async Task DeleteAsync_should_bubble_exceptions_for_disposed_context()
    {
        var store = CreateStore();
        await Context.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => store.DeleteAsync(1));
    }
}
