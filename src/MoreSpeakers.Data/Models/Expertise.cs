using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Data.Models;

public class Expertise
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    [MaxLength(500)] public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Foreign keys
    public int ExpertiseCategoryId { get; set; }

    // Navigation properties
    public ExpertiseCategory ExpertiseCategory { get; set; }

    // Navigation properties
    public ICollection<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
}