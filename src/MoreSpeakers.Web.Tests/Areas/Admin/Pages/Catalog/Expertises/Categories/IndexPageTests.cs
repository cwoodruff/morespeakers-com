using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Categories;

public class IndexPageTests
{
    [Fact]
    public async Task OnGet_should_list_all_when_manager_returns_success()
    {
        var items = new List<ExpertiseCategory>
        {
            new() { Id = 1, Name = "A", IsActive = true, SectorId = 1 },
            new() { Id = 2, Name = "B", IsActive = false, SectorId = 1 },
        };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllCategoriesAsync(TriState.Any, null)).ReturnsAsync(Result.Success(items));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>()) { Status = TriState.Any };

        await page.OnGet();

        page.Items.Select(c => c.Id).Should().ContainInOrder(1, 2);
    }

    [Fact]
    public async Task OnGet_should_clear_items_when_manager_returns_failure()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllCategoriesAsync(TriState.Any, null))
            .ReturnsAsync(Result.Failure<List<ExpertiseCategory>>(new Error("expertise-category.list.failed", "List failed.")));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>()) { Status = TriState.Any };

        await page.OnGet();

        page.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task OnPostActivateAsync_should_activate_and_redirect_with_query()
    {
        var category = new ExpertiseCategory { Id = 10, Name = "Energy", IsActive = false, SectorId = 1 };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetCategoryAsync(10)).ReturnsAsync(category);
        manager.Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>())).ReturnsAsync((ExpertiseCategory c) => Result.Success(c));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>()) { Q = "en", Status = TriState.False };

        var result = await page.OnPostActivateAsync(10);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.RouteValues.Should().Contain(new KeyValuePair<string, object?>("q", "en"))
            .And.Contain(new KeyValuePair<string, object?>("status", TriState.False));
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostActivateAsync_when_category_load_fails_should_not_save()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetCategoryAsync(99))
            .ReturnsAsync(Result.Failure<ExpertiseCategory>(new Error("expertise-category.not-found", "Missing category.")));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>());

        var result = await page.OnPostActivateAsync(99);

        result.Should().BeOfType<RedirectToPageResult>();
        manager.Verify(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()), Times.Never);
    }

    [Fact]
    public async Task OnPostDeactivateAsync_should_deactivate_and_redirect_with_query()
    {
        var category = new ExpertiseCategory { Id = 11, Name = "Retail", IsActive = true, SectorId = 1 };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetCategoryAsync(11)).ReturnsAsync(category);
        manager.Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>())).ReturnsAsync((ExpertiseCategory c) => Result.Success(c));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>()) { Q = "ret", Status = TriState.True };

        var result = await page.OnPostDeactivateAsync(11);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.RouteValues.Should().Contain(new KeyValuePair<string, object?>("q", "ret"))
            .And.Contain(new KeyValuePair<string, object?>("status", TriState.True));
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task OnPostDeactivateAsync_should_redirect_when_save_fails()
    {
        var category = new ExpertiseCategory { Id = 12, Name = "Data", IsActive = true, SectorId = 1 };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetCategoryAsync(12)).ReturnsAsync(category);
        manager.Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()))
            .ReturnsAsync(Result.Failure<ExpertiseCategory>(new Error("expertise-category.save.failed", "Save failed.")));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>()) { Q = "da", Status = TriState.True };

        var result = await page.OnPostDeactivateAsync(12);

        result.Should().BeOfType<RedirectToPageResult>();
    }
}
