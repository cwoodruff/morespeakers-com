using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Expertises.Expertises;

public class CreatePageTests
{
    [Fact]
    public void PageModel_should_have_authorize_attribute_with_admin_role()
    {
        typeof(CreateModel).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Any(a => a.Roles?.Contains("Administrator") == true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task OnGet_should_load_and_order_categories()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllCategoriesAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<ExpertiseCategory>
            {
                new() { Id = 2, Name = "Beta" },
                new() { Id = 1, Name = "Alpha" }
            });
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(manager.Object, logger.Object);

        await page.OnGet();

        page.Categories.Should().HaveCount(2);
        page.Categories.Select(c => c.Name).Should().ContainInOrder("Alpha", "Beta");
    }

    [Fact]
    public async Task OnPost_should_return_page_when_invalid_and_reload_categories()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllCategoriesAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<ExpertiseCategory> { new() { Id = 1, Name = "Alpha" } });
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(manager.Object, logger.Object)
        {
            Name = " ",
            ExpertiseCategoryId = 1
        };

        var result = await page.OnPost();

        result.Should().BeOfType<PageResult>();
        page.Categories.Should().NotBeEmpty();
        manager.Verify(m => m.GetAllCategoriesAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_should_return_page_when_create_fails()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllCategoriesAsync(It.IsAny<MoreSpeakers.Domain.Models.AdminUsers.TriState>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<ExpertiseCategory> { new() { Id = 1, Name = "Alpha" } });
        manager.Setup(m => m.CreateExpertiseAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>()))
            .ReturnsAsync(0);
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(manager.Object, logger.Object)
        {
            Name = "NewExp",
            Description = "Desc",
            ExpertiseCategoryId = 1
        };

        var result = await page.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_should_redirect_to_edit_when_success()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.CreateExpertiseAsync("NewExp", "Desc", 1)).ReturnsAsync(42);
        var logger = new Mock<ILogger<CreateModel>>();
        var page = new CreateModel(manager.Object, logger.Object)
        {
            Name = "NewExp",
            Description = "Desc",
            ExpertiseCategoryId = 1
        };

        var result = await page.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = (RedirectToPageResult)result;
        redirect.PageName.Should().Be("/Expertises/Expertises/Edit");
        redirect.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(42);
        redirect.RouteValues.Should().Contain(new KeyValuePair<string, object?>("area", "Admin"));
    }
}
