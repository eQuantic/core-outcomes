namespace eQuantic.Core.Outcomes.Errors;

/// <summary>
/// Represents a validation error with property-specific information.
/// </summary>
public record ValidationError : Error
{
    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    public string PropertyName { get; init; }

    /// <summary>
    /// Gets the value that was attempted to be set.
    /// </summary>
    public object? AttemptedValue { get; init; }

    /// <summary>
    /// Creates a new validation error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="propertyName">The property name that failed validation.</param>
    /// <param name="attemptedValue">The value that was attempted.</param>
    public ValidationError(
        string code,
        string message,
        string propertyName,
        object? attemptedValue = null)
        : base(code, message, ErrorType.Validation, ErrorSeverity.Error)
    {
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
    }
}
