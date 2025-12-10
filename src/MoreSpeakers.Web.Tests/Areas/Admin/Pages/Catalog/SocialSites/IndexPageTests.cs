using FluentAssertions;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.SocialSites;

public class IndexPageTests
{
    [Fact]
    public async Task OnGet_should_load_all_items_ordered_when_no_query()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        var items = new List<SocialMediaSite>
        {
            new() { Id = 2, Name = "YouTube", Icon = "yt", UrlFormat = "https://youtube.com/@{handle}" },
            new() { Id = 1, Name = "Twitter", Icon = "x", UrlFormat = "https://twitter.com/{handle}" },
            new() { Id = 3, Name = "LinkedIn", Icon = "in", UrlFormat = "https://linkedin.com/in/{handle}" },
        };
        manager.Setup(m => m.GetAllAsync()).ReturnsAsync(items);
        var page = new IndexModel(manager.Object);

        await page.OnGet();

        var names = page.Items.Select(x => x.Name).ToArray();
        names.Should().Equal("LinkedIn", "Twitter", "YouTube");
        manager.Verify(m => m.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task OnGet_should_filter_by_query_case_insensitive_and_order()
    {
        var manager = new Mock<ISocialMediaSiteManager>();
        var items = new List<SocialMediaSite>
        {
            new() { Id = 1, Name = "Mastodon", Icon = "mstdn", UrlFormat = "https://mastodon.social/@{handle}" },
            new() { Id = 2, Name = "Meta (Facebook)", Icon = "fb", UrlFormat = "https://facebook.com/{handle}" },
            new() { Id = 3, Name = "Meta (Instagram)", Icon = "ig", UrlFormat = "https://instagram.com/{handle}" },
        };
        manager.Setup(m => m.GetAllAsync()).ReturnsAsync(items);
        var page = new IndexModel(manager.Object)
        {
            Q = "meta"
        };

        await page.OnGet();

        page.Items.Should().HaveCount(2);
        page.Items.Select(x => x.Name).Should().Equal("Meta (Facebook)", "Meta (Instagram)");
        manager.Verify(m => m.GetAllAsync(), Times.Once);
    }
}
