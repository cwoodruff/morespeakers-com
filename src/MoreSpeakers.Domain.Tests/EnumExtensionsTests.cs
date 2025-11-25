using System.ComponentModel;
using MoreSpeakers.Domain.Extensions;

namespace MoreSpeakers.Domain.Tests;

public class EnumExtensionsTests
{
    [Fact]
    public void GetDescriptionSucceedsWithEnum()
    {
        Assert.Equal("New Speaker", SomeEnum.NewSpeaker.GetDescription());
    }

    [Fact]
    public void GetDescriptionSucceedsWithStruct()
    {
        Assert.Empty(NotAnEnum.Value.GetDescription());
    }

    #region test-specific testing types
    private enum SomeEnum
    {
        [Description("New Speaker")]
        NewSpeaker = 1,

        [Description("Experienced Speaker")]
        ExperiencedSpeaker = 2
    }

    private struct NotAnEnum
    {
        public static int Value;
    }
    #endregion
}