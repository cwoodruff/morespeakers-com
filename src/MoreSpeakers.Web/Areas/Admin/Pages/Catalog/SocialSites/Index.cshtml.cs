using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

public class IndexModel(ISocialMediaSiteManager manager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public List<SocialMediaSite> Items { get; private set; } = new();

    public async Task OnGet()
    {
        var all = await manager.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var q = Q.Trim();
            Items = all
                .Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Name)
                .ToList();
        }
        else
        {
            Items = all.OrderBy(s => s.Name).ToList();
        }
    }
}
