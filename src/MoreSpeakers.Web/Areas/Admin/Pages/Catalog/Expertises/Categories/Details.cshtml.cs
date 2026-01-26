using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public class DetailsModel(IExpertiseManager expertiseManager, ISectorManager sectorManager) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly ISectorManager _sectorManager = sectorManager;

    [FromRoute]
    public int Id { get; set; }

    public ExpertiseCategory? Category { get; private set; }
    public Sector? Sector { get; private set; }
    public List<Expertise> Expertises { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Category = await _expertiseManager.GetCategoryAsync(Id);
        if (Category is null)
        {
            return RedirectToPage("Index");
        }

        Sector = await _sectorManager.GetAsync(Category.SectorId);
        Expertises = await _expertiseManager.GetByCategoryIdAsync(Category.Id);
        return Page();
    }
}
