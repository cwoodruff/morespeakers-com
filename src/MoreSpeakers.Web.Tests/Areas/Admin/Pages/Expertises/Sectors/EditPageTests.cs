using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Sectors;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Expertises.Sectors;

public class EditPageTests
{
    [Fact]
    public async Task OnGetAsync_should_redirect_to_Index_when_not_found()
    {
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(999)).ReturnsAsync((Sector?)null);
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(manager.Object, logger.Object)
        {
            Id = 999
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_should_load_sector_into_Input_and_return_Page()
    {
        var sector = new Sector
        {
            Id = 5,
            Name = "Tech",
            Slug = "tech",
            Description = "desc",
            DisplayOrder = 7,
            IsActive = true
        };
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(5)).ReturnsAsync(sector);
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(manager.Object, logger.Object)
        {
            Id = 5
        };

        var result = await page.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        page.Input.Name.Should().Be("Tech");
        page.Input.Slug.Should().Be("tech");
        page.Input.Description.Should().Be("desc");
        page.Input.DisplayOrder.Should().Be(7);
        page.Input.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_should_return_Page_when_model_invalid()
    {
        var manager = new Mock<ISectorManager>();
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(manager.Object, logger.Object)
        {
            Id = 3
        };
        page.ModelState.AddModelError("Input.Name", "Required");

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        manager.Verify(m => m.SaveAsync(It.IsAny<Sector>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_should_redirect_to_Index_when_sector_not_found()
    {
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(42)).ReturnsAsync((Sector?)null);
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(manager.Object, logger.Object)
        {
            Id = 42,
            Input = new EditModel.InputModel { Name = "X" }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        manager.Verify(m => m.SaveAsync(It.IsAny<Sector>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_should_update_fields_trim_and_redirect()
    {
        var existing = new Sector
        {
            Id = 8,
            Name = "Old",
            Slug = null,
            Description = null,
            DisplayOrder = 1,
            IsActive = false
        };
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.GetAsync(8)).ReturnsAsync(existing);
        manager.Setup(m => m.SaveAsync(It.IsAny<Sector>())).ReturnsAsync((Sector s) => s);
        var logger = new Mock<ILogger<EditModel>>();
        var page = new EditModel(manager.Object, logger.Object)
        {
            Id = 8,
            Input = new EditModel.InputModel
            {
                Name = "  NewName  ",
                Slug = "  new  ",
                Description = "  d  ",
                DisplayOrder = 9,
                IsActive = true
            }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        existing.Name.Should().Be("NewName");
        existing.Slug.Should().Be("new");
        existing.Description.Should().Be("d");
        existing.DisplayOrder.Should().Be(9);
        existing.IsActive.Should().BeTrue();
        manager.Verify(m => m.SaveAsync(It.Is<Sector>(s => s.Id == 8 && s.Name == "NewName")), Times.Once);
    }
}
