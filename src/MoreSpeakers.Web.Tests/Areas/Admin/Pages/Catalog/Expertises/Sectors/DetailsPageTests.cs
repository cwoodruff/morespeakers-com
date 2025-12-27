using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public class DetailsPageTests
{
    [Fact]
    public async Task OnGetAsync_should_redirect_to_Index_when_not_found()
    {
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetSectorWithRelationshipsAsync(123)).ReturnsAsync((Sector?)null);
        var page = new DetailsModel(manager.Object)
        {
            Id = 123
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_should_load_sector_and_return_Page()
    {
        var sector = new Sector { Id = 2, Name = "Finance" };
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetSectorWithRelationshipsAsync(2)).ReturnsAsync(sector);
        var page = new DetailsModel(manager.Object)
        {
            Id = 2
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        page.Sector.Should().NotBeNull();
        page.Sector!.Id.Should().Be(2);
        page.Sector!.Name.Should().Be("Finance");
    }
}
