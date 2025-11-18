using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models.ViewModels;

public class UserSocialMediaSiteEdit
{
    public UserSocialMediaSite? UserSocialMediaSite { get; set; }
    public List<SocialMediaSite> SocialMediaSites { get; set; } = new();
    public int ItemNumber { get; set; }
    public int TotalCount { get; set; }
}