using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.EmailTemplates;

public class EditPageTests
{
    private readonly Mock<IEmailTemplateManager> _managerMock = new();
    private readonly Mock<ILogger<EditModel>> _loggerMock = new();

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToIndex_WhenTemplateDoesNotExist()
    {
        // Arrange
        _managerMock.Setup(m => m.GetAsync(999)).ReturnsAsync((EmailTemplate?)null);
        var page = new EditModel(_managerMock.Object, _loggerMock.Object);

        // Act
        var result = await page.OnGetAsync(999);

        // Assert
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPage_WhenTemplateExists()
    {
        // Arrange
        var template = new EmailTemplate { Id = 1, Location = "exists", Content = "content", IsActive = true };
        _managerMock.Setup(m => m.GetAsync(1)).ReturnsAsync(template);
        var page = new EditModel(_managerMock.Object, _loggerMock.Object);

        // Act
        var result = await page.OnGetAsync(1);

        // Assert
        result.Should().BeOfType<PageResult>();
        page.Input.Location.Should().Be("exists");
        page.Input.Content.Should().Be("content");
        page.Input.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        // Arrange
        var page = new EditModel(_managerMock.Object, _loggerMock.Object);
        page.ModelState.AddModelError("Error", "Required");

        // Act
        var result = await page.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _managerMock.Verify(m => m.SaveAsync(It.IsAny<EmailTemplate>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenLocationAlreadyExistsForAnotherTemplate()
    {
        // Arrange
        var id = 1;
        var existingLocation = "Templates/Existing.cshtml";
        var template = new EmailTemplate { Id = id, Location = "Templates/Old.cshtml" };
        
        _managerMock.Setup(m => m.GetAsync(id)).ReturnsAsync(template);
        _managerMock.Setup(m => m.GetByLocationAsync(existingLocation)).ReturnsAsync(new EmailTemplate { Id = 2, Location = existingLocation });
        
        var page = new EditModel(_managerMock.Object, _loggerMock.Object)
        {
            Id = id,
            Input = new EditModel.InputModel { Location = existingLocation, Content = "New Content" }
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
        var id = 1;
        var location = "Templates/Updated.cshtml";
        var template = new EmailTemplate { Id = id, Location = "Templates/Old.cshtml", Content = "Old Content" };
        
        _managerMock.Setup(m => m.GetAsync(id)).ReturnsAsync(template);
        _managerMock.Setup(m => m.GetByLocationAsync(location)).ReturnsAsync((EmailTemplate?)null);
        _managerMock.Setup(m => m.SaveAsync(It.IsAny<EmailTemplate>())).ReturnsAsync((EmailTemplate t) => t);
        
        var page = new EditModel(_managerMock.Object, _loggerMock.Object)
        {
            Id = id,
            Input = new EditModel.InputModel { Location = location, Content = "Updated Content", IsActive = false }
        };

        // Act
        var result = await page.OnPostAsync();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
        _managerMock.Verify(m => m.SaveAsync(It.Is<EmailTemplate>(t => t.Id == id && t.Location == location && t.Content == "Updated Content" && !t.IsActive)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSaveAndRedirect_WhenLocationUnchanged()
    {
        // Arrange
        var id = 1;
        var location = "Templates/Same.cshtml";
        var template = new EmailTemplate { Id = id, Location = location, Content = "Old Content" };

        _managerMock.Setup(m => m.GetAsync(id)).ReturnsAsync(template);
        _managerMock.Setup(m => m.SaveAsync(It.IsAny<EmailTemplate>())).ReturnsAsync((EmailTemplate t) => t);

        var page = new EditModel(_managerMock.Object, _loggerMock.Object)
        {
            Id = id,
            Input = new EditModel.InputModel { Location = location, Content = "Updated Content", IsActive = true }
        };

        // Act
        var result = await page.OnPostAsync();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("Index");
        _managerMock.Verify(m => m.GetByLocationAsync(It.IsAny<string>()), Times.Never);
        _managerMock.Verify(m => m.SaveAsync(It.Is<EmailTemplate>(t => t.Id == id && t.Location == location && t.Content == "Updated Content")), Times.Once);
    }
}