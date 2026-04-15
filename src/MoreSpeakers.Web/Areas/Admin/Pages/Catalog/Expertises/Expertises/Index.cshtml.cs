using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public partial class IndexModel(IExpertiseManager expertiseManager, ILogger<IndexModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ILogger<IndexModel> _logger = logger;

    public List<Expertise> Items { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TriState Status { get; set; } = TriState.Any;

    public async Task OnGet(string? q)
    {
        Q = q;
        var expertisesResult = await _expertiseManager.GetAllExpertisesAsync(Status, Q);
        if (expertisesResult.IsFailure)
        {
            SetErrorMessage(expertisesResult.Error.Message);
            Items = [];
            return;
        }

        Items = expertisesResult.Value;
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var expertiseResult = await _expertiseManager.GetAsync(id);
        if (expertiseResult.IsFailure)
        {
            SetErrorMessage(expertiseResult.Error.Message);
            return RedirectToPage(new { q = Q, status = Status });
        }

        var expertise = expertiseResult.Value;

        if (!expertise.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        expertise.IsActive = false;
        var saveResult = await _expertiseManager.SaveAsync(expertise);
        if (saveResult.IsFailure)
        {
            SetErrorMessage(saveResult.Error.Message);
            return RedirectToPage(new { q = Q, status = Status });
        }

        LogAdminExpertisesDeactivatedExpertiseIdName(expertise.Id, expertise.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var expertiseResult = await _expertiseManager.GetAsync(id);
        if (expertiseResult.IsFailure)
        {
            SetErrorMessage(expertiseResult.Error.Message);
            return RedirectToPage(new { q = Q, status = Status });
        }

        var expertise = expertiseResult.Value;

        if (expertise.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        expertise.IsActive = true;
        var saveResult = await _expertiseManager.SaveAsync(expertise);
        if (saveResult.IsFailure)
        {
            SetErrorMessage(saveResult.Error.Message);
            return RedirectToPage(new { q = Q, status = Status });
        }

        LogAdminExpertisesActivatedCategoryIdName(expertise.Id, expertise.Name);
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
