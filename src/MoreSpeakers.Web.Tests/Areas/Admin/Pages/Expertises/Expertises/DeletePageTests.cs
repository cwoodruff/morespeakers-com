using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Expertises.Expertises;

public class DeletePageTests
{
    [Fact]
    public void PageModel_should_have_authorize_attribute_with_admin_role()
    {
        typeof(DeleteModel).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Any(a => a.Roles?.Contains("Administrator") == true)
            .Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_should_soft_delete_and_redirect()
    {
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.SoftDeleteAsync(7)).ReturnsAsync(true);
        var logger = new Mock<ILogger<DeleteModel>>();
        var page = new DeleteModel(manager.Object, logger.Object)
        {
            Id = 7
        };

        var result = await page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        manager.Verify(m => m.SoftDeleteAsync(7), Times.Once);
    }
}
