using MoreSpeakers.Domain.Extensions;

namespace MoreSpeakers.Domain.Tests;

public class StringExtensionTests
{
    [Fact]
    public void ToTitleCaseSuccessfullyTitleCasesLowercase()
    {
        Assert.Equal("Hello World", "hello world".ToTitleCase());
    }

    [Fact]
    public void ToTitleCaseSuccessfullyTitleCasesTitleCase()
    {
        Assert.Equal("Hello World", "Hello World".ToTitleCase());
    }

    [Fact]
    public void ToTitleCaseSuccessfullyTitleCasesEmpty()
    {
        Assert.Equal(string.Empty, string.Empty.ToTitleCase());
    }
}