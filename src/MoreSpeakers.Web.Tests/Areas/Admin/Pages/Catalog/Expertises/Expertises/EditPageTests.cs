using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public class EditPageTests
{
    [Fact]
    public async Task OnGetAsync_should_load_entity_when_results_succeed()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAsync(1)).ReturnsAsync(new Expertise { Id = 1, Name = "X", ExpertiseCategoryId = 2, IsActive = true });
        manager.Setup(m => m.GetAllCategoriesAsync()).ReturnsAsync(Result.Success(new List<ExpertiseCategory> { new() { Id = 2, Name = "Cat" } }));
        manager.Setup(m => m.GetCategoryAsync(2)).ReturnsAsync(new ExpertiseCategory { Id = 2, Name = "Cat", SectorId = 10 });
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync([new Sector { Id = 10, Name = "S" }]);
        var page = new EditModel(manager.Object, sectorManager.Object, Mock.Of<ILogger<EditModel>>()) { Id = 1 };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        page.Entity.Should().NotBeNull();
        page.Input.Name.Should().Be("X");
        page.SectorId.Should().Be(10);
    }

    [Fact]
    public async Task OnGetAsync_should_redirect_when_lookup_fails()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAsync(9))
            .ReturnsAsync(Result.Failure<Expertise>(new Error("expertise.not-found", "Missing expertise.")));
        var page = new EditModel(manager.Object, Mock.Of<ISectorManager>(), Mock.Of<ILogger<EditModel>>()) { Id = 9 };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task OnPostAsync_should_validate_and_save_then_redirect()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAsync(3)).ReturnsAsync(new Expertise { Id = 3, Name = "Old", ExpertiseCategoryId = 2 });
        manager.Setup(m => m.SaveAsync(It.IsAny<Expertise>())).ReturnsAsync((Expertise e) => Result.Success(e));
        var page = new EditModel(manager.Object, Mock.Of<ISectorManager>(), Mock.Of<ILogger<EditModel>>())
        {
            Id = 3,
            Input = new EditModel.InputModel { Name = "New", Description = "Desc", ExpertiseCategoryId = 5 }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        manager.Verify(m => m.SaveAsync(It.Is<Expertise>(e => e.Id == 3 && e.Name == "New" && e.ExpertiseCategoryId == 5)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_should_return_page_when_save_fails()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAsync(3)).ReturnsAsync(new Expertise { Id = 3, Name = "Old", ExpertiseCategoryId = 2 });
        manager.Setup(m => m.SaveAsync(It.IsAny<Expertise>()))
            .ReturnsAsync(Result.Failure<Expertise>(new Error("expertise.save.failed", "Save failed.")));
        manager.Setup(m => m.GetAllCategoriesAsync()).ReturnsAsync(Result.Success(new List<ExpertiseCategory> { new() { Id = 2, Name = "Cat" } }));
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync([new Sector { Id = 10, Name = "S" }]);
        var page = new EditModel(manager.Object, sectorManager.Object, Mock.Of<ILogger<EditModel>>())
        {
            Id = 3,
            Input = new EditModel.InputModel { Name = "New", ExpertiseCategoryId = 5 }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        page.ModelState[string.Empty]!.Errors.Should().ContainSingle(e => e.ErrorMessage == "Save failed.");
        page.ExpertiseCategories.Should().ContainSingle();
    }
}
