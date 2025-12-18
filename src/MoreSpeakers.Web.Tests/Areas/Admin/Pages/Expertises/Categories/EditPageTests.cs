using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Categories;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Expertises.Categories;

public class EditPageTests
{
    [Fact]
    public async Task OnGetAsync_should_redirect_to_Index_when_not_found()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetCategoryAsync(999)).ReturnsAsync((ExpertiseCategory?)null);
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<Sector>());
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(expertiseManager.Object, sectorManager.Object, logger.Object)
        {
            Id = 999
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_should_load_category_into_Input_and_return_Page()
    {
        var category = new ExpertiseCategory
        {
            Id = 5,
            Name = "Data",
            Description = "desc",
            SectorId = 2,
            IsActive = true
        };
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetCategoryAsync(5)).ReturnsAsync(category);
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<Sector>());
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(expertiseManager.Object, sectorManager.Object, logger.Object)
        {
            Id = 5
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        page.Input.Name.Should().Be("Data");
        page.Input.Description.Should().Be("desc");
        page.Input.SectorId.Should().Be(2);
        page.Input.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_should_return_Page_when_model_invalid()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<Sector>());
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(expertiseManager.Object, sectorManager.Object, logger.Object)
        {
            Id = 3
        };
        page.ModelState.AddModelError("Input.Name", "Required");

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        expertiseManager.Verify(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_should_redirect_to_Index_when_category_not_found()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetCategoryAsync(42)).ReturnsAsync((ExpertiseCategory?)null);
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<Sector>());
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(expertiseManager.Object, sectorManager.Object, logger.Object)
        {
            Id = 42,
            Input = new EditModel.InputModel { Name = "X" }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        expertiseManager.Verify(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_should_update_fields_trim_and_redirect()
    {
        var existing = new ExpertiseCategory
        {
            Id = 8,
            Name = "Old",
            Description = null,
            SectorId = 1,
            IsActive = false
        };
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetCategoryAsync(8)).ReturnsAsync(existing);
        expertiseManager.Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>())).ReturnsAsync((ExpertiseCategory c) => c);
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<Sector>());
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(expertiseManager.Object, sectorManager.Object, logger.Object)
        {
            Id = 8,
            Input = new EditModel.InputModel
            {
                Name = "  NewName  ",
                Description = "  d  ",
                SectorId = 3,
                IsActive = true
            }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        existing.Name.Should().Be("NewName");
        existing.Description.Should().Be("d");
        existing.SectorId.Should().Be(3);
        existing.IsActive.Should().BeTrue();
        expertiseManager.Verify(m => m.SaveCategoryAsync(It.Is<ExpertiseCategory>(c => c.Id == 8 && c.Name == "NewName")), Times.Once);
    }
}
