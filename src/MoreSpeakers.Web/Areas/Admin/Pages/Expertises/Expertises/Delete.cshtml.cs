using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

[Authorize(Roles = "Administrator")]
public class DeleteModel(IExpertiseManager expertiseManager, ILogger<DeleteModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ILogger<DeleteModel> _logger = logger;

    [FromRoute]
    public int Id { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _expertiseManager.SoftDeleteAsync(Id);
        if (result)
        {
            _logger.LogInformation("[Admin:Expertises] Soft-deleted expertise {Id}", Id);
        }
        else
        {
            _logger.LogWarning("[Admin:Expertises] Soft delete returned false for expertise {Id}", Id);
        }
        return RedirectToPage("../Categories/Index");
    }
}
