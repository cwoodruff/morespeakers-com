using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public class EditModel(IExpertiseManager expertiseManager, ISectorManager sectorManager, ILogger<EditModel> logger) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ISectorManager _sectorManager = sectorManager;
    private readonly ILogger<EditModel> _logger = logger;

    [FromRoute]
    public int Id { get; set; }

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

    public List<Sector> Sectors { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var category = await _expertiseManager.GetCategoryAsync(Id);
        if (category is null)
        {
            return RedirectToPage("Index");
        }

        Sectors = await _sectorManager.GetAllSectorsAsync();
        Input = new InputModel
        {
            Name = category.Name,
            Description = category.Description,
            SectorId = category.SectorId,
            IsActive = category.IsActive
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Sectors = await _sectorManager.GetAllSectorsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = await _expertiseManager.GetCategoryAsync(Id);
        if (category is null)
        {
            return RedirectToPage("Index");
        }

        category.Name = Input.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim();
        category.SectorId = Input.SectorId;
        category.IsActive = Input.IsActive;

        await _expertiseManager.SaveCategoryAsync(category);
        _logger.LogInformation("[Admin:Categories] Updated category {Id} {Name}", category.Id, category.Name);
        return RedirectToPage("Index");
    }
}
