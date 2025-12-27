using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public class CreatePageTests
{
    [Fact]
    public async Task OnPostAsync_should_return_Page_when_model_invalid()
    {
        var manager = new Mock<ISectorManager>();
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(manager.Object, logger.Object);
        page.ModelState.AddModelError("Input.Name", "Required");

        var result = await page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        manager.Verify(m => m.SaveAsync(It.IsAny<Sector>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_should_save_and_redirect_when_valid()
    {
        var manager = new Mock<ISectorManager>();
        manager.Setup(m => m.SaveAsync(It.IsAny<Sector>())).ReturnsAsync((Sector s) => { s.Id = 123; return s; });
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(manager.Object, logger.Object)
        {
            Input = new CreateModel.InputModel
            {
                Name = "  Tech  ",
                Slug = "  tech ",
                Description = "  desc ",
                DisplayOrder = 5,
                IsActive = true
            }
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("Index");
        manager.Verify(m => m.SaveAsync(It.Is<Sector>(s =>
            s.Name == "Tech" && s.Slug == "tech" && s.Description == "desc" && s.DisplayOrder == 5 && s.IsActive
        )), Times.Once);
    }
}
