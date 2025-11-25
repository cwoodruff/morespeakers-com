using System.Text.RegularExpressions;

namespace MoreSpeakers.Web.Services;

public static class SocialMediaSiteHelper
{
    /// <summary>
    /// Parses the posted form collection for indexed pairs like
    /// Input.SocialId[7] and Input.SocialMediaSiteId[7] and returns
    /// a dictionary keyed by the index with the matched values.
    /// </summary>
    /// <param name="form">The posted form collection (e.g., Request.Form)</param>
    /// <param name="leftKeyPrefix">The left key prefix (e.g., "Input.SocialId")</param>
    /// <param name="rightKeyPrefix">The right key prefix (e.g., "Input.SocialMediaSiteId")</param>
    /// <returns>
    /// Dictionary where the key is the index (e.g., 7) and value is a tuple of (leftValue, rightValue).
    /// Only indices present for both sides are included.
    /// </returns>
    private static Dictionary<int, (string leftValue, string rightValue)> ParseIndexedPairs(
        IFormCollection form,
        string leftKeyPrefix,
        string rightKeyPrefix)
    {
        var leftPattern = new Regex($"^" + Regex.Escape(leftKeyPrefix) + "\\[(\\d+)\\]$",
            RegexOptions.Compiled);
        var rightPattern = new Regex($"^" + Regex.Escape(rightKeyPrefix) + "\\[(\\d+)\\]$",
            RegexOptions.Compiled);

        var leftByIndex = new Dictionary<int, string>();
        var rightByIndex = new Dictionary<int, string>();

        foreach (var key in form.Keys)
        {
            var leftMatch = leftPattern.Match(key);
            if (leftMatch.Success && int.TryParse(leftMatch.Groups[1].Value, out var li))
            {
                var v = form[key].ToString();
                leftByIndex[li] = v;
                continue;
            }

            var rightMatch = rightPattern.Match(key);
            if (rightMatch.Success && int.TryParse(rightMatch.Groups[1].Value, out var ri))
            {
                var v = form[key].ToString();
                rightByIndex[ri] = v;
            }
        }

        var result = new Dictionary<int, (string leftValue, string rightValue)>();
        foreach (var idx in leftByIndex.Keys)
        {
            if (!rightByIndex.TryGetValue(idx, out var rightVal))
            {
                continue; // only include indices present on both sides
            }

            var leftVal = leftByIndex[idx];

            // Skip completely empty pairs
            if (string.IsNullOrWhiteSpace(leftVal) && string.IsNullOrWhiteSpace(rightVal))
            {
                continue;
            }

            result[idx] = (leftVal.Trim(), rightVal.Trim());
        }

        return result;
    }

    /// <summary>
    /// Specialized helper for Social Media fields. It reads pairs of
    /// Input.SocialId[n] and Input.SocialMediaSiteId[n] from the provided form
    /// and returns a dictionary keyed by index with (SiteId, SocialId).
    /// </summary>
    public static Dictionary<int, (int SiteId, string SocialId)> ParseSocialMediaPairs(IFormCollection form)
    {
        var pairs = ParseIndexedPairs(form, "Input.SocialId", "Input.SocialMediaSiteId");
        var result = new Dictionary<int, (int SiteId, string SocialId)>();

        foreach (var kvp in pairs)
        {
            // kvp.Value.rightValue is the SocialMediaSiteId, kvp.Value.leftValue is SocialId
            if (!int.TryParse(kvp.Value.rightValue, out var siteId))
            {
                continue; // ignore invalid site ids
            }

            var socialId = kvp.Value.leftValue.Trim();
            if (string.IsNullOrWhiteSpace(socialId))
            {
                continue; // ignore rows without a social id
            }

            result[kvp.Key] = (siteId, socialId);
        }

        return result;
    }
}