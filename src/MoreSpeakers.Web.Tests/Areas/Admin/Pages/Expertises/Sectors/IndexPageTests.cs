using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;
using MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Sectors;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Expertises.Sectors;

public class IndexPageTests
{
    [Fact]
    public async Task OnGet_should_list_all_when_no_filters()
    {
        // Manager is responsible for ordering (DisplayOrder then Name). Return in expected order: Id 2 (order 1) then Id 1 (order 2)
        var items = new List<Sector>
        {
            new() { Id = 2, Name = "B", DisplayOrder = 1, IsActive = false },
            new() { Id = 1, Name = "A", DisplayOrder = 2, IsActive = true },
        };
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAllSectorsAsync(TriState.Any, It.Is<string?>(s => s == null), false))
            .ReturnsAsync(items);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object)
        {
            Q = null,
            Status = TriState.Any
        };

        await page.OnGet();

        page.Items.Should().HaveCount(2);
        // Order by DisplayOrder then Name
        page.Items.Select(s => s.Id).Should().ContainInOrder(2, 1);
        manager.Verify(m => m.GetAllSectorsAsync(TriState.Any, It.Is<string?>(s => s == null), false), Times.Once);
    }

    [Fact]
    public async Task OnGet_should_filter_by_query_and_status()
    {
        var items = new List<Sector>
        {
            new() { Id = 1, Name = "Tech", DisplayOrder = 5, IsActive = true },
            new() { Id = 2, Name = "FinTech", DisplayOrder = 1, IsActive = false },
            new() { Id = 3, Name = "Healthcare", DisplayOrder = 2, IsActive = true },
        };
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAllSectorsAsync(TriState.True, "tech", false))
            .ReturnsAsync(items.Where(s => s.IsActive && s.Name.Contains("tech", StringComparison.OrdinalIgnoreCase)).ToList());
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object) { Q = "tech", Status = TriState.True };

        await page.OnGet();

        page.Items.Should().HaveCount(1);
        page.Items[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task OnGet_should_call_manager_with_expected_parameters()
    {
        var manager = new Mock<ISectorManager>();
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object) { Q = "alpha", Status = TriState.False };

        manager.Setup(m => m.GetAllSectorsAsync(TriState.False, "alpha", false))
            .ReturnsAsync(new List<Sector>());

        await page.OnGet();

        manager.Verify(m => m.GetAllSectorsAsync(TriState.False, "alpha", false), Times.Once);
    }

    [Fact]
    public async Task OnPostActivateAsync_should_activate_and_redirect_with_query()
    {
        var sector = new Sector { Id = 10, Name = "Energy", IsActive = false };
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(10)).ReturnsAsync(sector);
        manager.Setup(m => m.SaveAsync(It.IsAny<Sector>())).ReturnsAsync((Sector s) => s);
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
        sector.IsActive.Should().BeTrue();
        manager.Verify(m => m.SaveAsync(It.Is<Sector>(s => s.Id == 10 && s.IsActive)), Times.Once);
    }

    [Fact]
    public async Task OnPostActivateAsync_when_sector_not_found_redirects_without_route_values()
    {
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(99)).ReturnsAsync((Sector?)null);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object);

        var result = await page.OnPostActivateAsync(99);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.RouteValues.Should().BeNull();
        manager.Verify(m => m.SaveAsync(It.IsAny<Sector>()), Times.Never);
    }

    [Fact]
    public async Task OnPostDeactivateAsync_should_deactivate_and_redirect_with_query()
    {
        var sector = new Sector { Id = 11, Name = "Retail", IsActive = true };
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(11)).ReturnsAsync(sector);
        manager.Setup(m => m.SaveAsync(It.IsAny<Sector>())).ReturnsAsync((Sector s) => s);
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
        sector.IsActive.Should().BeFalse();
        manager.Verify(m => m.SaveAsync(It.Is<Sector>(s => s.Id == 11 && !s.IsActive)), Times.Once);
    }
}
