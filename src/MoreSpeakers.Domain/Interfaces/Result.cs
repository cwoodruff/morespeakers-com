namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// A result encapsulation to wrap a result at a high-level.
/// </summary>
public sealed class Result
{
    private Result(bool isSuccessful)
    {
        IsSuccessful = isSuccessful;
        Errors = [];
    }

    private Result(IEnumerable<string> errors)
    {
        Errors = errors;
        IsSuccessful = false;
    }

    public bool IsSuccessful { get; private set; }

    public IEnumerable<string> Errors { get; }

    public static Result Success() => new(true);

    public static Result Failure(IEnumerable<string> errors) => new(errors);
    public static Result Failure(Exception ex) => new([ex.Message]);

    public static Result Failure(string message) => new([message]);
}