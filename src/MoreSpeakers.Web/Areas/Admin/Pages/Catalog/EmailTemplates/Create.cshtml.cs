using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

public class CreateModel(IEmailTemplateManager emailTemplateManager, ILogger<CreateModel> logger) : PageModel
{
    private readonly IEmailTemplateManager _emailTemplateManager = emailTemplateManager;
    private readonly ILogger<CreateModel> _logger = logger;

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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check if a template with this location already exists
        EmailTemplate? existing = await _emailTemplateManager.GetByLocationAsync(Input.Location);
        if (existing != null)
        {
            ModelState.AddModelError("Input.Location", "An email template with this location already exists.");
            return Page();
        }

        EmailTemplate template = new EmailTemplate
        {
            Location = Input.Location.Trim(),
            Content = Input.Content,
            IsActive = Input.IsActive,
            CreatedDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _emailTemplateManager.SaveAsync(template);
        _logger.LogInformation("[Admin:EmailTemplates] Created email template {Location}", template.Location);

        return RedirectToPage("Index");
    }
}