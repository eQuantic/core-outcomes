using eQuantic.Core.Outcomes;
using eQuantic.Core.Outcomes.Errors;
using eQuantic.Core.Outcomes.Extensions;
using eQuantic.Core.Outcomes.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace eQuantic.Core.Outcomes.Sample.Controllers;

/// <summary>
/// Example controller demonstrating v2.0 Result Pattern usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Example 1: Simple success/failure with ToActionResult
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var result = GetUserById(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Example 2: Railway-Oriented Programming chain
    /// </summary>
    [HttpGet("{id}/email")]
    public IActionResult GetUserEmail(int id)
    {
        var result = GetUserById(id)
            .Map(user => user.Email)
            .Ensure(email => !string.IsNullOrEmpty(email),
                    Error.Validation("VAL001", "Email is required"))
            .Tap(email => _logger.LogInformation($"Retrieved email for user {id}"));

        return result.ToActionResult();
    }

    /// <summary>
    /// Example 3: Async operations with error handling
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await ValidateRequest(request)
            .BindAsync(async req => await CreateUserAsync(req))
            .TapAsync(user => _logger.LogInformation($"User created: {user.Id}"));

        if (result.IsFailure)
        {
            _logger.LogError($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        }

        return result.ToCreatedAtActionResult("GetUser", new { id = result.IsSuccess ? result.Value.Id : 0 });
    }

    /// <summary>
    /// Example 4: Multiple validations with Ensure
    /// </summary>
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var result = GetUserById(id)
            .Ensure(user => user.IsActive,
                    Error.BusinessRule("BIZ001", "Cannot update inactive user"))
            .Ensure(user => !user.IsDeleted,
                    Error.BusinessRule("BIZ002", "Cannot update deleted user"))
            .Bind(user => UpdateUserData(user, request))
            .Tap(user => _logger.LogInformation($"User {id} updated successfully"));

        return result.ToNoContentResult();
    }

    /// <summary>
    /// Example 5: Pattern matching with custom response
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var result = GetUserById(id)
            .Bind(user => DeleteUserSafely(user));

        return result.Match(
            onSuccess: _ => Ok(new { message = "User deleted successfully" }),
            onFailure: errors => errors.First().Type switch
            {
                ErrorType.NotFound => NotFound(new { message = errors.First().Message }),
                ErrorType.BusinessRule => UnprocessableEntity(new { message = errors.First().Message }),
                _ => StatusCode(500, new { message = "An error occurred" })
            }
        );
    }

    /// <summary>
    /// Example 6: Implicit conversions
    /// </summary>
    [HttpGet("{id}/status")]
    public IActionResult GetUserStatus(int id)
    {
        // Implicit conversion from value to Result
        Result<string> result = id > 0
            ? "Active"
            : Error.Validation("VAL001", "Invalid user ID");

        return result.ToActionResult();
    }

    // Helper methods

    private Result<User> GetUserById(int id)
    {
        if (id <= 0)
            return Error.Validation("VAL001", "User ID must be positive", "id");

        // Simulate database lookup
        var user = id == 1
            ? new User { Id = 1, Email = "john@example.com", IsActive = true }
            : null;

        if (user == null)
            return Error.NotFound("USER_001", "User not found", id.ToString());

        return Result<User>.Success(user);
    }

    private Result<CreateUserRequest> ValidateRequest(CreateUserRequest request)
    {
        var errors = new List<IError>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(new ValidationError("VAL001", "Email is required", "Email"));

        if (!request.Email.Contains("@"))
            errors.Add(new ValidationError("VAL002", "Invalid email format", "Email", request.Email));

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError("VAL003", "Name is required", "Name"));

        if (errors.Any())
            return Result<CreateUserRequest>.Failure(errors);

        return Result<CreateUserRequest>.Success(request);
    }

    private async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        // Simulate async database operation
        await Task.Delay(100);

        // Check for duplicates
        if (request.Email == "existing@example.com")
            return Error.Conflict("CONF001", "Email already exists");

        var user = new User
        {
            Id = new Random().Next(1000, 9999),
            Email = request.Email,
            Name = request.Name,
            IsActive = true
        };

        return Result<User>.Success(user);
    }

    private Result<User> UpdateUserData(User user, UpdateUserRequest request)
    {
        user.Name = request.Name ?? user.Name;
        user.Email = request.Email ?? user.Email;
        return Result<User>.Success(user);
    }

    private Result<User> DeleteUserSafely(User user)
    {
        if (user.IsActive)
            return Error.BusinessRule("BIZ001", "Cannot delete active user. Deactivate first.");

        user.IsDeleted = true;
        return Result<User>.Success(user);
    }
}

// DTOs
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Name { get; set; }
}
