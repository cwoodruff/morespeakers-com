using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.EmailTemplates;

public class DetailsModel(IEmailTemplateManager emailTemplateManager) : PageModel
{
    private readonly IEmailTemplateManager _emailTemplateManager = emailTemplateManager;

    public EmailTemplate EmailTemplate { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        EmailTemplate? template = await _emailTemplateManager.GetAsync(id);

        if (template == null)
        {
            return NotFound();
        }

        EmailTemplate = template;
        return Page();
    }
}