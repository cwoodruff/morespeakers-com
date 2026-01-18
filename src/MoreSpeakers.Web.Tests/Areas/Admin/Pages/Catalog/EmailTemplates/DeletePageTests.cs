using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.EmailTemplates;

public class DeletePageTests
{
    private readonly Mock<IEmailTemplateManager> _managerMock = new();
    private readonly Mock<ILogger<DeleteModel>> _loggerMock = new();

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToIndex_WhenTemplateDoesNotExist()
    {
        // Arrange
        _managerMock.Setup(m => m.GetAsync(999)).ReturnsAsync((EmailTemplate?)null);
        var page = new DeleteModel(_managerMock.Object, _loggerMock.Object);

        // Act
        var result = await page.OnGetAsync(999);

        // Assert
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPage_WhenTemplateExists()
    {
        // Arrange
        var template = new EmailTemplate { Id = 1, Location = "exists" };
        _managerMock.Setup(m => m.GetAsync(1)).ReturnsAsync(template);
        var page = new DeleteModel(_managerMock.Object, _loggerMock.Object);

        // Act
        var result = await page.OnGetAsync(1);

        // Assert
        result.Should().BeOfType<PageResult>();
        page.EmailTemplate.Should().BeEquivalentTo(template);
    }

    [Fact]
    public async Task OnPostAsync_ShouldDeleteAndRedirectToIndex_WhenValid()
    {
        // Arrange
        var id = 1;
        _managerMock.Setup(m => m.GetAsync(id)).ReturnsAsync(new EmailTemplate { Id = id, Location = "L" });
        _managerMock.Setup(m => m.DeleteAsync(id)).ReturnsAsync(true);
        var page = new DeleteModel(_managerMock.Object, _loggerMock.Object) { Id = id };

        // Act
        var result = await page.OnPostAsync();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
        _managerMock.Verify(m => m.DeleteAsync(id), Times.Once);
    }
}