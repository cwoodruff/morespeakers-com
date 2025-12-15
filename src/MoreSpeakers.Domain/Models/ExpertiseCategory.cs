using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Domain.Models;

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
    
    // NEW: required parent Sector (NOT NULL in DB)
    public int SectorId { get; set; }
    public Sector? Sector { get; set; }

    // Navigation properties
    public ICollection<Expertise> Expertises { get; set; } = new List<Expertise>();
}
