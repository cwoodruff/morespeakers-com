using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

[Authorize(Roles = "Administrator")]
public class IndexModel(IExpertiseManager expertiseManager) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;

    public List<Expertise> Items { get; private set; } = new();

    // Basic search support (query parameter `q`), mirroring Categories pattern
    public string? Q { get; private set; }

    public async Task OnGet(string? q)
    {
        Q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var all = await _expertiseManager.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var query = Q.ToLowerInvariant();
            Items = all
                .Where(e => (!string.IsNullOrEmpty(e.Name) && e.Name.ToLowerInvariant().Contains(query))
                            || (!string.IsNullOrEmpty(e.Description) && e.Description!.ToLowerInvariant().Contains(query)))
                .OrderBy(e => e.Name)
                .ToList();
        }
        else
        {
            Items = all.OrderBy(e => e.Name).ToList();
        }
    }
}
