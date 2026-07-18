using eQuantic.Core.Outcomes.Errors;
using Microsoft.AspNetCore.Http;

// The library defines its own eQuantic.Core.Outcomes.Results.IResult; alias the framework's
// interface explicitly so the two never get confused in this file.
using IHttpResult = Microsoft.AspNetCore.Http.IResult;

namespace eQuantic.Core.Outcomes.AspNetCore;

/// <summary>
/// Extension methods for converting Result types to Minimal API results
/// (<see cref="Microsoft.AspNetCore.Http.IResult"/> / <c>TypedResults</c>), the Minimal API
/// counterpart of <see cref="ResultExtensions"/>. Failures map to RFC 7807 Problem Details with
/// the same status codes and <c>errors</c> payload as the MVC adapter, driven by
/// <see cref="OutcomeHttpMapping"/>.
/// </summary>
public static class HttpResultExtensions
{
    /// <summary>
    /// Converts a Result{T} to a Minimal API result: 200 OK with the value on success, RFC 7807
    /// Problem Details (status from <see cref="OutcomeHttpMapping"/>) on failure.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The Result{T} instance to convert to an HTTP response.</param>
    /// <param name="mapping">Custom error → HTTP mapping; defaults apply when omitted.</param>
    /// <example>
    /// <code>
    /// app.MapGet("/users/{id}", (int id, IUserService users) =>
    ///     users.GetUser(id).ToHttpResult());
    /// </code>
    /// </example>
    public static IHttpResult ToHttpResult<T>(this Result<T> result, OutcomeHttpMapping? mapping = null)
    {
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : CreateProblemResult(result.Errors, mapping ?? OutcomeHttpMapping.Default);
    }

    /// <summary>
    /// Converts a Result (without value) to a Minimal API result: 200 OK on success, RFC 7807
    /// Problem Details on failure.
    /// </summary>
    /// <param name="result">The Result instance (without value) to convert to an HTTP response.</param>
    /// <param name="mapping">Custom error → HTTP mapping; defaults apply when omitted.</param>
    public static IHttpResult ToHttpResult(this Result result, OutcomeHttpMapping? mapping = null)
    {
        return result.IsSuccess
            ? TypedResults.Ok()
            : CreateProblemResult(result.Errors, mapping ?? OutcomeHttpMapping.Default);
    }

    /// <summary>
    /// Converts a Result{T} to a Minimal API result with a custom success status code
    /// (e.g. 202 Accepted); failures map to RFC 7807 Problem Details.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The Result{T} instance to convert to an HTTP response.</param>
    /// <param name="successStatusCode">The HTTP status code to return when the result is successful.</param>
    /// <param name="mapping">Custom error → HTTP mapping; defaults apply when omitted.</param>
    public static IHttpResult ToHttpResult<T>(
        this Result<T> result,
        int successStatusCode,
        OutcomeHttpMapping? mapping = null)
    {
        return result.IsSuccess
            ? TypedResults.Json(result.Value, statusCode: successStatusCode)
            : CreateProblemResult(result.Errors, mapping ?? OutcomeHttpMapping.Default);
    }

    /// <summary>
    /// Converts a Result{T} to a 201 Created Minimal API result with a Location header;
    /// failures map to RFC 7807 Problem Details.
    /// </summary>
    /// <typeparam name="T">The type of the created resource contained in the result.</typeparam>
    /// <param name="result">The Result{T} instance containing the created resource.</param>
    /// <param name="location">URI of the created resource for the Location header (optional).</param>
    /// <param name="mapping">Custom error → HTTP mapping; defaults apply when omitted.</param>
    public static IHttpResult ToCreatedHttpResult<T>(
        this Result<T> result,
        string? location = null,
        OutcomeHttpMapping? mapping = null)
    {
        return result.IsSuccess
            ? TypedResults.Created(location, result.Value)
            : CreateProblemResult(result.Errors, mapping ?? OutcomeHttpMapping.Default);
    }

    /// <summary>
    /// Converts a Result{T} to a 201 Created Minimal API result, building the Location header
    /// from the created value; failures map to RFC 7807 Problem Details.
    /// </summary>
    /// <typeparam name="T">The type of the created resource contained in the result.</typeparam>
    /// <param name="result">The Result{T} instance containing the created resource.</param>
    /// <param name="locationFactory">Builds the Location URI from the created value.</param>
    /// <param name="mapping">Custom error → HTTP mapping; defaults apply when omitted.</param>
    /// <example>
    /// <code>
    /// app.MapPost("/users", (CreateUser request, IUserService users) =>
    ///     users.Create(request).ToCreatedHttpResult(user => $"/users/{user.Id}"));
    /// </code>
    /// </example>
    public static IHttpResult ToCreatedHttpResult<T>(
        this Result<T> result,
        Func<T, string> locationFactory,
        OutcomeHttpMapping? mapping = null)
    {
        ArgumentNullException.ThrowIfNull(locationFactory);

        return result.IsSuccess
            ? TypedResults.Created(locationFactory(result.Value), result.Value)
            : CreateProblemResult(result.Errors, mapping ?? OutcomeHttpMapping.Default);
    }

    /// <summary>
    /// Converts a Result{T} to a 204 No Content Minimal API result on success (the value is
    /// discarded); failures map to RFC 7807 Problem Details.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result (discarded on success).</typeparam>
    /// <param name="result">The Result{T} instance to convert to an HTTP response.</param>
    /// <param name="mapping">Custom error → HTTP mapping; defaults apply when omitted.</param>
    public static IHttpResult ToNoContentHttpResult<T>(this Result<T> result, OutcomeHttpMapping? mapping = null)
    {
        return result.IsSuccess
            ? TypedResults.NoContent()
            : CreateProblemResult(result.Errors, mapping ?? OutcomeHttpMapping.Default);
    }

    /// <summary>
    /// Converts a Result (without value) to a 204 No Content Minimal API result on success;
    /// failures map to RFC 7807 Problem Details.
    /// </summary>
    /// <param name="result">The Result instance (without value) to convert to an HTTP response.</param>
    /// <param name="mapping">Custom error → HTTP mapping; defaults apply when omitted.</param>
    public static IHttpResult ToNoContentHttpResult(this Result result, OutcomeHttpMapping? mapping = null)
    {
        return result.IsSuccess
            ? TypedResults.NoContent()
            : CreateProblemResult(result.Errors, mapping ?? OutcomeHttpMapping.Default);
    }

    private static IHttpResult CreateProblemResult(IReadOnlyList<IError> errors, OutcomeHttpMapping mapping)
    {
        var firstError = errors[0];

        // TypedResults.ValidationProblem always answers 400 — the canonical shape for
        // validation failures; custom status codes apply to the non-validation mappings.
        if (firstError.Type == ErrorType.Validation)
        {
            return TypedResults.ValidationProblem(
                errors: mapping.GetValidationErrors(errors),
                title: mapping.GetTitle(firstError),
                detail: mapping.GetDetail(firstError),
                extensions: new Dictionary<string, object?> { ["errors"] = mapping.GetErrorsPayload(errors) });
        }

        return TypedResults.Problem(
            title: mapping.GetTitle(firstError),
            detail: mapping.GetDetail(firstError),
            statusCode: mapping.GetStatusCode(firstError),
            extensions: new Dictionary<string, object?> { ["errors"] = mapping.GetErrorsPayload(errors) });
    }
}
