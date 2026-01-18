using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

public class EditModel(IEmailTemplateManager emailTemplateManager, ILogger<EditModel> logger) : PageModel
{
    private readonly IEmailTemplateManager _emailTemplateManager = emailTemplateManager;
    private readonly ILogger<EditModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public class InputModel
    {
        [Required]
        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        EmailTemplate? template = await _emailTemplateManager.GetAsync(id);
        if (template == null)
        {
            return RedirectToPage("Index");
        }

        Id = id;
        Input = new InputModel
        {
            Location = template.Location,
            Content = template.Content,
            IsActive = template.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        EmailTemplate? template = await _emailTemplateManager.GetAsync(Id);
        if (template == null)
        {
            return RedirectToPage("Index");
        }

        // Check if a template with this location already exists (if location changed)
        if (template.Location != Input.Location)
        {
            EmailTemplate? existing = await _emailTemplateManager.GetByLocationAsync(Input.Location);
            if (existing != null)
            {
                ModelState.AddModelError("Input.Location", "An email template with this location already exists.");
                return Page();
            }
        }

        template.Location = Input.Location.Trim();
        template.Content = Input.Content;
        template.IsActive = Input.IsActive;

        await _emailTemplateManager.SaveAsync(template);
        _logger.LogInformation("[Admin:EmailTemplates] Updated email template {Id} {Location}", template.Id, template.Location);

        return RedirectToPage("Index");
    }
}