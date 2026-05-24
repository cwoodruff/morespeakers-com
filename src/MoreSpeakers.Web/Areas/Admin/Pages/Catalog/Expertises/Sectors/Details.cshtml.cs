using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public class DetailsModel(ISectorManager sectorManager) : PageModel
{
    private readonly ISectorManager _sectorManager = sectorManager;

    [FromRoute]
    public int Id { get; set; }

    public Sector? Sector { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _sectorManager.GetSectorWithRelationshipsAsync(Id);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToPage("Index");
        }

        Sector = result.Value;
        return Page();
    }
}
