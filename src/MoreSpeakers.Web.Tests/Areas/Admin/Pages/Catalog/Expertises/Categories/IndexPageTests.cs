using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Categories;

public class IndexPageTests
{
    [Fact]
    public async Task OnGet_should_list_all_when_no_filters()
    {
        var items = new List<ExpertiseCategory>
        {
            new() { Id = 1, Name = "A", IsActive = true, SectorId = 1 },
            new() { Id = 2, Name = "B", IsActive = false, SectorId = 1 },
        };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllCategoriesAsync(It.IsAny<TriState>(), It.IsAny<string?>())).ReturnsAsync(items);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object)
        {
            Q = null,
            Status = TriState.Any
        };

        await page.OnGet();

        page.Items.Should().HaveCount(2);
        page.Items.Select(c => c.Id).Should().ContainInOrder(1, 2);
        manager.Verify(m => m.GetAllCategoriesAsync(TriState.Any, It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task OnGet_should_filter_by_query_and_status()
    {
        var items = new List<ExpertiseCategory>
        {
            new() { Id = 1, Name = "Tech", IsActive = true, SectorId = 1 },
            new() { Id = 2, Name = "FinTech", IsActive = false, SectorId = 1 },
            new() { Id = 3, Name = "Healthcare", IsActive = true, SectorId = 2 },
        };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllCategoriesAsync(TriState.True, "tech")).ReturnsAsync(new List<ExpertiseCategory> { items[0] });
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object)
        {
            Q = "tech",
            Status = TriState.True
        };

        await page.OnGet();

        page.Items.Should().HaveCount(1);
        page.Items[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task OnPostActivateAsync_should_activate_and_redirect_with_query()
    {
        var category = new ExpertiseCategory { Id = 10, Name = "Energy", IsActive = false, SectorId = 1 };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetCategoryAsync(10)).ReturnsAsync(category);
        manager.Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>())).ReturnsAsync((ExpertiseCategory c) => c);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object)
        {
            Q = "en",
            Status = TriState.False
        };

        var result = await page.OnPostActivateAsync(10);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.RouteValues.Should().Contain(new KeyValuePair<string, object?>("q", "en"))
            .And.Contain(new KeyValuePair<string, object?>("status", TriState.False));
        category.IsActive.Should().BeTrue();
        manager.Verify(m => m.SaveCategoryAsync(It.Is<ExpertiseCategory>(c => c.Id == 10 && c.IsActive)), Times.Once);
    }

    [Fact]
    public async Task OnPostActivateAsync_when_category_not_found_redirects_without_route_values()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetCategoryAsync(99)).ReturnsAsync((ExpertiseCategory?)null);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object);

        var result = await page.OnPostActivateAsync(99);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.RouteValues.Should().BeNull();
        manager.Verify(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()), Times.Never);
    }

    [Fact]
    public async Task OnPostDeactivateAsync_should_deactivate_and_redirect_with_query()
    {
        var category = new ExpertiseCategory { Id = 11, Name = "Retail", IsActive = true, SectorId = 1 };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetCategoryAsync(11)).ReturnsAsync(category);
        manager.Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>())).ReturnsAsync((ExpertiseCategory c) => c);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object)
        {
            Q = "ret",
            Status = TriState.True
        };

        var result = await page.OnPostDeactivateAsync(11);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.RouteValues.Should().Contain(new KeyValuePair<string, object?>("q", "ret"))
            .And.Contain(new KeyValuePair<string, object?>("status", TriState.True));
        category.IsActive.Should().BeFalse();
        manager.Verify(m => m.SaveCategoryAsync(It.Is<ExpertiseCategory>(c => c.Id == 11 && !c.IsActive)), Times.Once);
    }
}
