using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Data.Models;

public class ExpertiseCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
    
    public int SectorId { get; set; }
    public Sector Sector { get; set; } = null!;

    // Navigation properties
    public ICollection<Expertise> Expertises { get; set; } = new List<Expertise>();
}
