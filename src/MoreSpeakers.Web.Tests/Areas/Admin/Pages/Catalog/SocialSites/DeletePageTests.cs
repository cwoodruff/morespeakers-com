using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoreSpeakers.Data;
using MoreSpeakers.Data.Models;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;
using DomainSite = MoreSpeakers.Domain.Models.SocialMediaSite;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.SocialSites;

public class DeletePageTests
{
    private static MoreSpeakersDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<MoreSpeakersDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;
        return new MoreSpeakersDbContext(options);
    }

    [Fact]
    public async Task OnGetAsync_should_redirect_to_index_when_site_not_found()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.GetAsync(10)).ReturnsAsync((DomainSite?)null);
        await using var db = CreateDb(nameof(OnGetAsync_should_redirect_to_index_when_site_not_found));
        var page = new DeleteModel(manager.Object, db);

        var result = await page.OnGetAsync(10);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_should_load_site_and_reference_count()
    {
        var site = new DomainSite { Id = 5, Name = "Apple", Icon = "fa-brands fa-apple", UrlFormat = "https://apple.com/{handle}" };
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.GetAsync(5)).ReturnsAsync(site);
        await using var db = CreateDb(nameof(OnGetAsync_should_load_site_and_reference_count));

        // Seed references
        db.UserSocialMediaSite.Add(new UserSocialMediaSites { Id = 1, SocialMediaSiteId = 5, UserId = Guid.NewGuid(), SocialId = "abc" });
        db.UserSocialMediaSite.Add(new UserSocialMediaSites { Id = 2, SocialMediaSiteId = 5, UserId = Guid.NewGuid(), SocialId = "def" });
        await db.SaveChangesAsync();

        var page = new DeleteModel(manager.Object, db);

        var result = await page.OnGetAsync(5);

        result.Should().BeOfType<PageResult>();
        page.Site.Should().NotBeNull();
        page.Site!.Id.Should().Be(5);
        page.ReferenceCount.Should().Be(2);
    }

    [Fact]
    public async Task OnPostAsync_should_redirect_back_to_delete_when_in_use()
    {
        var manager = new Mock<ISocialMediaSiteManager>(MockBehavior.Strict);
        await using var db = CreateDb(nameof(OnPostAsync_should_redirect_back_to_delete_when_in_use));
        // Seed one reference
        db.UserSocialMediaSite.Add(new UserSocialMediaSites { Id = 1, SocialMediaSiteId = 7, UserId = Guid.NewGuid(), SocialId = "h" });
        await db.SaveChangesAsync();

        var page = new DeleteModel(manager.Object, db) { Id = 7 };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Delete");
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OnPostAsync_should_delete_and_redirect_when_not_in_use()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.DeleteAsync(7)).ReturnsAsync(true);
        await using var db = CreateDb(nameof(OnPostAsync_should_delete_and_redirect_when_not_in_use));

        var page = new DeleteModel(manager.Object, db) { Id = 7 };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        manager.Verify(m => m.DeleteAsync(7), Times.Once);
    }
}
