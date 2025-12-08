using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

public class IndexModel : PageModel
{
    private readonly ISocialMediaSiteManager _manager;

    public IndexModel(ISocialMediaSiteManager manager)
    {
        _manager = manager;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public List<SocialMediaSite> Items { get; private set; } = new();

    public async Task OnGet()
    {
        var all = await _manager.GetAllAsync();
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
