using eQuantic.Core.Outcomes.Errors;

namespace eQuantic.Core.Outcomes;

/// <summary>
/// Represents the result of an operation that returns a value.
/// Implements Railway-Oriented Programming pattern.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed record Result<T>
{
    private readonly T? _value;
    private readonly IReadOnlyList<IError> _errors;
    private readonly IReadOnlyDictionary<string, object> _metadata;

    internal Result(
        T? value,
        IReadOnlyList<IError> errors,
        bool isSuccess,
        IReadOnlyDictionary<string, object>? metadata = null,
        string? correlationId = null,
        string? traceId = null,
        TimeSpan? executionTime = null)
    {
        _value = value;
        _errors = errors;
        _metadata = metadata ?? new Dictionary<string, object>();
        IsSuccess = isSuccess;
        CorrelationId = correlationId;
        TraceId = traceId;
        ExecutionTime = executionTime;
    }

    /// <summary>
    /// Gets the value if the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value on a failed result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result. Check IsSuccess before accessing Value.");

    /// <summary>
    /// Gets the errors if the result is failed.
    /// </summary>
    public IReadOnlyList<IError> Errors => _errors;

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets a value indicating whether the result is failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the first error if the result is failed.
    /// </summary>
    public IError? FirstError => _errors.FirstOrDefault();

    /// <summary>
    /// Gets the correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the trace ID for distributed tracing.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the execution time of the operation.
    /// </summary>
    public TimeSpan? ExecutionTime { get; init; }

    /// <summary>
    /// Gets additional metadata associated with the result.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(value, Array.Empty<IError>(), true);
    }

    /// <summary>
    /// Creates a failed result with errors.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure(params IError[] errors)
    {
        if (errors == null || errors.Length == 0)
        {
            throw new ArgumentException("At least one error must be provided for a failed result.", nameof(errors));
        }

        return new Result<T>(default, errors, false);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure(IError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(default, new[] { error }, false);
    }

    /// <summary>
    /// Creates a failed result from errors collection.
    /// </summary>
    /// <param name="errors">The errors collection.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure(IEnumerable<IError> errors)
    {
        var errorsList = errors?.ToList() ?? throw new ArgumentNullException(nameof(errors));

        if (errorsList.Count == 0)
        {
            throw new ArgumentException("At least one error must be provided for a failed result.", nameof(errors));
        }

        return new Result<T>(default, errorsList, false);
    }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Implicitly converts an error array to a failed result.
    /// </summary>
    public static implicit operator Result<T>(Error[] errors) => Failure(errors);
}

/// <summary>
/// Represents the result of an operation without a return value.
/// </summary>
public sealed record Result
{
    private readonly IReadOnlyList<IError> _errors;
    private readonly IReadOnlyDictionary<string, object> _metadata;

    internal Result(
        IReadOnlyList<IError> errors,
        bool isSuccess,
        IReadOnlyDictionary<string, object>? metadata = null,
        string? correlationId = null,
        string? traceId = null,
        TimeSpan? executionTime = null)
    {
        _errors = errors;
        _metadata = metadata ?? new Dictionary<string, object>();
        IsSuccess = isSuccess;
        CorrelationId = correlationId;
        TraceId = traceId;
        ExecutionTime = executionTime;
    }

    /// <summary>
    /// Gets the errors if the result is failed.
    /// </summary>
    public IReadOnlyList<IError> Errors => _errors;

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets a value indicating whether the result is failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the first error if the result is failed.
    /// </summary>
    public IError? FirstError => _errors.FirstOrDefault();

    /// <summary>
    /// Gets the correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the trace ID for distributed tracing.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the execution time of the operation.
    /// </summary>
    public TimeSpan? ExecutionTime { get; init; }

    /// <summary>
    /// Gets additional metadata associated with the result.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new(Array.Empty<IError>(), true);

    /// <summary>
    /// Creates a failed result with errors.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(params IError[] errors)
    {
        if (errors == null || errors.Length == 0)
        {
            throw new ArgumentException("At least one error must be provided for a failed result.", nameof(errors));
        }

        return new Result(errors, false);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(IError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(new[] { error }, false);
    }

    /// <summary>
    /// Creates a failed result from errors collection.
    /// </summary>
    /// <param name="errors">The errors collection.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(IEnumerable<IError> errors)
    {
        var errorsList = errors?.ToList() ?? throw new ArgumentNullException(nameof(errors));

        if (errorsList.Count == 0)
        {
            throw new ArgumentException("At least one error must be provided for a failed result.", nameof(errors));
        }

        return new Result(errorsList, false);
    }

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Implicitly converts an error array to a failed result.
    /// </summary>
    public static implicit operator Result(Error[] errors) => Failure(errors);
}
