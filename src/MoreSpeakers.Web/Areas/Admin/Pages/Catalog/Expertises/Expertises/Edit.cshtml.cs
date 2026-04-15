using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models.ViewModels;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

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
        public int ExpertiseCategoryId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Expertise? Entity { get; private set; }

    [BindProperty(SupportsGet = true)]
    public int SectorId { get; set; }

    public List<Sector> Sectors { get; set; } = [];
    public List<ExpertiseCategory> ExpertiseCategories { get; set; } = [];


    public async Task<IActionResult> OnGetAsync()
    {
        var expertiseResult = await _expertiseManager.GetAsync(Id);
        if (expertiseResult.IsFailure)
        {
            SetErrorMessage(expertiseResult.Error.Message);
            return RedirectToPage("../Expertises/Index");
        }

        Sectors = await _sectorManager.GetAllSectorsAsync();
        var categoriesResult = await _expertiseManager.GetAllCategoriesAsync();
        if (categoriesResult.IsFailure)
        {
            SetErrorMessage(categoriesResult.Error.Message);
            return RedirectToPage("../Expertises/Index");
        }

        ExpertiseCategories = categoriesResult.Value;
        var expertise = expertiseResult.Value;
        var expertiseCategoryResult = await _expertiseManager.GetCategoryAsync(expertise.ExpertiseCategoryId);
        if (expertiseCategoryResult.IsFailure)
        {
            SetErrorMessage(expertiseCategoryResult.Error.Message);
            return RedirectToPage("../Expertises/Index");
        }

        Entity = expertise;
        Input = new InputModel
        {
            Name = expertise.Name,
            Description = expertise.Description,
            ExpertiseCategoryId = expertise.ExpertiseCategoryId,
            IsActive = expertise.IsActive
        };
        SectorId = expertiseCategoryResult.Value.SectorId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var expertiseResult = await _expertiseManager.GetAsync(Id);
        if (expertiseResult.IsFailure)
        {
            SetErrorMessage(expertiseResult.Error.Message);
            return RedirectToPage("../Expertises/Index");
        }

        var expertise = expertiseResult.Value;

        expertise.Name = Input.Name.Trim();
        expertise.Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim();
        expertise.ExpertiseCategoryId = Input.ExpertiseCategoryId;
        expertise.IsActive = Input.IsActive;

        var saveResult = await _expertiseManager.SaveAsync(expertise);
        if (saveResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, saveResult.Error.Message);
            Sectors = await _sectorManager.GetAllSectorsAsync();
            var categoriesResult = await _expertiseManager.GetAllCategoriesAsync();
            ExpertiseCategories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return Page();
        }

        LogAdminExpertisesUpdated(expertise.Id, expertise.Name);
        return RedirectToPage("../Expertises/Index");
    }

    public async Task<IActionResult> OnGetExpertiseCategoriesAsync()
    {
        var sectorId = SectorId;
        var categoriesResult = await _expertiseManager.GetAllActiveCategoriesForSector(sectorId);
        if (categoriesResult.IsFailure)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return Content($"<div class=\"text-danger small\">{categoriesResult.Error.Message}</div>", "text/html");
        }

        var expertiseCategorySelectItem = new ExpertiseCategoryDropDownViewModel
        {
            ExpertiseCategories = categoriesResult.Value, SelectedCategoryId = Input.ExpertiseCategoryId
        };
        return Partial("_ExpertiseCategorySelectItem", expertiseCategorySelectItem);

    }

    private void SetErrorMessage(string message)
    {
        if (TempData is not null)
        {
            TempData["ErrorMessage"] = message;
        }
    }
}
