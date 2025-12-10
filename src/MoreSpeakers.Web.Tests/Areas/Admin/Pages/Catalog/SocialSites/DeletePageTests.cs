using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;
using DomainSite = MoreSpeakers.Domain.Models.SocialMediaSite;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.SocialSites;

public class DeletePageTests
{

    [Fact]
    public async Task OnGetAsync_should_redirect_to_index_when_site_not_found()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.GetAsync(10))!.ReturnsAsync((DomainSite?)null);
        var page = new DeleteModel(manager.Object);

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
        manager.Setup(m => m.RefCountAsync(5)).ReturnsAsync(2);

        var page = new DeleteModel(manager.Object);

        var result = await page.OnGetAsync(5);

        result.Should().BeOfType<PageResult>();
        page.Site.Should().NotBeNull();
        page.Site!.Id.Should().Be(5);
        page.ReferenceCount.Should().Be(2);
        manager.Verify(m => m.RefCountAsync(5), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_should_redirect_back_to_delete_when_in_use()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.InUseAsync(7)).ReturnsAsync(true);

        var page = new DeleteModel(manager.Object) { Id = 7 };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Delete");
        manager.Verify(m => m.DeleteAsync(It.IsAny<int>()), Times.Never);
        manager.Verify(m => m.InUseAsync(7), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_should_delete_and_redirect_when_not_in_use()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.InUseAsync(7)).ReturnsAsync(false);
        manager.Setup(m => m.DeleteAsync(7)).ReturnsAsync(true);

        var page = new DeleteModel(manager.Object) { Id = 7 };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        manager.Verify(m => m.InUseAsync(7), Times.Once);
        manager.Verify(m => m.DeleteAsync(7), Times.Once);
    }
}
