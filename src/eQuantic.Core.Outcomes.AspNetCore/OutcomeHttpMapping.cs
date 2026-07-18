using eQuantic.Core.Outcomes.Errors;
using Microsoft.AspNetCore.Http;

namespace eQuantic.Core.Outcomes.AspNetCore;

/// <summary>
/// Maps result errors to HTTP semantics (status code, Problem Details title/detail and the
/// <c>errors</c> extension payload). Both the MVC (<c>ToActionResult</c>) and Minimal API
/// (<c>ToHttpResult</c>) adapters route through this type — derive from it and override the
/// virtual members to customize the mapping, then pass your instance to the extension overloads.
/// </summary>
/// <example>
/// <code>
/// public sealed class MyMapping : OutcomeHttpMapping
/// {
///     public override int GetStatusCode(IError error) =>
///         error.Type == ErrorType.BusinessRule
///             ? StatusCodes.Status409Conflict          // business rules as conflicts
///             : base.GetStatusCode(error);
/// }
///
/// return result.ToHttpResult(new MyMapping());
/// </code>
/// </example>
public class OutcomeHttpMapping
{
    /// <summary>The default mapping used when no instance is supplied.</summary>
    public static OutcomeHttpMapping Default { get; } = new();

    /// <summary>HTTP status code for an error. Follows the documented ErrorType mapping.</summary>
    /// <param name="error">Leading error of the failed result.</param>
    public virtual int GetStatusCode(IError error) => error.Type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError,
    };

    /// <summary>Problem Details title for an error.</summary>
    /// <param name="error">Leading error of the failed result.</param>
    public virtual string GetTitle(IError error) => error.Type switch
    {
        ErrorType.NotFound => "Not Found",
        ErrorType.Validation => "Validation Error",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.BusinessRule => "Business Rule Violation",
        _ => "Internal Server Error",
    };

    /// <summary>Problem Details detail text for an error.</summary>
    /// <param name="error">Leading error of the failed result.</param>
    public virtual string GetDetail(IError error) => error.Type switch
    {
        ErrorType.NotFound => "The requested resource was not found.",
        ErrorType.Validation => "One or more validation errors occurred.",
        ErrorType.Conflict => "A conflict occurred while processing your request.",
        ErrorType.Unauthorized => "Authentication is required to access this resource.",
        ErrorType.Forbidden => "You do not have permission to access this resource.",
        ErrorType.BusinessRule => "A business rule was violated.",
        _ => "An error occurred while processing your request.",
    };

    /// <summary>
    /// Validation errors grouped by property name (from the <c>PropertyName</c> metadata entry;
    /// ungrouped errors land under <c>General</c>) — the shape of the Problem Details
    /// <c>errors</c> dictionary.
    /// </summary>
    /// <param name="errors">All errors of the failed result.</param>
    public virtual Dictionary<string, string[]> GetValidationErrors(IReadOnlyList<IError> errors) =>
        errors
            .Where(e => e.Type == ErrorType.Validation)
            .GroupBy(e => e.Metadata.TryGetValue("PropertyName", out var property) ? property.ToString() : "General")
            .ToDictionary(
                g => g.Key ?? "General",
                g => g.Select(e => e.Message).ToArray());

    /// <summary>
    /// The structured <c>errors</c> extension payload (code, message, type, severity, timestamp
    /// per error) attached to every Problem Details response.
    /// </summary>
    /// <param name="errors">All errors of the failed result.</param>
    public virtual object GetErrorsPayload(IReadOnlyList<IError> errors) =>
        errors.Select(e => new
        {
            code = e.Code,
            message = e.Message,
            type = e.Type.ToString(),
            severity = e.Severity.ToString(),
            timestamp = e.Timestamp,
        }).ToList();
}
