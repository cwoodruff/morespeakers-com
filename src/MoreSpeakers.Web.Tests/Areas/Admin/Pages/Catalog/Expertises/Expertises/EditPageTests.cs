using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public class EditPageTests
{

    [Fact]
    public async Task OnGetAsync_should_load_entity_or_redirect_when_category_missing()
    {
        var manager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        // Sectors and categories loaded on GET
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync([new Sector { Id = 10, Name = "S" }]);
        manager.Setup(m => m.GetAllCategoriesAsync()).ReturnsAsync([new ExpertiseCategory { Id = 2, Name = "Cat" }]);

        manager.Setup(m => m.GetAsync(1)).ReturnsAsync(new Expertise { Id = 1, Name = "X", ExpertiseCategoryId = 2, IsActive = true });
        manager.Setup(m => m.GetCategoryAsync(2)).ReturnsAsync(new ExpertiseCategory { Id = 2, Name = "Cat", SectorId = 10 });

        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(manager.Object, sectorManager.Object, logger.Object)
        {
            Id = 1
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.RazorPages.PageResult>();
        page.Entity.Should().NotBeNull();

        var page2 = new EditModel(manager.Object, sectorManager.Object, logger.Object) { Id = 9 };
        // Expertise exists but category is missing -> redirect
        manager.Setup(m => m.GetAsync(9)).ReturnsAsync(new Expertise { Id = 9, Name = "Y", ExpertiseCategoryId = 99, IsActive = true });
        manager.Setup(m => m.GetCategoryAsync(99)).ReturnsAsync((ExpertiseCategory?)null);
        var redirect = await page2.OnGetAsync();
        redirect.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task OnPostAsync_should_validate_and_save_then_redirect()
    {
        var manager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(3)).ReturnsAsync(new Expertise { Id = 3, Name = "Old", ExpertiseCategoryId = 2 });
        manager.Setup(m => m.SaveAsync(It.IsAny<Expertise>())).ReturnsAsync((Expertise e) => e);
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(manager.Object, sectorManager.Object, logger.Object)
        {
            Id = 3,
            Input = new EditModel.InputModel { Name = "New", Description = "Desc", ExpertiseCategoryId = 5 }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        manager.Verify(m => m.SaveAsync(It.Is<Expertise>(e => e.Id == 3 && e.Name == "New" && e.ExpertiseCategoryId == 5)), Times.Once);

        page.ModelState.AddModelError("Input.Name", "Required");
        var invalid = await page.OnPostAsync();
        invalid.Should().BeOfType<Microsoft.AspNetCore.Mvc.RazorPages.PageResult>();
    }

    // Note: Partial-rendering handler requires runtime PageContext; we intentionally
    // skip invoking it here to avoid coupling tests to MVC infrastructure.
}
