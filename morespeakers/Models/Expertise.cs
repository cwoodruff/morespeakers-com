using System.ComponentModel.DataAnnotations;

namespace morespeakers.Models;

public class Expertise
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;

    [MaxLength(500)] public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
}