using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Domain.Models;

public class SocialMedia
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    [Required] [MaxLength(50)] public string Platform { get; set; } = string.Empty;

    [Required] [Url] [MaxLength(500)] public string Url { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}