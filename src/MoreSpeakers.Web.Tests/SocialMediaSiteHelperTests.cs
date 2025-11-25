using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Tests;

public class SocialMediaSiteHelperTests
{
    [Fact]
    public void ParseValidSocialMediaPairs_ReturnsExpectedResults()
    {
        // Arrange
        var expected = new Dictionary<int, (int SiteId, string SocialId)>
        {
            { 0, (1234, "https://twitter.com/user") },
        };
        var input = new Dictionary<string, StringValues>
        {
            { "Input.SocialId[0]" , new StringValues("https://twitter.com/user") },
            { "Input.SocialMediaSiteId[0]" , new StringValues("1234") },
        };
        var formCollection = new FormCollection(input);
        // Act
        Dictionary<int, (int SiteId, string SocialId)> result = SocialMediaSiteHelper.ParseSocialMediaPairs(formCollection);
        // Assert
        Assert.Equal(expected.Count, result.Count);
        foreach (var kvp in expected)
        {
            Assert.True(result.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, result[kvp.Key]);
        }
    }

    [Fact]
    public void ParseSocialMediaPairs_WithInvalidSocialMediaSiteId_ReturnsExpectedResults()
    {
        // Arrange
        var input = new Dictionary<string, StringValues>
        {
            { "Input.SocialId[0]" , new StringValues("https://twitter.com/user") },
            { "Input.SocialMediaSiteId" , new StringValues("1234") },
        };
        var formCollection = new FormCollection(input);
        // Act
        Dictionary<int, (int SiteId, string SocialId)> result = SocialMediaSiteHelper.ParseSocialMediaPairs(formCollection);
        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseSocialMediaPairs_WithInvalidSocialId_ReturnsExpectedResults()
    {
        // Arrange
        var input = new Dictionary<string, StringValues>
        {
            { "Input.SocialId" , new StringValues("https://twitter.com/user") },
            { "Input.SocialMediaSiteId[0]" , new StringValues("1234") },
        };
        var formCollection = new FormCollection(input);
        // Act
        Dictionary<int, (int SiteId, string SocialId)> result = SocialMediaSiteHelper.ParseSocialMediaPairs(formCollection);
        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseSocialMediaPairs_WithMismatchedIndexes_ReturnsExpectedResults()
    {
        // Arrange
        var input = new Dictionary<string, StringValues>
        {
            { "Input.SocialId[0]" , new StringValues("https://twitter.com/user") },
            { "Input.SocialMediaSiteId[1]" , new StringValues("1234") },
        };
        var formCollection = new FormCollection(input);
        // Act
        Dictionary<int, (int SiteId, string SocialId)> result = SocialMediaSiteHelper.ParseSocialMediaPairs(formCollection);
        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseSocialMediaPairs_WithEmptySocialIdValue_ReturnsExpectedResults()
    {
        // Arrange
        var input = new Dictionary<string, StringValues>
        {
            { "Input.SocialId[0]" , new StringValues("") },
            { "Input.SocialMediaSiteId[0]" , new StringValues("1234") },
        };
        var formCollection = new FormCollection(input);
        // Act
        Dictionary<int, (int SiteId, string SocialId)> result = SocialMediaSiteHelper.ParseSocialMediaPairs(formCollection);
        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseSocialMediaPairs_WithEmptySocialMediaSiteIdValue_ReturnsExpectedResults()
    {
        // Arrange
        var input = new Dictionary<string, StringValues>
        {
            { "Input.SocialId[0]" , new StringValues("SocialId") },
            { "Input.SocialMediaSiteId[0]" , new StringValues("") },
        };
        var formCollection = new FormCollection(input);
        // Act
        Dictionary<int, (int SiteId, string SocialId)> result = SocialMediaSiteHelper.ParseSocialMediaPairs(formCollection);
        // Assert
        Assert.Empty(result);
    }
}