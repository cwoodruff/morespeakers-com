using MoreSpeakers.Domain;

namespace MoreSpeakers.Domain.Tests;

public class ResultTests
{
    [Fact]
    public void Error_stores_code_message_and_exception()
    {
        var exception = new InvalidOperationException("boom");

        var error = new Error("users.save.failed", "Unable to save the user.", exception);

        Assert.Equal("users.save.failed", error.Code);
        Assert.Equal("Unable to save the user.", error.Message);
        Assert.Same(exception, error.Exception);
    }

    [Fact]
    public void Success_without_value_creates_successful_result()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void Failure_without_value_creates_failed_result()
    {
        var error = new Error("users.save.failed", "Unable to save the user.");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Success_with_value_creates_successful_generic_result()
    {
        var result = Result.Success("speaker");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("speaker", result.Value);
        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void Failure_with_value_type_parameter_creates_failed_generic_result()
    {
        var error = new Error("users.find.failed", "Unable to find the user.");

        var result = Result.Failure<string>(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Implicit_conversion_wraps_value_in_successful_result()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Implicit_error_conversion_wraps_failure_results()
    {
        Error error = new("users.delete.failed", "Unable to delete the user.");

        Result result = error;
        Result<string> genericResult = error;

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.True(genericResult.IsFailure);
        Assert.Equal(error, genericResult.Error);
    }

    [Fact]
    public void Results_support_value_equality()
    {
        var error = new Error("users.delete.failed", "Unable to delete the user.");

        var left = Result.Success("speaker");
        Result<string> right = "speaker";
        var failedLeft = Result.Failure(error);
        var failedRight = Result.Failure(new Error("users.delete.failed", "Unable to delete the user."));

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.Equal(failedLeft, failedRight);
        Assert.True(failedLeft == failedRight);
    }
}
