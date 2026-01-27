using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Data.Models;

public class Expertise
{
    public int Id { get; init; }

    [Required] [MaxLength(100)] public string Name { get; init; } = string.Empty;

    [MaxLength(500)] public string? Description { get; init; }

    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Foreign keys
    public int ExpertiseCategoryId { get; init; }

    // Navigation properties
    public ExpertiseCategory? ExpertiseCategory { get; set; }

    // Navigation properties
    public ICollection<UserExpertise> UserExpertise { get; init; } = [];
}