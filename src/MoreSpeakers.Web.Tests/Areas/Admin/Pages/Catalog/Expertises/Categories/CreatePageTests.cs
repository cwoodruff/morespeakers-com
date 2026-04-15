using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain;
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
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false)).ReturnsAsync([]);
        var page = new CreateModel(expertiseManager.Object, sectorManager.Object, Mock.Of<ILogger<CreateModel>>());
        page.ModelState.AddModelError("Input.Name", "Required");

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        expertiseManager.Verify(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_should_save_and_redirect_when_valid()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager
            .Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()))
            .ReturnsAsync((ExpertiseCategory c) =>
            {
                c.Id = 123;
                return Result.Success(c);
            });
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync([new Sector { Id = 1, Name = "Tech" }]);
        var page = new CreateModel(expertiseManager.Object, sectorManager.Object, Mock.Of<ILogger<CreateModel>>())
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

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
        expertiseManager.Verify(m => m.SaveCategoryAsync(It.Is<ExpertiseCategory>(c =>
            c.Name == "Data" && c.Description == "desc" && c.SectorId == 1 && c.IsActive)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_should_return_Page_with_error_when_save_fails()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager
            .Setup(m => m.SaveCategoryAsync(It.IsAny<ExpertiseCategory>()))
            .ReturnsAsync(Result.Failure<ExpertiseCategory>(new Error("expertise-category.save.failed", "Save failed.")));
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync([new Sector { Id = 1, Name = "Tech" }]);
        var page = new CreateModel(expertiseManager.Object, sectorManager.Object, Mock.Of<ILogger<CreateModel>>())
        {
            Input = new CreateModel.InputModel { Name = "Data", SectorId = 1 }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        page.ModelState[string.Empty]!.Errors.Should().ContainSingle(e => e.ErrorMessage == "Save failed.");
    }
}
