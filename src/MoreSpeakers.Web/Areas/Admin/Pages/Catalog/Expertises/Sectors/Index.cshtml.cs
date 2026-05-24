using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public partial class IndexModel(ISectorManager manager, ILogger<IndexModel> logger) : PageModel
{
    private readonly ISectorManager _manager = manager;
    private readonly ILogger<IndexModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TriState Status { get; set; } = TriState.Any;

    public List<Sector> Items { get; private set; } = [];

    public async Task OnGet()
    {
        var result = await _manager.GetAllSectorsAsync(Status, Q);
        if (result.IsSuccess)
        {
            Items = result.Value;
        }
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var result = await _manager.GetAsync(id);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToPage();
        }

        var sector = result.Value;
        if (!sector.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        sector.IsActive = false;
        var saveResult = await _manager.SaveAsync(sector);
        if (saveResult.IsFailure)
        {
            TempData["ErrorMessage"] = saveResult.ErrorMessage;
        }
        else
        {
            LogAdminSectorsDeactivated(sector.Id, sector.Name);
        }

        return RedirectToPage(new { q = Q, status = Status });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var result = await _manager.GetAsync(id);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToPage();
        }

        var sector = result.Value;
        if (sector.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        sector.IsActive = true;
        var saveResult = await _manager.SaveAsync(sector);
        if (saveResult.IsFailure)
        {
            TempData["ErrorMessage"] = saveResult.ErrorMessage;
        }
        else
        {
            LogAdminSectorsActivated(sector.Id, sector.Name);
        }

        return RedirectToPage(new { q = Q, status = Status });
    }
}
