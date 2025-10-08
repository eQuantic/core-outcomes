namespace eQuantic.Core.Outcomes.Errors;

/// <summary>
/// Defines the severity level of an error.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational message
    /// </summary>
    Info,

    /// <summary>
    /// Warning - operation completed but with concerns
    /// </summary>
    Warning,

    /// <summary>
    /// Error - operation failed
    /// </summary>
    Error,

    /// <summary>
    /// Critical error - system stability affected
    /// </summary>
    Critical
}
