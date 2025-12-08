using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Data;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

public class DeleteModel : PageModel
{
    private readonly ISocialMediaSiteManager _manager;
    private readonly MoreSpeakersDbContext _db;

    public DeleteModel(ISocialMediaSiteManager manager, MoreSpeakersDbContext db)
    {
        _manager = manager;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public SocialMediaSite? Site { get; private set; }
    public int ReferenceCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Id = id;
        Site = await _manager.GetAsync(id);
        if (Site is null)
        {
            return RedirectToPage("Index");
        }

        ReferenceCount = await _db.UserSocialMediaSite.CountAsync(x => x.SocialMediaSiteId == id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Guard: prevent delete when referenced
        var inUse = await _db.UserSocialMediaSite.AnyAsync(x => x.SocialMediaSiteId == Id);
        if (inUse)
        {
            return RedirectToPage("Delete", new { id = Id });
        }

        await _manager.DeleteAsync(Id);
        return RedirectToPage("Index");
    }
}
