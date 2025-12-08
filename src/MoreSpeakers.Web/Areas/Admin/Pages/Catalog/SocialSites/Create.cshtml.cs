using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

public class CreateModel : PageModel
{
    private readonly ISocialMediaSiteManager _manager;

    public CreateModel(ISocialMediaSiteManager manager)
    {
        _manager = manager;
    }

    [BindProperty]
    public FormModel Form { get; set; } = new();

    public string UrlExample => BuildExample(Form.UrlFormat);

    public void OnGet()
    {
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

        var entity = new SocialMediaSite
        {
            Name = Form.Name!.Trim(),
            Icon = Form.Icon!.Trim(),
            UrlFormat = Form.UrlFormat!.Trim()
        };

        await _manager.SaveAsync(entity);
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
        return string.IsNullOrWhiteSpace(fmt) ? string.Empty : fmt.Replace("{handle}", "example");
    }

    public class FormModel
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Icon { get; set; }
        [Required]
        public string? UrlFormat { get; set; }
    }
}
