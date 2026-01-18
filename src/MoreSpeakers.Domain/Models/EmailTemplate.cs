using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Domain.Models;

public class EmailTemplate
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public DateTime? LastRequested { get; set; }
}