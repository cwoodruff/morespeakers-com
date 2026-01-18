using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.EmailTemplates;

public class DetailsPageTests
{
    private readonly Mock<IEmailTemplateManager> _managerMock = new();

    [Fact]
    public async Task OnGetAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        // Arrange
        _managerMock.Setup(m => m.GetAsync(999)).ReturnsAsync((EmailTemplate?)null);
        var page = new DetailsModel(_managerMock.Object);

        // Act
        var result = await page.OnGetAsync(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPage_WhenTemplateExists()
    {
        // Arrange
        var template = new EmailTemplate { Id = 1, Location = "exists", Content = "content" };
        _managerMock.Setup(m => m.GetAsync(1)).ReturnsAsync(template);
        var page = new DetailsModel(_managerMock.Object);

        // Act
        var result = await page.OnGetAsync(1);

        // Assert
        result.Should().BeOfType<PageResult>();
        page.EmailTemplate.Should().BeEquivalentTo(template);
    }
}