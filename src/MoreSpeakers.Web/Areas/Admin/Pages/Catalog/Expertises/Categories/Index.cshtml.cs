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
        var expertiseCategoriesResult = await _expertiseManager.GetAllCategoriesAsync(Status, Q);
        if (expertiseCategoriesResult.IsFailure)
        {
            SetErrorMessage(expertiseCategoriesResult.Error.Message);
            Items = [];
            return;
        }

        Items = [.. expertiseCategoriesResult.Value];
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var categoryResult = await _expertiseManager.GetCategoryAsync(id);
        if (categoryResult.IsFailure)
        {
            SetErrorMessage(categoryResult.Error.Message);
            return RedirectToPage();
        }

        var category = categoryResult.Value;
        if (!category.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        category.IsActive = false;
        var saveResult = await _expertiseManager.SaveCategoryAsync(category);
        if (saveResult.IsFailure)
        {
            SetErrorMessage(saveResult.Error.Message);
            return RedirectToPage(new { q = Q, status = Status });
        }

        LogAdminCategoriesDeactivatedCategoryIdName(category.Id, category.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var categoryResult = await _expertiseManager.GetCategoryAsync(id);
        if (categoryResult.IsFailure)
        {
            SetErrorMessage(categoryResult.Error.Message);
            return RedirectToPage();
        }

        var category = categoryResult.Value;
        if (category.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        category.IsActive = true;
        var saveResult = await _expertiseManager.SaveCategoryAsync(category);
        if (saveResult.IsFailure)
        {
            SetErrorMessage(saveResult.Error.Message);
            return RedirectToPage(new { q = Q, status = Status });
        }

        LogAdminCategoriesActivatedCategoryIdName(category.Id, category.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }

    private void SetErrorMessage(string message)
    {
        if (TempData is not null)
        {
            TempData["ErrorMessage"] = message;
        }
    }
}
