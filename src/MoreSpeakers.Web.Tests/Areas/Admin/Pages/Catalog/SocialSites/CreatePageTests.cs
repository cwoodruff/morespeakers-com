using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.SocialSites;

public class CreatePageTests
{
    [Fact]
    public async Task OnPostAsync_should_return_Page_when_urlformat_invalid()
    {
        var manager = new Mock<ISocialMediaSiteManager>(MockBehavior.Strict);
        var page = new CreateModel(manager.Object)
        {
            Form = new SocialMediaSite { Name = "X", Icon = "x", UrlFormat = "https://twitter.com/handle" } // missing {handle}
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OnPostAsync_should_save_and_redirect_when_model_valid()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        manager.Setup(m => m.SaveAsync(It.IsAny<SocialMediaSite>()))
            .ReturnsAsync((SocialMediaSite s) => s);
        var page = new CreateModel(manager.Object)
        {
            Form = new SocialMediaSite { Name = "LinkedIn", Icon = "in", UrlFormat = "https://linkedin.com/in/{handle}" }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        manager.Verify(m => m.SaveAsync(It.Is<SocialMediaSite>(s => s.Name == "LinkedIn")), Times.Once);
    }
}
