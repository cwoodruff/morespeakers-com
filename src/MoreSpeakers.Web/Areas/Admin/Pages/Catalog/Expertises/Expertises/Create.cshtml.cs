using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using System.ComponentModel.DataAnnotations;
using MoreSpeakers.Web.Models.ViewModels;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

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

        [Range(1, int.MaxValue, ErrorMessage = "Please select a sector")]
        public int SectorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an expertise category")]
        public int ExpertiseCategoryId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Sector> Sectors { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Sectors = await _sectorManager.GetAllSectorsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Sectors = await _sectorManager.GetAllSectorsAsync();
            return Page();
        }

        var expertise = new Expertise
        {
            Name = Input.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim(),
            ExpertiseCategoryId = Input.ExpertiseCategoryId,
            IsActive = Input.IsActive
        };

        var savedResult = await _expertiseManager.SaveAsync(expertise);
        if (savedResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, savedResult.Error.Message);
            Sectors = await _sectorManager.GetAllSectorsAsync();
            return Page();
        }

        LogAdminExpertisesCreatedExpertiseIdName(savedResult.Value.Id, savedResult.Value.Name);
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnGetCategoriesBySector(int sectorId)
    {
        var categoriesResult = await _expertiseManager.GetAllActiveCategoriesForSector(sectorId);
        if (categoriesResult.IsFailure)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return Content($"<div class=\"text-danger small\">{categoriesResult.Error.Message}</div>", "text/html");
        }

        var viewModel = new ExpertiseCategoryDropDownViewModel
        {
            ExpertiseCategories = [.. categoriesResult.Value.OrderBy(c => c.Name)]
        };

        return Partial("_ExpertiseCategorySelectItem", viewModel);
    }
}
