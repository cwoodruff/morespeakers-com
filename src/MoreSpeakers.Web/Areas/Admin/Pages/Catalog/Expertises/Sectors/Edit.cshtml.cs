using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public class EditModel(ISectorManager manager, ILogger<EditModel> logger) : PageModel
{
    private readonly ISectorManager _manager = manager;
    private readonly ILogger<EditModel> _logger = logger;

    [FromRoute]
    public int Id { get; set; }

    public sealed class InputModel
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Slug { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, 10_000)]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var sector = await _manager.GetAsync(Id);
        if (sector is null)
            return RedirectToPage("Index");

        Input = new InputModel
        {
            Name = sector.Name,
            Slug = sector.Slug,
            Description = sector.Description,
            DisplayOrder = sector.DisplayOrder,
            IsActive = sector.IsActive
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var sector = await _manager.GetAsync(Id);
        if (sector is null)
        {
            return RedirectToPage("Index");
        }

        sector.Name = Input.Name.Trim();
        sector.Slug = string.IsNullOrWhiteSpace(Input.Slug) ? null : Input.Slug!.Trim();
        sector.Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim();
        sector.DisplayOrder = Input.DisplayOrder;
        sector.IsActive = Input.IsActive;

        await _manager.SaveAsync(sector);
        _logger.LogInformation("[Admin:Sectors] Updated sector {Id} {Name}", sector.Id, sector.Name);
        return RedirectToPage("Index");
    }
}
