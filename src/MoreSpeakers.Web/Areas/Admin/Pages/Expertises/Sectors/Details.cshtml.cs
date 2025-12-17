using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Sectors;

[Authorize(Roles = "Administrator")]
public class DetailsModel(ISectorManager manager) : PageModel
{
    private readonly ISectorManager _manager = manager;

    [FromRoute]
    public int Id { get; set; }

    public Sector? Sector { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Sector = await _manager.GetAsync(Id);
        if (Sector is null)
        {
            return RedirectToPage("Index");
        }

        return Page();
    }
}
