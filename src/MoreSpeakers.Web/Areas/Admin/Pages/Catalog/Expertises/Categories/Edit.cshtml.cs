using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public partial class EditModel(IExpertiseManager expertiseManager, ISectorManager sectorManager, ILogger<EditModel> logger) : PageModel
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

    public List<Sector> Sectors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var categoryResult = await _expertiseManager.GetCategoryAsync(Id);
        if (categoryResult.IsFailure)
        {
            SetErrorMessage(categoryResult.Error.Message);
            return RedirectToPage("Index");
        }

        Sectors = await _sectorManager.GetAllSectorsAsync();
        Input = new InputModel
        {
            Name = categoryResult.Value.Name,
            Description = categoryResult.Value.Description,
            SectorId = categoryResult.Value.SectorId,
            IsActive = categoryResult.Value.IsActive
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

        var categoryResult = await _expertiseManager.GetCategoryAsync(Id);
        if (categoryResult.IsFailure)
        {
            SetErrorMessage(categoryResult.Error.Message);
            return RedirectToPage("Index");
        }

        var category = categoryResult.Value;
        category.Name = Input.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim();
        category.SectorId = Input.SectorId;
        category.IsActive = Input.IsActive;

        var savedResult = await _expertiseManager.SaveCategoryAsync(category);
        if (savedResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, savedResult.Error.Message);
            return Page();
        }

        LogAdminCategoriesUpdated(category.Id, category.Name);
        return RedirectToPage("Index");
    }

    private void SetErrorMessage(string message)
    {
        if (TempData is not null)
        {
            TempData["ErrorMessage"] = message;
        }
    }
}
