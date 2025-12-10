using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Data;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

public class DeleteModel(ISocialMediaSiteManager manager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public SocialMediaSite? Site { get; private set; }
    public int ReferenceCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Id = id;
        Site = await manager.GetAsync(id);
        if (Site is null)
        {
            return RedirectToPage("Index");
        }

        ReferenceCount = await manager.RefCountAsync(Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Guard: prevent delete when referenced
        var inUse = await manager.InUseAsync(Id);
        if (inUse)
        {
            return RedirectToPage("Delete", new { id = Id });
        }

        await manager.DeleteAsync(Id);
        return RedirectToPage("Index");
    }
}
