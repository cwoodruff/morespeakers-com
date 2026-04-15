using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;
using MoreSpeakers.Web.Models.ViewModels;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public class CreatePageTests
{
    private static CreateModel CreatePageModel(Mock<IExpertiseManager> expertiseManager, Mock<ISectorManager> sectorManager)
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        var pageContext = new PageContext(actionContext) { ViewData = viewData };

        return new CreateModel(expertiseManager.Object, sectorManager.Object, Mock.Of<ILogger<CreateModel>>())
        {
            PageContext = pageContext,
            MetadataProvider = modelMetadataProvider
        };
    }

    [Fact]
    public async Task OnGetAsync_should_load_sectors()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync([new Sector { Id = 1, Name = "Sector 1" }]);
        var page = CreatePageModel(expertiseManager, sectorManager);

        await page.OnGetAsync();

        page.Sectors.Should().ContainSingle().Which.Name.Should().Be("Sector 1");
    }

    [Fact]
    public async Task OnPostAsync_should_return_page_when_invalid_and_reload_sectors()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync([new Sector { Id = 1, Name = "Sector 1" }]);
        var page = CreatePageModel(expertiseManager, sectorManager);
        page.ModelState.AddModelError("Input.Name", "Required");

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        page.Sectors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task OnPostAsync_should_redirect_to_index_when_save_succeeds()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.SaveAsync(It.IsAny<Expertise>()))
            .ReturnsAsync((Expertise e) => { e.Id = 42; return Result.Success(e); });
        var sectorManager = new Mock<ISectorManager>();
        var page = CreatePageModel(expertiseManager, sectorManager);
        page.Input = new CreateModel.InputModel { Name = "NewExp", Description = "Desc", SectorId = 1, ExpertiseCategoryId = 1 };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnPostAsync_should_return_page_when_save_fails()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.SaveAsync(It.IsAny<Expertise>()))
            .ReturnsAsync(Result.Failure<Expertise>(new Error("expertise.save.failed", "Save failed.")));
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync([new Sector { Id = 1, Name = "Sector 1" }]);
        var page = CreatePageModel(expertiseManager, sectorManager);
        page.Input = new CreateModel.InputModel { Name = "NewExp", SectorId = 1, ExpertiseCategoryId = 1 };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        page.ModelState[string.Empty]!.Errors.Should().ContainSingle(e => e.ErrorMessage == "Save failed.");
    }

    [Fact]
    public async Task OnGetCategoriesBySector_should_return_partial_with_categories()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetAllActiveCategoriesForSector(1))
            .ReturnsAsync(Result.Success(new List<ExpertiseCategory> { new() { Id = 1, Name = "Category 1" } }));
        var page = CreatePageModel(expertiseManager, new Mock<ISectorManager>());

        var result = await page.OnGetCategoriesBySector(1);

        result.Should().BeOfType<PartialViewResult>();
        var partial = (PartialViewResult)result;
        partial.ViewName.Should().Be("_ExpertiseCategorySelectItem");
        ((ExpertiseCategoryDropDownViewModel)partial.Model!).ExpertiseCategories.Should().ContainSingle();
    }

    [Fact]
    public async Task OnGetCategoriesBySector_should_return_bad_request_content_when_lookup_fails()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        expertiseManager.Setup(m => m.GetAllActiveCategoriesForSector(1))
            .ReturnsAsync(Result.Failure<List<ExpertiseCategory>>(new Error("expertise-category.by-sector.failed", "Lookup failed.")));
        var page = CreatePageModel(expertiseManager, new Mock<ISectorManager>());

        var result = await page.OnGetCategoriesBySector(1);

        page.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Should().BeOfType<ContentResult>().Which.Content.Should().Contain("Lookup failed.");
    }
}

