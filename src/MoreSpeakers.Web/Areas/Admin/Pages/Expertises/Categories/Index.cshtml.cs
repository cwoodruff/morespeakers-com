using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Categories;

[Authorize(Roles = "Administrator")]
public class IndexModel(IExpertiseManager expertiseManager, ILogger<IndexModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ILogger<IndexModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TriState Status { get; set; } = TriState.Any;

    public List<ExpertiseCategory> Items { get; private set; } = new();

    public async Task OnGet()
    {
        var all = await _expertiseManager.GetAllCategoriesAsync(onlyActive: false);

        IEnumerable<ExpertiseCategory> result = all;

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var q = Q.Trim();
            result = result.Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        result = Status switch
        {
            TriState.True => result.Where(c => c.IsActive),
            TriState.False => result.Where(c => !c.IsActive),
            _ => result
        };

        Items = result
            .OrderBy(c => c.Name)
            .ToList();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var category = await _expertiseManager.GetCategoryAsync(id);
        if (category is null)
        {
            return RedirectToPage();
        }

        if (!category.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        category.IsActive = false;
        await _expertiseManager.SaveCategoryAsync(category);
        _logger.LogInformation("[Admin:Categories] Deactivated category {Id} {Name}", category.Id, category.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var category = await _expertiseManager.GetCategoryAsync(id);
        if (category is null)
        {
            return RedirectToPage();
        }

        if (category.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        category.IsActive = true;
        await _expertiseManager.SaveCategoryAsync(category);
        _logger.LogInformation("[Admin:Categories] Activated category {Id} {Name}", category.Id, category.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }
}
