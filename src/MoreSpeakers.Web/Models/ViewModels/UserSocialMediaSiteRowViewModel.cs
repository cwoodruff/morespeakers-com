using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models.ViewModels;

/// <summary>
/// Used to render a row in the table of social media sites.
/// </summary>
public class UserSocialMediaSiteRowViewModel
{
    public int ItemNumber { get; set; }
    public UserSocialMediaSite? UserSocialMediaSite { get; set; }
    public List<SocialMediaSite> SocialMediaSites { get; set; } = [];
}