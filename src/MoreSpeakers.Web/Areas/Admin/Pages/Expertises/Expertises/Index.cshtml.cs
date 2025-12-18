using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

[Authorize(Roles = "Administrator")]
public class IndexModel(IExpertiseManager expertiseManager, ILogger<IndexModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ILogger<IndexModel> _logger = logger;

    public List<Expertise> Items { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TriState Status { get; set; } = TriState.Any;

    public async Task OnGet(string? q)
    {
        var expertises = await _expertiseManager.GetAllExpertisesAsync(Status, Q);
        Items = expertises;
    }
    
    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var expertise = await _expertiseManager.GetAsync(id);

        if (!expertise.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        expertise.IsActive = false;
        await _expertiseManager.SaveAsync(expertise);
        _logger.LogInformation("[Admin:Expertises] Deactivated expertise {Id} {Name}", expertise.Id, expertise.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var expertise = await _expertiseManager.GetAsync(id);
        
        if (expertise.IsActive)
        {
            return RedirectToPage(new { q = Q, status = Status });
        }

        expertise.IsActive = true;
        await _expertiseManager.SaveAsync(expertise);
        _logger.LogInformation("[Admin:Expertises] Activated category {Id} {Name}", expertise.Id, expertise.Name);
        return RedirectToPage(new { q = Q, status = Status });
    }
}
