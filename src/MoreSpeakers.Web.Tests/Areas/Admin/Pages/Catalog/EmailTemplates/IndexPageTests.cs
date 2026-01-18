using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;
using MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Catalog.EmailTemplates;

public class IndexPageTests
{
    private readonly Mock<IEmailTemplateManager> _managerMock = new();
    private readonly Mock<ILogger<IndexModel>> _loggerMock = new();

    [Fact]
    public async Task OnGetAsync_ShouldLoadAllItems_WhenNoQuery()
    {
        // Arrange
        var items = new List<EmailTemplate>
        {
            new() { Location = "A" },
            new() { Location = "B" },
        };
        _managerMock.Setup(m => m.GetAllTemplatesAsync(TriState.Any, null)).ReturnsAsync(items);
        var page = new IndexModel(_managerMock.Object, _loggerMock.Object);

        // Act
        await page.OnGetAsync();

        // Assert
        page.Items.Should().BeEquivalentTo(items);
        _managerMock.Verify(m => m.GetAllTemplatesAsync(TriState.Any, null), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_ShouldFilterByQueryAndStatus()
    {
        // Arrange
        var items = new List<EmailTemplate> { new() { Location = "Welcome" } };
        _managerMock.Setup(m => m.GetAllTemplatesAsync(TriState.True, "welcome")).ReturnsAsync(items);
        var page = new IndexModel(_managerMock.Object, _loggerMock.Object)
        {
            Q = "welcome",
            Status = TriState.True
        };

        // Act
        await page.OnGetAsync();

        // Assert
        page.Items.Should().BeEquivalentTo(items);
        _managerMock.Verify(m => m.GetAllTemplatesAsync(TriState.True, "welcome"), Times.Once);
    }

    [Fact]
    public async Task OnPostDeactivateAsync_ShouldDeactivateAndRedirect()
    {
        // Arrange
        var id = 1;
        var template = new EmailTemplate { Id = id, Location = "L", IsActive = true };
        _managerMock.Setup(m => m.GetAsync(id)).ReturnsAsync(template);
        _managerMock.Setup(m => m.SaveAsync(It.IsAny<EmailTemplate>())).ReturnsAsync((EmailTemplate t) => t);
        var page = new IndexModel(_managerMock.Object, _loggerMock.Object) { Q = "q", Status = TriState.Any };

        // Act
        var result = await page.OnPostDeactivateAsync(id);

        // Assert
        result.Should().BeOfType<RedirectToPageResult>();
        template.IsActive.Should().BeFalse();
        _managerMock.Verify(m => m.SaveAsync(It.Is<EmailTemplate>(t => t.Id == id && !t.IsActive)), Times.Once);
    }

    [Fact]
    public async Task OnPostActivateAsync_ShouldActivateAndRedirect()
    {
        // Arrange
        var id = 1;
        var template = new EmailTemplate { Id = id, Location = "L", IsActive = false };
        _managerMock.Setup(m => m.GetAsync(id)).ReturnsAsync(template);
        _managerMock.Setup(m => m.SaveAsync(It.IsAny<EmailTemplate>())).ReturnsAsync((EmailTemplate t) => t);
        var page = new IndexModel(_managerMock.Object, _loggerMock.Object) { Q = "q", Status = TriState.Any };

        // Act
        var result = await page.OnPostActivateAsync(id);

        // Assert
        result.Should().BeOfType<RedirectToPageResult>();
        template.IsActive.Should().BeTrue();
        _managerMock.Verify(m => m.SaveAsync(It.Is<EmailTemplate>(t => t.Id == id && t.IsActive)), Times.Once);
    }
}