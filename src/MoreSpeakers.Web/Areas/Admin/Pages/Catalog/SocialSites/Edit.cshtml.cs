using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.SocialSites;

public class EditModel : PageModel
{
    private readonly ISocialMediaSiteManager _manager;

    public EditModel(ISocialMediaSiteManager manager)
    {
        _manager = manager;
    }

    [BindProperty]
    public FormModel Form { get; set; } = new();

    public string UrlExample => BuildExample(Form.UrlFormat);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var entity = await _manager.GetAsync(id);
        if (entity is null)
        {
            return RedirectToPage("Index");
        }

        Form = new FormModel
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

        var entity = new SocialMediaSite
        {
            Id = Form.Id,
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
        if (string.IsNullOrWhiteSpace(fmt)) return string.Empty;
        return fmt.Replace("{handle}", "example");
    }

    public class FormModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Icon { get; set; }
        [Required]
        public string? UrlFormat { get; set; }
    }
}
