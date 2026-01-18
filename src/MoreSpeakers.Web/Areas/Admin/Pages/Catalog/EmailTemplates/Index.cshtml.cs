using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

public class IndexModel(IEmailTemplateManager emailTemplateManager, ILogger<IndexModel> logger) : PageModel
{
    private readonly IEmailTemplateManager _emailTemplateManager = emailTemplateManager;
    private readonly ILogger<IndexModel> _logger = logger;

    public List<EmailTemplate> Items { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TriState Status { get; set; } = TriState.Any;

    public async Task OnGetAsync()
    {
        Items = await _emailTemplateManager.GetAllTemplatesAsync(Status, Q);
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        EmailTemplate? template = await _emailTemplateManager.GetAsync(id);
        if (template is null)
        {
            return RedirectToPage();
        }

        if (!template.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        template.IsActive = false;
        await _emailTemplateManager.SaveAsync(template);
        _logger.LogInformation("[Admin:EmailTemplates] Deactivated email template {Id} {Location}", template.Id, template.Location);
        return RedirectToPage(new { q = Q, status = Status });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        EmailTemplate? template = await _emailTemplateManager.GetAsync(id);
        if (template is null)
        {
            return RedirectToPage();
        }

        if (template.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        template.IsActive = true;
        await _emailTemplateManager.SaveAsync(template);
        _logger.LogInformation("[Admin:EmailTemplates] Activated email template {Id} {Location}", template.Id, template.Location);
        return RedirectToPage(new { q = Q, status = Status });
    }
}