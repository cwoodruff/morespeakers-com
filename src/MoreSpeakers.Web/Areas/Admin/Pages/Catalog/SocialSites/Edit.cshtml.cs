using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

public class EditModel(ISocialMediaSiteManager manager) : PageModel
{
    [BindProperty]
    public SocialMediaSite Form { get; set; } = new()
    {
        Name = null,
        Icon = null,
        UrlFormat = null
    };

    public string UrlExample => BuildExample(Form.UrlFormat);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var entity = await manager.GetAsync(id);
        if (entity is null)
        {
            return RedirectToPage("Index");
        }

        Form = new SocialMediaSite
        {
            Id = entity.Id,
            Name = entity.Name,
            Icon = entity.Icon,
            UrlFormat = entity.UrlFormat
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ValidateUrlFormat(Form.UrlFormat))
        {
            ModelState.AddModelError(nameof(Form.UrlFormat), "URL format must contain {handle} and produce a valid URL example.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await manager.SaveAsync(Form);
        return RedirectToPage("Index");
    }

    private static bool ValidateUrlFormat(string? fmt)
    {
        if (string.IsNullOrWhiteSpace(fmt)) return false;
        if (!fmt.Contains("{handle}", StringComparison.Ordinal)) return false;
        var example = fmt.Replace("{handle}", "example");
        return Uri.TryCreate(example, UriKind.Absolute, out _);
    }

    private static string BuildExample(string? fmt)
    {
        if (string.IsNullOrWhiteSpace(fmt)) return string.Empty;
        return fmt.Replace("{handle}", "example");
    }
}
