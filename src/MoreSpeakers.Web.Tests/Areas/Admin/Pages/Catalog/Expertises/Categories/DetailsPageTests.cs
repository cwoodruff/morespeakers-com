using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Categories;

public class DetailsPageTests
{
    [Fact]
    public async Task OnGetAsync_should_redirect_to_Index_when_not_found()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetCategoryAsync(123)).ReturnsAsync((ExpertiseCategory?)null);
        var sectorManager = new Mock<ISectorManager>();

        var page = new DetailsModel(expertiseManager.Object, sectorManager.Object)
        {
            Id = 123
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_should_load_category_sector_and_expertises_and_return_Page()
    {
        var category = new ExpertiseCategory { Id = 5, Name = "Data", SectorId = 7 };
        var sector = new Sector { Id = 7, Name = "Tech" };
        var expertises = new List<Expertise>
        {
            new() { Id = 1, Name = "Analytics" },
            new() { Id = 2, Name = "ML" },
        };

        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetCategoryAsync(5)).ReturnsAsync(category);
        expertiseManager.Setup(m => m.GetByCategoryIdAsync(5)).ReturnsAsync(expertises);
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAsync(7)).ReturnsAsync(sector);

        var page = new DetailsModel(expertiseManager.Object, sectorManager.Object)
        {
            Id = 5
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        page.Category.Should().NotBeNull();
        page.Sector.Should().NotBeNull();
        page.Expertises.Should().HaveCount(2);
        page.Sector!.Name.Should().Be("Tech");
        page.Expertises.Select(e => e.Name).Should().Contain(["Analytics", "ML"]);
    }
}
