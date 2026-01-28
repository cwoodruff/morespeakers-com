using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public partial class CreateModel(IExpertiseManager expertiseManager, ISectorManager sectorManager, ILogger<CreateModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ISectorManager _sectorManager = sectorManager;
    private readonly ILogger<CreateModel> _logger = logger;

    public sealed class InputModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int SectorId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Sector> Sectors { get; private set; } = [];

    public async Task OnGet()
    {
        Sectors = await _sectorManager.GetAllSectorsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Sectors = await _sectorManager.GetAllSectorsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = new ExpertiseCategory
        {
            Name = Input.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim(),
            SectorId = Input.SectorId,
            IsActive = Input.IsActive
        };

        var saved = await _expertiseManager.SaveCategoryAsync(category);
        LogAdminCategoriesCreated(saved.Id, saved.Name);
        return RedirectToPage("Index");
    }
}
