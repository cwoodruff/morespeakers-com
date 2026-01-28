using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public partial class IndexModel(IExpertiseManager expertiseManager, ILogger<IndexModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ILogger<IndexModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TriState Status { get; set; } = TriState.Any;

    public List<ExpertiseCategory> Items { get; private set; } = [];

    public async Task OnGet()
    {
        var expertiseCategories = await _expertiseManager.GetAllCategoriesAsync(Status, Q);
        Items = [.. expertiseCategories];
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
        LogAdminCategoriesDeactivatedCategoryIdName(category.Id, category.Name);
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
        LogAdminCategoriesActivatedCategoryIdName(category.Id, category.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }
}
