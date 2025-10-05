using System.Globalization;

namespace MoreSpeakers.Domain.Extensions;

/// <summary>
/// String extensions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to title case.
    /// </summary>
    /// <param name="input">The string to convert</param>
    /// <returns>A string that is title cased</returns>
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }
}