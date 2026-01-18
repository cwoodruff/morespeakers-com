using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

public class DeleteModel(IEmailTemplateManager emailTemplateManager, ILogger<DeleteModel> logger) : PageModel
{
    private readonly IEmailTemplateManager _emailTemplateManager = emailTemplateManager;
    private readonly ILogger<DeleteModel> _logger = logger;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public EmailTemplate EmailTemplate { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        EmailTemplate? template = await _emailTemplateManager.GetAsync(id);
        if (template == null)
        {
            return RedirectToPage("Index");
        }

        Id = id;
        EmailTemplate = template;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        EmailTemplate? template = await _emailTemplateManager.GetAsync(Id);
        string location = template?.Location ?? Id.ToString();

        await _emailTemplateManager.DeleteAsync(Id);
        _logger.LogInformation("[Admin:EmailTemplates] Deleted email template {Id} {Location}", Id, location);

        return RedirectToPage("Index");
    }
}