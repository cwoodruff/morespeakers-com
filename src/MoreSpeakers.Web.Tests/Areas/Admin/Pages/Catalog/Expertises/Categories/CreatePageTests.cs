using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Categories;

public class CreatePageTests
{
    [Fact]
    public async Task OnPostAsync_should_return_Page_when_model_invalid()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync([]);
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(expertiseManager.Object, sectorManager.Object, logger.Object);
        page.ModelState.AddModelError("Input.Name", "Required");

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        expertiseManager.Verify(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_should_save_and_redirect_when_valid()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()))
            .ReturnsAsync((ExpertiseCategory c) => { c.Id = 123; return c; });
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllAsync()).ReturnsAsync([new Sector { Id = 1, Name = "Tech" }]);
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(expertiseManager.Object, sectorManager.Object, logger.Object)
        {
            Input = new CreateModel.InputModel
            {
                Name = "  Data  ",
                Description = "  desc ",
                SectorId = 1,
                IsActive = true
            }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        expertiseManager.Verify(m => m.SaveCategoryAsync(It.Is<ExpertiseCategory>(c =>
            c.Name == "Data" && c.Description == "desc" && c.SectorId == 1 && c.IsActive
        )), Times.Once);
    }
}
