namespace MoreSpeakers.Domain;

/// <summary>
/// Represents a structured error for an expected failure.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> record.
    /// </summary>
    /// <param name="code">The machine-readable error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <param name="exception">The optional underlying exception.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is empty.
    /// </exception>
    public Error(string code, string message, Exception? exception = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("An error code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("An error message is required.", nameof(message));
        }

        Code = code;
        Message = message;
        Exception = exception;
    }

    /// <summary>
    /// Gets the machine-readable error code.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the optional underlying exception.
    /// </summary>
    public Exception? Exception { get; init; }
}
