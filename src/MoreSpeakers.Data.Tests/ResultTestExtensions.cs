using FluentAssertions;

using MoreSpeakers.Domain;

namespace MoreSpeakers.Data.Tests;

internal static class ResultTestExtensions
{
    public static void ShouldSucceed(this Result result)
    {
        result.IsSuccess.Should().BeTrue("expected the operation to succeed but it failed with {0}", result.ErrorMessage);
    }

    public static T ShouldSucceed<T>(this Result<T> result)
    {
        result.IsSuccess.Should().BeTrue("expected the operation to succeed but it failed with {0}", result.ErrorMessage);
        return result.Value;
    }

    public static Error ShouldFail(this Result result, string expectedCode)
    {
        result.IsFailure.Should().BeTrue("expected the operation to fail");
        result.Error.Code.Should().Be(expectedCode);
        return result.Error;
    }

    public static Error ShouldFail<T>(this Result<T> result, string expectedCode)
    {
        result.IsFailure.Should().BeTrue("expected the operation to fail");
        result.Error.Code.Should().Be(expectedCode);
        return result.Error;
    }
}
