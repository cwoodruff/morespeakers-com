// File: MoreSpeakers.Data/Models/Sector.cs
using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Data.Models;

public class Sector
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Slug { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ExpertiseCategory> ExpertiseCategories { get; set; } = [];
}