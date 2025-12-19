using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models.ViewModels;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Expertises.Expertises;

[Authorize(Roles = "Administrator")]
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
        public int ExpertiseCategoryId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Expertise? Entity { get; private set; }

    [BindProperty(SupportsGet = true)]
    public int SectorId { get; set; }
    
    public List<Sector> Sectors { get; set; } = new();
    public List<ExpertiseCategory> ExpertiseCategories { get; set; } = new();


    public async Task<IActionResult> OnGetAsync()
    {
        var expertise = await _expertiseManager.GetAsync(Id);

        Sectors = await _sectorManager.GetAllSectorsAsync();
        ExpertiseCategories = await _expertiseManager.GetAllCategoriesAsync();
        var expertiseCategory = await _expertiseManager.GetCategoryAsync(expertise.ExpertiseCategoryId);
        if (expertiseCategory is null)
        {
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
        SectorId = expertiseCategory.SectorId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var expertise = await _expertiseManager.GetAsync(Id);

        expertise.Name = Input.Name.Trim();
        expertise.Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim();
        expertise.ExpertiseCategoryId = Input.ExpertiseCategoryId;
        expertise.IsActive = Input.IsActive;

        await _expertiseManager.SaveAsync(expertise);
        _logger.LogInformation("[Admin:Expertises] Updated expertise {Id} {Name}", expertise.Id, expertise.Name);
        return RedirectToPage("../Expertises/Index");
    }

    public async Task<IActionResult> OnGetExpertiseCategoriesAsync()
    {
        var sectorId = SectorId;
        var categories = await _expertiseManager.GetAllActiveCategoriesForSector(sectorId);

        var expertiseCategorySelectItem = new ExpertiseCategoryDropDownViewModel
        {
            ExpertiseCategories = categories, SelectedCategoryId = Input.ExpertiseCategoryId
        };
        return Partial("/Areas/Admin/Pages/Expertises/Expertises/_ExpertiseCategorySelectItem.cshtml", expertiseCategorySelectItem);

    }
}
