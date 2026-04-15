using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public class IndexPageTests
{
    [Fact]
    public async Task OnGet_should_list_all_when_manager_returns_success()
    {
        var items = new List<Expertise> { new() { Id = 2, Name = "Beta" }, new() { Id = 1, Name = "Alpha" } };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllExpertisesAsync(TriState.Any, null)).ReturnsAsync(Result.Success(items));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>());

        await page.OnGet(q: null);

        page.Should().BeAssignableTo<PageModel>();
        page.Items.Select(e => e.Name).Should().ContainInOrder("Beta", "Alpha");
    }

    [Fact]
    public async Task OnGet_should_clear_items_when_manager_returns_failure()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllExpertisesAsync(TriState.Any, null))
            .ReturnsAsync(Result.Failure<List<Expertise>>(new Error("expertise.filtered-list.failed", "Load failed.")));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>());

        await page.OnGet(q: null);

        page.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task OnPostActivateAsync_should_activate_and_redirect_when_save_succeeds()
    {
        var expertise = new Expertise { Id = 4, Name = "Cloud", IsActive = false };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAsync(4)).ReturnsAsync(expertise);
        manager.Setup(m => m.SaveAsync(It.IsAny<Expertise>())).ReturnsAsync((Expertise e) => Result.Success(e));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>()) { Q = "cl", Status = TriState.False };

        var result = await page.OnPostActivateAsync(4);

        result.Should().BeOfType<RedirectToPageResult>();
        expertise.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostDeactivateAsync_should_redirect_when_lookup_fails()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAsync(5))
            .ReturnsAsync(Result.Failure<Expertise>(new Error("expertise.not-found", "Missing expertise.")));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>());

        var result = await page.OnPostDeactivateAsync(5);

        result.Should().BeOfType<RedirectToPageResult>();
        manager.Verify(m => m.SaveAsync(It.IsAny<Expertise>()), Times.Never);
    }

    [Fact]
    public async Task OnPostDeactivateAsync_should_redirect_when_save_fails()
    {
        var expertise = new Expertise { Id = 6, Name = "AI", IsActive = true };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAsync(6)).ReturnsAsync(expertise);
        manager.Setup(m => m.SaveAsync(It.IsAny<Expertise>()))
            .ReturnsAsync(Result.Failure<Expertise>(new Error("expertise.save.failed", "Save failed.")));
        var page = new IndexModel(manager.Object, Mock.Of<ILogger<IndexModel>>()) { Q = "ai", Status = TriState.True };

        var result = await page.OnPostDeactivateAsync(6);

        result.Should().BeOfType<RedirectToPageResult>();
    }
}
