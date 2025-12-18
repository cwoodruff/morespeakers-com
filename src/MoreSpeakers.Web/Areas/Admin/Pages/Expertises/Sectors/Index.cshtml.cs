using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Sectors;

[Authorize(Roles = "Administrator")]
public class IndexModel(ISectorManager manager, ILogger<IndexModel> logger) : PageModel
{
    private readonly ISectorManager _manager = manager;
    private readonly ILogger<IndexModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TriState Status { get; set; } = TriState.Any;

    public List<Sector> Items { get; private set; } = new();

    public async Task OnGet()
    {
        var sectors = await _manager.GetAllSectorsAsync(Status, Q);
        Items = sectors;
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var sector = await _manager.GetAsync(id);
        if (sector is null)
        {
            return RedirectToPage();
        }

        if (!sector.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        sector.IsActive = false;
        await _manager.SaveAsync(sector);
        _logger.LogInformation("[Admin:Sectors] Deactivated sector {Id} {Name}", sector.Id, sector.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var sector = await _manager.GetAsync(id);
        if (sector is null)
        {
            return RedirectToPage();
        }

        if (sector.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        sector.IsActive = true;
        await _manager.SaveAsync(sector);
        _logger.LogInformation("[Admin:Sectors] Activated sector {Id} {Name}", sector.Id, sector.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }
}
