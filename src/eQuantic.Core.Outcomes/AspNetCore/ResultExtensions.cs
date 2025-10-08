using eQuantic.Core.Outcomes.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eQuantic.Core.Outcomes.AspNetCore;

/// <summary>
/// Extension methods for converting Result types to ASP.NET Core action results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result{T} to an appropriate ASP.NET Core IActionResult with automatic HTTP status code mapping.
    /// </summary>
    /// <remarks>
    /// This method provides seamless integration between the Result pattern and ASP.NET Core MVC/Web API.
    /// It automatically maps success to HTTP 200 OK with the result value in the response body, and failures
    /// to appropriate HTTP error responses using RFC 7807 Problem Details format.
    ///
    /// HTTP Status Code Mapping (based on ErrorType):
    /// - Success: 200 OK with value in response body
    /// - NotFound: 404 Not Found
    /// - Validation: 400 Bad Request
    /// - Conflict: 409 Conflict
    /// - Unauthorized: 401 Unauthorized
    /// - Forbidden: 403 Forbidden
    /// - BusinessRule: 422 Unprocessable Entity
    /// - Unexpected/Other: 500 Internal Server Error
    ///
    /// Error responses follow RFC 7807 Problem Details specification with additional error metadata.
    /// </remarks>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The Result{T} instance to convert to an HTTP response.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the operation outcome:
    /// - <see cref="OkObjectResult"/> with the value when successful
    /// - Problem Details response (NotFoundObjectResult, BadRequestObjectResult, etc.) when failed
    /// </returns>
    /// <example>
    /// Basic usage in an ASP.NET Core controller:
    /// <code>
    /// [HttpGet("{id}")]
    /// public IActionResult GetUser(int id)
    /// {
    ///     var result = _userService.GetUserById(id);
    ///     return result.ToActionResult();
    ///
    ///     // Success: Returns 200 OK with user data
    ///     // NotFound: Returns 404 with Problem Details
    ///     // Other errors: Returns appropriate status code with Problem Details
    /// }
    /// </code>
    /// </example>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : CreateProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a Result (without value) to an appropriate ASP.NET Core IActionResult with automatic HTTP status code mapping.
    /// </summary>
    /// <remarks>
    /// This method is ideal for operations that don't return data (e.g., delete operations, void commands).
    /// It automatically maps success to HTTP 200 OK without response body, and failures to appropriate
    /// HTTP error responses using RFC 7807 Problem Details format.
    ///
    /// HTTP Status Code Mapping (based on ErrorType):
    /// - Success: 200 OK (no response body)
    /// - NotFound: 404 Not Found
    /// - Validation: 400 Bad Request
    /// - Conflict: 409 Conflict
    /// - Unauthorized: 401 Unauthorized
    /// - Forbidden: 403 Forbidden
    /// - BusinessRule: 422 Unprocessable Entity
    /// - Unexpected/Other: 500 Internal Server Error
    ///
    /// Error responses follow RFC 7807 Problem Details specification with additional error metadata.
    /// </remarks>
    /// <param name="result">The Result instance (without value) to convert to an HTTP response.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the operation outcome:
    /// - <see cref="OkResult"/> (200 OK) when successful
    /// - Problem Details response (NotFoundObjectResult, BadRequestObjectResult, etc.) when failed
    /// </returns>
    /// <example>
    /// Usage in an ASP.NET Core controller for operations without return values:
    /// <code>
    /// [HttpPost("send-email")]
    /// public IActionResult SendNotification(NotificationRequest request)
    /// {
    ///     var result = _notificationService.SendEmail(request);
    ///     return result.ToActionResult();
    ///
    ///     // Success: Returns 200 OK (no body)
    ///     // Validation error: Returns 400 Bad Request with Problem Details
    ///     // Other errors: Returns appropriate status code with Problem Details
    /// }
    /// </code>
    /// </example>
    public static IActionResult ToActionResult(this Result result)
    {
        return result.IsSuccess
            ? new OkResult()
            : CreateProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a Result{T} to an ASP.NET Core IActionResult with a custom HTTP status code for successful operations.
    /// </summary>
    /// <remarks>
    /// This overload allows you to specify a custom HTTP status code for successful results while maintaining
    /// automatic error mapping to RFC 7807 Problem Details. This is useful for REST API scenarios where you need
    /// to return status codes other than 200 OK, such as 201 Created, 202 Accepted, or 206 Partial Content.
    ///
    /// Common success status codes:
    /// - 200 (OK): Standard success response
    /// - 201 (Created): Resource successfully created
    /// - 202 (Accepted): Request accepted for processing
    /// - 204 (No Content): Success with no response body
    /// - 206 (Partial Content): Partial resource returned
    ///
    /// Error responses still follow the standard ErrorType mapping to HTTP status codes with RFC 7807 Problem Details.
    /// </remarks>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The Result{T} instance to convert to an HTTP response.</param>
    /// <param name="successStatusCode">The HTTP status code to return when the result is successful (e.g., 201 for Created, 202 for Accepted).</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the operation outcome:
    /// - <see cref="ObjectResult"/> with the specified status code and value when successful
    /// - Problem Details response (NotFoundObjectResult, BadRequestObjectResult, etc.) when failed
    /// </returns>
    /// <example>
    /// Usage with custom status codes for different scenarios:
    /// <code>
    /// [HttpPost]
    /// public IActionResult CreateUser(CreateUserRequest request)
    /// {
    ///     var result = _userService.CreateUser(request);
    ///     // Return 201 Created on success instead of 200 OK
    ///     return result.ToActionResult(StatusCodes.Status201Created);
    /// }
    ///
    /// [HttpPost("async-process")]
    /// public IActionResult StartAsyncProcess(ProcessRequest request)
    /// {
    ///     var result = _processService.EnqueueProcess(request);
    ///     // Return 202 Accepted for asynchronous operations
    ///     return result.ToActionResult(StatusCodes.Status202Accepted);
    /// }
    /// </code>
    /// </example>
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        int successStatusCode)
    {
        return result.IsSuccess
            ? new ObjectResult(result.Value) { StatusCode = successStatusCode }
            : CreateProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a Result{T} to a CreatedAtAction result with automatic Location header generation following REST API best practices.
    /// </summary>
    /// <remarks>
    /// This method is specifically designed for HTTP POST operations that create new resources. It follows REST API
    /// best practices by returning HTTP 201 Created with a Location header pointing to the newly created resource.
    /// The Location header is automatically constructed using the specified action name and route values.
    ///
    /// REST API Best Practices:
    /// - Returns 201 Created on success with the created resource in the response body
    /// - Sets the Location header to the URI where the created resource can be accessed
    /// - Allows clients to immediately access the created resource using the Location header
    /// - Error responses use RFC 7807 Problem Details format
    ///
    /// The Location header format: https://yourdomain.com/api/controller/{actionName}?{routeValues}
    /// </remarks>
    /// <typeparam name="T">The type of the created resource contained in the result.</typeparam>
    /// <param name="result">The Result{T} instance containing the created resource.</param>
    /// <param name="actionName">
    /// The name of the action method that retrieves the created resource (typically a GET endpoint).
    /// This is used to construct the Location header URI.
    /// </param>
    /// <param name="routeValues">
    /// An object containing route values (typically the resource ID) used to construct the Location header URI.
    /// Common pattern: new { id = createdResource.Id }
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the creation outcome:
    /// - <see cref="CreatedAtActionResult"/> (201 Created) with Location header and created resource when successful
    /// - Problem Details response (BadRequestObjectResult, ConflictObjectResult, etc.) when failed
    /// </returns>
    /// <example>
    /// Typical usage in a REST API controller for resource creation:
    /// <code>
    /// [HttpPost]
    /// public IActionResult CreateUser(CreateUserRequest request)
    /// {
    ///     var result = _userService.CreateUser(request);
    ///     return result.ToCreatedAtActionResult(
    ///         nameof(GetUser),
    ///         new { id = result.Value?.Id }
    ///     );
    ///
    ///     // Success Response:
    ///     // - Status: 201 Created
    ///     // - Location: https://api.example.com/users/123
    ///     // - Body: { "id": 123, "name": "John Doe", ... }
    ///
    ///     // Validation Error Response:
    ///     // - Status: 400 Bad Request
    ///     // - Body: RFC 7807 Problem Details with validation errors
    /// }
    ///
    /// [HttpGet("{id}")]
    /// public IActionResult GetUser(int id)
    /// {
    ///     var result = _userService.GetUserById(id);
    ///     return result.ToActionResult();
    /// }
    /// </code>
    /// </example>
    public static IActionResult ToCreatedAtActionResult<T>(
        this Result<T> result,
        string actionName,
        object? routeValues = null)
    {
        return result.IsSuccess
            ? new CreatedAtActionResult(actionName, null, routeValues, result.Value)
            : CreateProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a Result{T} to a NoContent (204) result on success, discarding the value and following REST API best practices for update/delete operations.
    /// </summary>
    /// <remarks>
    /// This method is ideal for HTTP PUT, PATCH, or DELETE operations where the operation succeeds but no content
    /// needs to be returned to the client. It follows REST API best practices by returning HTTP 204 No Content,
    /// which indicates successful processing without a response body.
    ///
    /// REST API Best Practices for 204 No Content:
    /// - Use for successful updates where the client doesn't need the updated resource
    /// - Use for successful deletes where the resource no longer exists
    /// - Use for successful operations where the result is self-evident
    /// - Reduces bandwidth usage by not sending unnecessary response bodies
    /// - Error responses still use RFC 7807 Problem Details format
    ///
    /// Note: The result value (if any) is intentionally discarded when successful.
    /// </remarks>
    /// <typeparam name="T">The type of the value contained in the result (will be discarded on success).</typeparam>
    /// <param name="result">The Result{T} instance to convert to an HTTP response.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the operation outcome:
    /// - <see cref="NoContentResult"/> (204 No Content) when successful, with no response body
    /// - Problem Details response (NotFoundObjectResult, BadRequestObjectResult, etc.) when failed
    /// </returns>
    /// <example>
    /// Usage in REST API controllers for update and delete operations:
    /// <code>
    /// [HttpPut("{id}")]
    /// public IActionResult UpdateUser(int id, UpdateUserRequest request)
    /// {
    ///     var result = _userService.UpdateUser(id, request);
    ///     return result.ToNoContentResult();
    ///
    ///     // Success: Returns 204 No Content (client knows update succeeded)
    ///     // NotFound: Returns 404 with Problem Details
    ///     // Validation: Returns 400 with Problem Details
    /// }
    ///
    /// [HttpDelete("{id}")]
    /// public IActionResult DeleteUser(int id)
    /// {
    ///     var result = _userService.DeleteUser(id);
    ///     return result.ToNoContentResult();
    ///
    ///     // Success: Returns 204 No Content (resource deleted)
    ///     // NotFound: Returns 404 with Problem Details
    ///     // Conflict: Returns 409 with Problem Details (e.g., resource in use)
    /// }
    /// </code>
    /// </example>
    public static IActionResult ToNoContentResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? new NoContentResult()
            : CreateProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a Result (without value) to a NoContent (204) result on success, following REST API best practices for update/delete operations.
    /// </summary>
    /// <remarks>
    /// This method is specifically designed for HTTP PUT, PATCH, or DELETE operations that don't return data.
    /// It follows REST API best practices by returning HTTP 204 No Content, which indicates successful
    /// processing without a response body. This is the preferred method for operations without return values.
    ///
    /// REST API Best Practices for 204 No Content:
    /// - Use for successful updates where the client doesn't need the updated resource
    /// - Use for successful deletes where the resource no longer exists
    /// - Use for successful operations where the result is self-evident
    /// - Reduces bandwidth usage by not sending unnecessary response bodies
    /// - Error responses still use RFC 7807 Problem Details format
    ///
    /// This is semantically more correct than returning 200 OK with no body.
    /// </remarks>
    /// <param name="result">The Result instance (without value) to convert to an HTTP response.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the operation outcome:
    /// - <see cref="NoContentResult"/> (204 No Content) when successful, with no response body
    /// - Problem Details response (NotFoundObjectResult, BadRequestObjectResult, etc.) when failed
    /// </returns>
    /// <example>
    /// Usage in REST API controllers for void update and delete operations:
    /// <code>
    /// [HttpPatch("{id}/activate")]
    /// public IActionResult ActivateUser(int id)
    /// {
    ///     var result = _userService.ActivateUser(id);
    ///     return result.ToNoContentResult();
    ///
    ///     // Success: Returns 204 No Content (activation succeeded)
    ///     // NotFound: Returns 404 with Problem Details
    ///     // BusinessRule: Returns 422 with Problem Details (e.g., already active)
    /// }
    ///
    /// [HttpDelete("{id}")]
    /// public IActionResult DeleteProduct(int id)
    /// {
    ///     var result = _productService.DeleteProduct(id);
    ///     return result.ToNoContentResult();
    ///
    ///     // Success: Returns 204 No Content (product deleted)
    ///     // NotFound: Returns 404 with Problem Details
    ///     // Forbidden: Returns 403 with Problem Details (e.g., no permission)
    /// }
    /// </code>
    /// </example>
    public static IActionResult ToNoContentResult(this Result result)
    {
        return result.IsSuccess
            ? new NoContentResult()
            : CreateProblemDetailsResult(result.Errors);
    }

    private static IActionResult CreateProblemDetailsResult(IReadOnlyList<IError> errors)
    {
        var firstError = errors.First();

        return firstError.Type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(CreateProblemDetails(
                "Not Found",
                "The requested resource was not found.",
                StatusCodes.Status404NotFound,
                errors)),

            ErrorType.Validation => new BadRequestObjectResult(CreateValidationProblemDetails(
                "Validation Error",
                "One or more validation errors occurred.",
                StatusCodes.Status400BadRequest,
                errors)),

            ErrorType.Conflict => new ConflictObjectResult(CreateProblemDetails(
                "Conflict",
                "A conflict occurred while processing your request.",
                StatusCodes.Status409Conflict,
                errors)),

            ErrorType.Unauthorized => new UnauthorizedObjectResult(CreateProblemDetails(
                "Unauthorized",
                "Authentication is required to access this resource.",
                StatusCodes.Status401Unauthorized,
                errors)),

            ErrorType.Forbidden => new ObjectResult(CreateProblemDetails(
                "Forbidden",
                "You do not have permission to access this resource.",
                StatusCodes.Status403Forbidden,
                errors))
            { StatusCode = StatusCodes.Status403Forbidden },

            ErrorType.BusinessRule => new UnprocessableEntityObjectResult(CreateProblemDetails(
                "Business Rule Violation",
                "A business rule was violated.",
                StatusCodes.Status422UnprocessableEntity,
                errors)),

            _ => new ObjectResult(CreateProblemDetails(
                "Internal Server Error",
                "An error occurred while processing your request.",
                StatusCodes.Status500InternalServerError,
                errors))
            { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }

    private static ProblemDetails CreateProblemDetails(
        string title,
        string detail,
        int statusCode,
        IReadOnlyList<IError> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Extensions =
            {
                ["errors"] = errors.Select(e => new
                {
                    code = e.Code,
                    message = e.Message,
                    type = e.Type.ToString(),
                    severity = e.Severity.ToString(),
                    timestamp = e.Timestamp
                }).ToList()
            }
        };

        return problemDetails;
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        string title,
        string detail,
        int statusCode,
        IReadOnlyList<IError> errors)
    {
        var validationErrors = errors
            .Where(e => e.Type == ErrorType.Validation)
            .GroupBy(e => e.Metadata.TryGetValue("PropertyName", out var prop) ? prop.ToString() : "General")
            .ToDictionary(
                g => g.Key ?? "General",
                g => g.Select(e => e.Message).ToArray());

        var problemDetails = new ValidationProblemDetails(validationErrors)
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Extensions =
            {
                ["errors"] = errors.Select(e => new
                {
                    code = e.Code,
                    message = e.Message,
                    type = e.Type.ToString(),
                    severity = e.Severity.ToString(),
                    timestamp = e.Timestamp
                }).ToList()
            }
        };

        return problemDetails;
    }
}
