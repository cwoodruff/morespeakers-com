using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public class CreateModel(ISectorManager manager, ILogger<CreateModel> logger) : PageModel
{
    private readonly ISectorManager _manager = manager;
    private readonly ILogger<CreateModel> _logger = logger;

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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var sector = new Sector
        {
            Name = Input.Name.Trim(),
            Slug = string.IsNullOrWhiteSpace(Input.Slug) ? null : Input.Slug!.Trim(),
            Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description!.Trim(),
            DisplayOrder = Input.DisplayOrder,
            IsActive = Input.IsActive
        };

        var saved = await _manager.SaveAsync(sector);
        _logger.LogInformation("[Admin:Sectors] Created sector {Id} {Name}", saved.Id, saved.Name);
        return RedirectToPage("Index");
    }
}
