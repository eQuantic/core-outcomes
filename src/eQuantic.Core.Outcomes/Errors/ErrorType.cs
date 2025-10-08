namespace eQuantic.Core.Outcomes.Errors;

/// <summary>
/// Defines the type of error that occurred.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Validation error - input data did not meet requirements
    /// </summary>
    Validation,

    /// <summary>
    /// Resource not found
    /// </summary>
    NotFound,

    /// <summary>
    /// Conflict with existing data or state
    /// </summary>
    Conflict,

    /// <summary>
    /// User is not authenticated
    /// </summary>
    Unauthorized,

    /// <summary>
    /// User lacks permission for the operation
    /// </summary>
    Forbidden,

    /// <summary>
    /// Business rule violation
    /// </summary>
    BusinessRule,

    /// <summary>
    /// Technical/infrastructure error
    /// </summary>
    Technical,

    /// <summary>
    /// External service error
    /// </summary>
    External,

    /// <summary>
    /// General error
    /// </summary>
    Failure
}
