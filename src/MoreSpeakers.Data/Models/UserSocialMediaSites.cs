using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Data.Models;

public class UserSocialMediaSites
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int SocialMediaSiteId { get; set; }
    [Required]
    public required string SocialId { get; set; }
    
    public required User User { get; set; }
    public required SocialMediaSite SocialMediaSite { get; set; }
}