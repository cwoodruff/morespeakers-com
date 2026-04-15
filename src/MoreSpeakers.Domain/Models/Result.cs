using System.Collections.Generic;

namespace MoreSpeakers.Domain;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public readonly struct Result : IEquatable<Result>
{
    private readonly Error? _error;

    private Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
        {
            throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
        }

        if (!isSuccess && error is null)
        {
            throw new ArgumentNullException(nameof(error), "A failed result must contain an error.");
        }

        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error that describes the failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is successful.</exception>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("A successful result does not contain an error.");

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Success<T>(T value) => new(value);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error that describes the failure.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with a value type parameter.
    /// </summary>
    /// <typeparam name="T">The type of the value that would have been returned on success.</typeparam>
    /// <param name="error">The error that describes the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(Error error) => new(error);

    /// <summary>
    /// Converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error to wrap.</param>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Indicates whether the current result is equal to another result.
    /// </summary>
    /// <param name="other">The result to compare with the current result.</param>
    /// <returns><see langword="true"/> when the results are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Result other) =>
        IsSuccess == other.IsSuccess &&
        EqualityComparer<Error?>.Default.Equals(_error, other._error);

    /// <summary>
    /// Indicates whether the current result is equal to a specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current result.</param>
    /// <returns><see langword="true"/> when the object is a matching result; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Result other && Equals(other);

    /// <summary>
    /// Returns the hash code for this result.
    /// </summary>
    /// <returns>A hash code for this result.</returns>
    public override int GetHashCode() => HashCode.Combine(IsSuccess, _error);

    /// <summary>
    /// Determines whether two results are equal.
    /// </summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when the results are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Result left, Result right) => left.Equals(right);

    /// <summary>
    /// Determines whether two results are not equal.
    /// </summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when the results differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Result left, Result right) => !left.Equals(right);
}

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
public readonly struct Result<T> : IEquatable<Result<T>>
{
    private readonly T? _value;
    private readonly Error? _error;

    internal Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    internal Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        IsSuccess = false;
        _value = default;
        _error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result does not contain a value.");

    /// <summary>
    /// Gets the error that describes the failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is successful.</exception>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("A successful result does not contain an error.");

    /// <summary>
    /// Converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    public static implicit operator Result<T>(T value) => Result.Success(value);

    /// <summary>
    /// Converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error to wrap.</param>
    public static implicit operator Result<T>(Error error) => Result.Failure<T>(error);

    /// <summary>
    /// Indicates whether the current result is equal to another result.
    /// </summary>
    /// <param name="other">The result to compare with the current result.</param>
    /// <returns><see langword="true"/> when the results are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Result<T> other) =>
        IsSuccess == other.IsSuccess &&
        EqualityComparer<T?>.Default.Equals(_value, other._value) &&
        EqualityComparer<Error?>.Default.Equals(_error, other._error);

    /// <summary>
    /// Indicates whether the current result is equal to a specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current result.</param>
    /// <returns><see langword="true"/> when the object is a matching result; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

    /// <summary>
    /// Returns the hash code for this result.
    /// </summary>
    /// <returns>A hash code for this result.</returns>
    public override int GetHashCode() => HashCode.Combine(IsSuccess, _value, _error);

    /// <summary>
    /// Determines whether two results are equal.
    /// </summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when the results are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two results are not equal.
    /// </summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when the results differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);
}
