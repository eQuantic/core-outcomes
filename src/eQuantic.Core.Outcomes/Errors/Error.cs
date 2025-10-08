namespace eQuantic.Core.Outcomes.Errors;

/// <summary>
/// Represents a general error.
/// </summary>
public record Error : IError
{
    /// <inheritdoc />
    public string Code { get; init; }

    /// <inheritdoc />
    public string Message { get; init; }

    /// <inheritdoc />
    public ErrorType Type { get; init; }

    /// <inheritdoc />
    public ErrorSeverity Severity { get; init; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <inheritdoc />
    public Exception? Exception { get; init; }

    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Creates a new error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="type">The error type.</param>
    /// <param name="severity">The error severity.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="exception">The exception that caused this error.</param>
    public Error(
        string code,
        string message,
        ErrorType type = ErrorType.Failure,
        ErrorSeverity severity = ErrorSeverity.Error,
        IReadOnlyDictionary<string, object>? metadata = null,
        Exception? exception = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Severity = severity;
        Metadata = metadata ?? new Dictionary<string, object>();
        Exception = exception;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="code">The error code that identifies the validation failure.</param>
    /// <param name="message">A descriptive message explaining the validation failure.</param>
    /// <param name="propertyName">Optional. The name of the property that failed validation.</param>
    /// <returns>A new <see cref="Error"/> instance configured for validation errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.Validation("EMAIL_INVALID", "Email address is not valid", "Email");
    /// </code>
    /// </example>
    public static Error Validation(string code, string message, string? propertyName = null)
    {
        var metadata = propertyName != null
            ? new Dictionary<string, object> { ["PropertyName"] = propertyName }
            : new Dictionary<string, object>();

        return new Error(code, message, ErrorType.Validation, ErrorSeverity.Error, metadata);
    }

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    /// <param name="code">The error code that identifies the not found error.</param>
    /// <param name="message">A descriptive message explaining what was not found.</param>
    /// <param name="resourceId">Optional. The identifier of the resource that was not found.</param>
    /// <returns>A new <see cref="Error"/> instance configured for not found errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.NotFound("USER_NOT_FOUND", "User with the specified ID was not found", userId.ToString());
    /// </code>
    /// </example>
    public static Error NotFound(string code, string message, string? resourceId = null)
    {
        var metadata = resourceId != null
            ? new Dictionary<string, object> { ["ResourceId"] = resourceId }
            : new Dictionary<string, object>();

        return new Error(code, message, ErrorType.NotFound, ErrorSeverity.Error, metadata);
    }

    /// <summary>
    /// Creates a conflict error indicating a resource state conflict.
    /// </summary>
    /// <param name="code">The error code that identifies the conflict.</param>
    /// <param name="message">A descriptive message explaining the nature of the conflict.</param>
    /// <returns>A new <see cref="Error"/> instance configured for conflict errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.Conflict("DUPLICATE_EMAIL", "An account with this email already exists");
    /// </code>
    /// </example>
    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict, ErrorSeverity.Error);
    }

    /// <summary>
    /// Creates an unauthorized error indicating authentication failure.
    /// </summary>
    /// <param name="code">The error code that identifies the authentication failure.</param>
    /// <param name="message">A descriptive message explaining the authentication issue.</param>
    /// <returns>A new <see cref="Error"/> instance configured for unauthorized errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.Unauthorized("INVALID_CREDENTIALS", "The provided credentials are invalid");
    /// </code>
    /// </example>
    public static Error Unauthorized(string code, string message)
    {
        return new Error(code, message, ErrorType.Unauthorized, ErrorSeverity.Error);
    }

    /// <summary>
    /// Creates a forbidden error indicating insufficient permissions.
    /// </summary>
    /// <param name="code">The error code that identifies the permission denial.</param>
    /// <param name="message">A descriptive message explaining why access is forbidden.</param>
    /// <returns>A new <see cref="Error"/> instance configured for forbidden errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.Forbidden("INSUFFICIENT_PERMISSIONS", "You do not have permission to access this resource");
    /// </code>
    /// </example>
    public static Error Forbidden(string code, string message)
    {
        return new Error(code, message, ErrorType.Forbidden, ErrorSeverity.Error);
    }

    /// <summary>
    /// Creates a business rule error indicating a violation of domain business rules.
    /// </summary>
    /// <param name="code">The error code that identifies the business rule violation.</param>
    /// <param name="message">A descriptive message explaining which business rule was violated.</param>
    /// <returns>A new <see cref="Error"/> instance configured for business rule errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.BusinessRule("INSUFFICIENT_BALANCE", "Account balance is insufficient for this transaction");
    /// </code>
    /// </example>
    public static Error BusinessRule(string code, string message)
    {
        return new Error(code, message, ErrorType.BusinessRule, ErrorSeverity.Error);
    }

    /// <summary>
    /// Creates a technical error indicating a system or infrastructure failure.
    /// </summary>
    /// <param name="code">The error code that identifies the technical failure.</param>
    /// <param name="message">A descriptive message explaining the technical issue.</param>
    /// <param name="exception">Optional. The exception that caused the technical error.</param>
    /// <returns>A new <see cref="Error"/> instance configured for technical errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.Technical("DATABASE_CONNECTION_FAILED", "Unable to connect to the database", dbException);
    /// </code>
    /// </example>
    public static Error Technical(string code, string message, Exception? exception = null)
    {
        return new Error(code, message, ErrorType.Technical, ErrorSeverity.Error, exception: exception);
    }

    /// <summary>
    /// Creates an external service error indicating a failure in a third-party or external system.
    /// </summary>
    /// <param name="code">The error code that identifies the external service failure.</param>
    /// <param name="message">A descriptive message explaining the external service issue.</param>
    /// <param name="exception">Optional. The exception that occurred during the external service call.</param>
    /// <returns>A new <see cref="Error"/> instance configured for external service errors.</returns>
    /// <example>
    /// <code>
    /// var error = Error.External("PAYMENT_GATEWAY_UNAVAILABLE", "The payment gateway is currently unavailable", httpException);
    /// </code>
    /// </example>
    public static Error External(string code, string message, Exception? exception = null)
    {
        return new Error(code, message, ErrorType.External, ErrorSeverity.Error, exception: exception);
    }

    /// <summary>
    /// Creates an error from an existing exception, automatically extracting the message and attaching the exception.
    /// </summary>
    /// <param name="exception">The exception to convert into an error.</param>
    /// <param name="code">Optional. The error code. If not provided, the exception type name will be used.</param>
    /// <param name="type">Optional. The error type. Defaults to <see cref="ErrorType.Technical"/>.</param>
    /// <returns>A new <see cref="Error"/> instance created from the exception.</returns>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some operation that throws
    /// }
    /// catch (Exception ex)
    /// {
    ///     var error = Error.FromException(ex, "OPERATION_FAILED", ErrorType.Technical);
    /// }
    /// </code>
    /// </example>
    public static Error FromException(Exception exception, string? code = null, ErrorType type = ErrorType.Technical)
    {
        return new Error(
            code ?? exception.GetType().Name,
            exception.Message,
            type,
            ErrorSeverity.Error,
            exception: exception);
    }
}
