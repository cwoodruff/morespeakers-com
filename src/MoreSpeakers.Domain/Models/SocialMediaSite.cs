using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Domain.Models;

public class SocialMediaSite
{
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Icon {get; set;}
    
    [Required]
    public required string UrlFormat {get; set;}
}