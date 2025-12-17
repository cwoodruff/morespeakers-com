using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

[Authorize(Roles = "Administrator")]
public class CreateModel(IExpertiseManager expertiseManager, ILogger<CreateModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ILogger<CreateModel> _logger = logger;

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }
    
    [BindProperty]
    public int ExpertiseCategoryId { get; set; }

    public List<ExpertiseCategory> Categories { get; private set; } = new();

    public async Task OnGet()
    {
        var cats = await _expertiseManager.GetAllCategoriesAsync();
        Categories = cats.OrderBy(c => c.Name).ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ModelState.AddModelError(nameof(Name), "Name is required");
        }
        if (!ModelState.IsValid)
        {
            await OnGet();
            return Page();
        }

        var id = await _expertiseManager.CreateExpertiseAsync(Name.Trim(), Description?.Trim(), ExpertiseCategoryId);
        if (id == 0)
        {
            ModelState.AddModelError(string.Empty, "Failed to create expertise");
            await OnGet();
            return Page();
        }

        _logger.LogInformation("[Admin:Expertises] Created expertise {Id} {Name}", id, Name);
        return RedirectToPage("/Expertises/Expertises/Edit", new { area = "Admin", id });
    }
}
