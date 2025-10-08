namespace eQuantic.Core.Outcomes.Errors;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
public interface IError
{
    /// <summary>
    /// Gets the error code (e.g., "USER_001", "VALIDATION_EMAIL").
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the type of error.
    /// </summary>
    ErrorType Type { get; }

    /// <summary>
    /// Gets the severity level of the error.
    /// </summary>
    ErrorSeverity Severity { get; }

    /// <summary>
    /// Gets additional metadata associated with the error.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the exception that caused this error, if any.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Gets the timestamp when the error occurred.
    /// </summary>
    DateTimeOffset Timestamp { get; }
}
