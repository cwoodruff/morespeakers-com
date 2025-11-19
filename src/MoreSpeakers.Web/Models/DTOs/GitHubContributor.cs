using System.Text.Json.Serialization;

namespace MoreSpeakers.Web.Models.DTOs;

public class GitHubContributor
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("contributions")]
    public int Contributions { get; set; }
}