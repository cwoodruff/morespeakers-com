using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Data.Models;

public class UserSocialMediaSites
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int SocialMediaSiteId { get; set; }
    [Required]
    public required string SocialId { get; set; }

    public User User { get; set; } = null!;
    public SocialMediaSite SocialMediaSite { get; set; } = null!;
}