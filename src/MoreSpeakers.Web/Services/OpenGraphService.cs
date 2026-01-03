using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Services;

public class OpenGraphService : IOpenGraphService
{
    public Dictionary<string, string> GenerateUserMetadata(User user, string profileUrl)
    {
        var metadata = new Dictionary<string, string>
        {
            { "og:title", user.FullName + "'s MoreSpeakers.com Profile"},
            { "og:type", "profile" },
            { "og:url", profileUrl },
            { "og:description", Truncate(user.Bio, 300) },
            { "og:profile:first_name", user.FirstName },
            { "og:profile:last_name", user.LastName }
        };

        if (!string.IsNullOrWhiteSpace(user.HeadshotUrl))
        {
            metadata.Add("og:image", user.HeadshotUrl);
        }

        return metadata;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength).TrimEnd() + "...";
    }
}