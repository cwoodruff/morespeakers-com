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
        var siteResult = await manager.GetAsync(id);
        if (siteResult.IsFailure)
        {
            TempData["ErrorMessage"] = siteResult.ErrorMessage;
            return RedirectToPage("Index");
        }

        Site = siteResult.Value;

        var refCountResult = await manager.RefCountAsync(Id);
        ReferenceCount = refCountResult.IsSuccess ? refCountResult.Value : 0;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Guard: prevent delete when referenced
        var inUseResult = await manager.InUseAsync(Id);
        if (inUseResult.IsSuccess && inUseResult.Value)
        {
            return RedirectToPage("Delete", new { id = Id });
        }

        var deleteResult = await manager.DeleteAsync(Id);
        if (deleteResult.IsFailure)
        {
            TempData["ErrorMessage"] = deleteResult.ErrorMessage;
        }

        return RedirectToPage("Index");
    }
}
