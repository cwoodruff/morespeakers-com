using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.SocialSites;

public class EditPageTests
{
    [Fact]
    public async Task OnGetAsync_should_redirect_to_index_when_not_found()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.GetAsync(123))!.ReturnsAsync((SocialMediaSite?)null);
        var page = new EditModel(manager.Object);

        var result = await page.OnGetAsync(123);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_should_load_entity_into_Form_and_return_Page()
    {
        var site = new SocialMediaSite { Id = 5, Name = "Apple", Icon = "fa-brands fa-apple", UrlFormat = "https://apple.com/{handle}" };
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.GetAsync(5)).ReturnsAsync(site);
        var page = new EditModel(manager.Object);

        var result = await page.OnGetAsync(5);

        result.Should().BeOfType<PageResult>();
        page.Form.Id.Should().Be(5);
        page.Form.Name.Should().Be("Apple");
        page.Form.Icon.Should().Be("fa-brands fa-apple");
        page.Form.UrlFormat.Should().Be("https://apple.com/{handle}");
    }

    [Fact]
    public async Task OnPostAsync_should_return_Page_when_urlformat_invalid()
    {
        var manager = new Mock<ISocialMediaSiteManager>(MockBehavior.Strict);
        var page = new EditModel(manager.Object)
        {
            Form = new SocialMediaSite { Id = 7, Name = "AppleXXX", Icon = "fa-brands fa-apple", UrlFormat = "https://apple.com/handle" }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OnPostAsync_should_save_and_redirect_when_valid()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.SaveAsync(It.IsAny<SocialMediaSite>()))
            .ReturnsAsync((SocialMediaSite s) => s);
        var page = new EditModel(manager.Object)
        {
            Form = new SocialMediaSite { Id = 7, Name = "Apple", Icon = "fa-brands fa-apple", UrlFormat = "https://apple.com/{handle}" }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        manager.Verify(m => m.SaveAsync(It.Is<SocialMediaSite>(s => s.Id == 7 && s.Name == "Apple")), Times.Once);
    }
}
