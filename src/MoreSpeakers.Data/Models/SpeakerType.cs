using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Data.Models;

public class SpeakerType
{
    public int Id { get; set; }

    [Required] [MaxLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [MaxLength(200)] public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<User> Users { get; set; } = [];
}