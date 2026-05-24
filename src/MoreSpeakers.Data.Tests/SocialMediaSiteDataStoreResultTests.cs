using Bogus;
using FluentAssertions;

namespace MoreSpeakers.Data.Tests;

public class SocialMediaSiteDataStoreResultTests : DataStoreTestBase
{
    private SocialMediaSiteDataStore CreateStore() => new(Context, Mapper, CreateLogger<SocialMediaSiteDataStore>().Object);

    private async Task<Models.SocialMediaSite> AddSocialMediaSiteAsync(string name = "Twitter", string urlFormat = "https://twitter.com/{handle}")
    {
        var site = new Models.SocialMediaSite
        {
            Name = name,
            Icon = "fa-twitter",
            UrlFormat = urlFormat
        };
        Context.SocialMediaSite.Add(site);
        await Context.SaveChangesAsync(CancellationToken);
        return site;
    }

    [Fact]
    public async Task GetAsync_should_return_success_for_existing_site_and_failure_for_missing()
    {
        var store = CreateStore();
        var site = await AddSocialMediaSiteAsync("Twitter");

        var success = await store.GetAsync(site.Id);
        var failure = await store.GetAsync(999);

        success.ShouldSucceed().Name.Should().Be("Twitter");
        failure.ShouldFail("social-media-site.not-found");
    }

    [Fact]
    public async Task GetAllAsync_should_return_success_with_all_sites_ordered_by_name()
    {
        var store = CreateStore();
        await AddSocialMediaSiteAsync("Twitter");
        await AddSocialMediaSiteAsync("LinkedIn");
        await AddSocialMediaSiteAsync("GitHub");

        var result = await store.GetAllAsync();

        var sites = result.ShouldSucceed();
        sites.Should().HaveCount(3);
        sites[0].Name.Should().Be("GitHub");
        sites[1].Name.Should().Be("LinkedIn");
        sites[2].Name.Should().Be("Twitter");
    }

    [Fact]
    public async Task SaveAsync_should_return_success_for_new_and_existing_sites()
    {
        var store = CreateStore();

        var createResult = await store.SaveAsync(new MoreSpeakers.Domain.Models.SocialMediaSite
        {
            Name = "Twitter",
            Icon = "fa-twitter",
            UrlFormat = "https://twitter.com/{handle}"
        });

        var created = createResult.ShouldSucceed();
        created.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be("Twitter");

        created.Name = "X (Twitter)";
        var updateResult = await store.SaveAsync(created);

        var updated = updateResult.ShouldSucceed();
        updated.Id.Should().Be(created.Id);
        updated.Name.Should().Be("X (Twitter)");
    }

    [Fact]
    public async Task SaveAsync_should_bubble_exceptions_for_disposed_context()
    {
        var store = CreateStore();
        await Context.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            store.SaveAsync(new MoreSpeakers.Domain.Models.SocialMediaSite
            {
                Name = "Twitter",
                Icon = "fa-twitter",
                UrlFormat = "https://twitter.com/{handle}"
            }));
    }

    [Fact]
    public async Task DeleteAsync_by_id_should_return_success_for_existing_site_and_not_found_for_missing()
    {
        var store = CreateStore();
        var site = await AddSocialMediaSiteAsync("Twitter");

        var successResult = await store.DeleteAsync(site.Id);
        var missingResult = await store.DeleteAsync(999);

        successResult.ShouldSucceed();
        missingResult.ShouldFail("social-media-site.delete.not-found");
    }

    [Fact]
    public async Task DeleteAsync_by_entity_should_return_success_for_existing_site()
    {
        var store = CreateStore();
        var site = await AddSocialMediaSiteAsync("Twitter");

        var result = await store.DeleteAsync(new MoreSpeakers.Domain.Models.SocialMediaSite
        {
            Id = site.Id,
            Name = site.Name,
            Icon = site.Icon,
            UrlFormat = site.UrlFormat
        });

        result.ShouldSucceed();
    }

    [Fact]
    public async Task DeleteAsync_should_bubble_exceptions_for_disposed_context()
    {
        var store = CreateStore();
        await Context.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => store.DeleteAsync(1));
    }

    [Fact]
    public async Task RefCountAsync_should_return_success_with_reference_count()
    {
        var store = CreateStore();
        var site = await AddSocialMediaSiteAsync("Twitter");
        var user = await AddUserAsync();

        // Add a user social media site reference
        Context.UserSocialMediaSite.Add(new Models.UserSocialMediaSites
        {
            UserId = user.Id,
            SocialMediaSiteId = site.Id,
            SocialId = "testuser"
        });
        await Context.SaveChangesAsync(CancellationToken);

        var result = await store.RefCountAsync(site.Id);

        result.ShouldSucceed().Should().Be(1);
    }

    [Fact]
    public async Task InUseAsync_should_return_success_with_true_when_site_has_references()
    {
        var store = CreateStore();
        var site = await AddSocialMediaSiteAsync("Twitter");
        var user = await AddUserAsync();

        Context.UserSocialMediaSite.Add(new Models.UserSocialMediaSites
        {
            UserId = user.Id,
            SocialMediaSiteId = site.Id,
            SocialId = "testuser"
        });
        await Context.SaveChangesAsync(CancellationToken);

        var result = await store.InUseAsync(site.Id);

        result.ShouldSucceed().Should().BeTrue();
    }

    [Fact]
    public async Task InUseAsync_should_return_success_with_false_when_site_has_no_references()
    {
        var store = CreateStore();
        var site = await AddSocialMediaSiteAsync("Twitter");

        var result = await store.InUseAsync(site.Id);

        result.ShouldSucceed().Should().BeFalse();
    }
}
