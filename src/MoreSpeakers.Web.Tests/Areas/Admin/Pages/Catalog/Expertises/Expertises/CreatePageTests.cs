using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public class CreatePageTests
{
    private CreateModel CreatePageModel(Mock<IExpertiseManager> expertiseManager, Mock<ISectorManager> sectorManager)
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        var pageContext = new PageContext(actionContext)
        {
            ViewData = viewData
        };

        return new CreateModel(expertiseManager.Object, sectorManager.Object, new Mock<ILogger<CreateModel>>().Object)
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
            .ReturnsAsync(new List<Sector>
            {
                new() { Id = 1, Name = "Sector 1" }
            });
        
        var page = CreatePageModel(expertiseManager, sectorManager);

        await page.OnGetAsync();

        page.Sectors.Should().HaveCount(1);
        page.Sectors[0].Name.Should().Be("Sector 1");
    }

    [Fact]
    public async Task OnPostAsync_should_return_page_when_invalid_and_reload_sectors()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        sectorManager.Setup(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false))
            .ReturnsAsync(new List<Sector> { new() { Id = 1, Name = "Sector 1" } });
        
        var page = CreatePageModel(expertiseManager, sectorManager);
        page.ModelState.AddModelError("Input.Name", "Required");

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        page.Sectors.Should().NotBeEmpty();
        sectorManager.Verify(m => m.GetAllSectorsAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>(), false), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_should_redirect_to_index_when_success()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        expertiseManager.Setup(m => m.SaveAsync(It.IsAny<Expertise>())).ReturnsAsync(new Expertise { Id = 42, Name = "NewExp" });
        
        var page = CreatePageModel(expertiseManager, sectorManager);
        page.Input = new CreateModel.InputModel
        {
            Name = "NewExp",
            Description = "Desc",
            SectorId = 1,
            ExpertiseCategoryId = 1
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = (RedirectToPageResult)result;
        redirect.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetCategoriesBySector_should_return_partial_with_categories()
    {
        var expertiseManager = new Mock<IExpertiseManager>();
        var sectorManager = new Mock<ISectorManager>();
        expertiseManager.Setup(m => m.GetAllActiveCategoriesForSector(1))
            .ReturnsAsync(new List<ExpertiseCategory>
            {
                new() { Id = 1, Name = "Category 1" }
            });
        
        var page = CreatePageModel(expertiseManager, sectorManager);

        var result = await page.OnGetCategoriesBySector(1);

        result.Should().BeOfType<PartialViewResult>();
        var partial = (PartialViewResult)result;
        partial.ViewName.Should().Be("_ExpertiseCategorySelectItem");
        partial.Model.Should().BeOfType<ExpertiseCategoryDropDownViewModel>();
        var model = (ExpertiseCategoryDropDownViewModel)partial.Model!;
        model.ExpertiseCategories.Should().HaveCount(1);
        model.ExpertiseCategories[0].Name.Should().Be("Category 1");
    }
}
