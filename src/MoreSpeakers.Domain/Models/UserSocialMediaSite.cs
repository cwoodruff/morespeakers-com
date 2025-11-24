using System.ComponentModel.DataAnnotations;

namespace MoreSpeakers.Domain.Models;

public class UserSocialMediaSite
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int SocialMediaSiteId { get; set; }
    [Required] public required string SocialId { get; set; }

    public required User User { get; set; }
    public required SocialMediaSite SocialMediaSite { get; set; }

    /// <summary>
    /// Returns the fully qualified URL for the user's social media site.
    /// </summary>
    public string UserUrl
    {
        get
        {
            return string.Format(SocialMediaSite.UrlFormat, SocialId);
        }
    }
}