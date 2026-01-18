using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.EmailTemplates;

public class CreatePageTests
{
    private readonly Mock<IEmailTemplateManager> _managerMock = new();
    private readonly Mock<ILogger<CreateModel>> _loggerMock = new();

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        // Arrange
        var page = new CreateModel(_managerMock.Object, _loggerMock.Object);
        page.ModelState.AddModelError("Error", "Required");

        // Act
        var result = await page.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _managerMock.Verify(m => m.SaveAsync(It.IsAny<EmailTemplate>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenLocationAlreadyExists()
    {
        // Arrange
        var location = "Templates/Welcome.cshtml";
        _managerMock.Setup(m => m.GetByLocationAsync(location)).ReturnsAsync(new EmailTemplate { Location = location });
        var page = new CreateModel(_managerMock.Object, _loggerMock.Object)
        {
            Input = new CreateModel.InputModel { Location = location, Content = "Some content" }
        };

        // Act
        var result = await page.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        page.ModelState.ContainsKey("Input.Location").Should().BeTrue();
        _managerMock.Verify(m => m.SaveAsync(It.IsAny<EmailTemplate>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSaveAndRedirect_WhenValid()
    {
        // Arrange
        var location = "Templates/New.cshtml";
        _managerMock.Setup(m => m.GetByLocationAsync(location)).ReturnsAsync((EmailTemplate?)null);
        _managerMock.Setup(m => m.SaveAsync(It.IsAny<EmailTemplate>())).ReturnsAsync((EmailTemplate t) => t);
        
        var page = new CreateModel(_managerMock.Object, _loggerMock.Object)
        {
            Input = new CreateModel.InputModel { Location = location, Content = "Hello World", IsActive = true }
        };

        // Act
        var result = await page.OnPostAsync();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
        _managerMock.Verify(m => m.SaveAsync(It.Is<EmailTemplate>(t => t.Location == location && t.Content == "Hello World")), Times.Once);
    }
}