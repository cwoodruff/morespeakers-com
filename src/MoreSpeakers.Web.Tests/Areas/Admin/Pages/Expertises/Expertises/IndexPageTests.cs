using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Moq;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

namespace MoreSpeakers.Web.Tests.Areas.Admin.Pages.Expertises.Expertises;

public class IndexPageTests
{
    [Fact]
    public async Task OnGet_should_list_all_sorted_by_name_when_no_query()
    {
        // Arrange
        var items = new List<Expertise>
        {
            new() { Id = 2, Name = "Beta" },
            new() { Id = 1, Name = "Alpha" },
        };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllAsync()).ReturnsAsync(items);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object);

        // Act
        await page.OnGet(q: null);

        // Assert
        page.Should().BeAssignableTo<PageModel>();
        page.Items.Should().HaveCount(2);
        page.Items.Select(e => e.Name).Should().ContainInOrder("Alpha", "Beta");
        manager.Verify(m => m.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task OnGet_should_filter_by_query_name_or_description()
    {
        // Arrange
        var items = new List<Expertise>
        {
            new() { Id = 1, Name = "FinTech", Description = "Finance technology" },
            new() { Id = 2, Name = "Healthcare", Description = "Med Tech" },
            new() { Id = 3, Name = "Retail", Description = "Commerce" },
        };
        var manager = new Mock<IExpertiseManager>();
        manager.Setup(m => m.GetAllAsync()).ReturnsAsync(items);
        var logger = new Mock<ILogger<IndexModel>>();
        var page = new IndexModel(manager.Object, logger.Object);

        // Act
        await page.OnGet(q: "tech");

        // Assert
        page.Items.Should().HaveCount(2);
        page.Items.Select(e => e.Id).Should().BeEquivalentTo(new[] { 1, 2 });
    }
}
