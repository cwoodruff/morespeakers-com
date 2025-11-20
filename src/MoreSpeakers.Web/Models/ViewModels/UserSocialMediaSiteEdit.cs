using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models.ViewModels;

public class UserSocialMediaSiteEdit
{
    public UserSocialMediaSite? UserSocialMediaSite { get; set; }
    public List<SocialMediaSite> SocialMediaSites { get; set; } = new();
}

/// <summary>
/// Used to render a row in the table of social media sites.
/// </summary>
public class UserSocialMediaSiteRow
{
    public int ItemNumber { get; set; }
    public UserSocialMediaSite? UserSocialMediaSite { get; set; }
    public List<SocialMediaSite> SocialMediaSites { get; set; } = new();
}