using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public class DetailsModel(IExpertiseManager expertiseManager, ISectorManager sectorManager) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ISectorManager _sectorManager = sectorManager;

    [FromRoute]
    public int Id { get; set; }

    public Expertise Expertise { get; private set; } = new();
    public ExpertiseCategory? ExpertiseCategory { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var expertiseResult = await _expertiseManager.GetAsync(Id);
        if (expertiseResult.IsFailure)
        {
            SetErrorMessage(expertiseResult.Error.Message);
            return RedirectToPage("Index");
        }

        Expertise = expertiseResult.Value;
        var expertiseCategoryResult = await _expertiseManager.GetCategoryAsync(Expertise.ExpertiseCategoryId);
        if (expertiseCategoryResult.IsFailure)
        {
            SetErrorMessage(expertiseCategoryResult.Error.Message);
            return RedirectToPage("Index");
        }

        ExpertiseCategory = expertiseCategoryResult.Value;
        return Page();
    }

    private void SetErrorMessage(string message)
    {
        if (TempData is not null)
        {
            TempData["ErrorMessage"] = message;
        }
    }
}
