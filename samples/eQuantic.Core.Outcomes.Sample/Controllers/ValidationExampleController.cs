using eQuantic.Core.Outcomes;
using eQuantic.Core.Outcomes.AspNetCore;
using eQuantic.Core.Outcomes.Extensions;
using eQuantic.Core.Outcomes.FluentValidation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace eQuantic.Core.Outcomes.Sample.Controllers;

/// <summary>
/// Example controller demonstrating FluentValidation integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationExampleController : ControllerBase
{
    private readonly ILogger<ValidationExampleController> _logger;

    public ValidationExampleController(ILogger<ValidationExampleController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Example 1: Direct validation with Result
    /// </summary>
    [HttpPost("register")]
    public IActionResult RegisterUser([FromBody] RegisterUserRequest request)
    {
        var validator = new RegisterUserRequestValidator();

        // Direct validation returns Result<T>
        var validationResult = validator.Validate(request);
        var result = validationResult.ToResult(request)
            .Tap(req => _logger.LogInformation($"Valid registration request for {req.Email}"))
            .Map(req => new { Message = $"User {req.Email} registered successfully" });

        return result.ToActionResult();
    }

    /// <summary>
    /// Example 2: Chain validation in Railway-Oriented Programming
    /// </summary>
    [HttpPost("create-profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
    {
        var validator = new CreateProfileRequestValidator();

        var result = await Result<CreateProfileRequest>.Success(request)
            .ValidateAsync(validator)  // Validate using FluentValidation
            .BindAsync(async req => await CreateProfileInternalAsync(req))
            .TapAsync(profile => _logger.LogInformation($"Profile created: {profile.Id}"))
            .MapAsync(async profile => await EnrichProfileAsync(profile));

        return result.ToCreatedAtActionResult("GetProfile", new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    /// <summary>
    /// Example 3: Convert ValidationResult to Result
    /// </summary>
    [HttpPut("update-settings/{userId}")]
    public IActionResult UpdateSettings(int userId, [FromBody] UpdateSettingsRequest request)
    {
        var validator = new UpdateSettingsRequestValidator();
        var validationResult = validator.Validate(request);

        // Convert FluentValidation ValidationResult to Result<T>
        var result = validationResult.ToResult(request)
            .Bind(req => ApplySettings(userId, req))
            .Tap(_ => _logger.LogInformation($"Settings updated for user {userId}"));

        return result.ToNoContentResult();
    }

    /// <summary>
    /// Example 4: Multiple validators in chain
    /// </summary>
    [HttpPost("complex-operation")]
    public async Task<IActionResult> ComplexOperation([FromBody] ComplexRequest request)
    {
        var requestValidator = new ComplexRequestValidator();
        var dataValidator = new ProcessedDataValidator();

        var result = await Result<ComplexRequest>.Success(request)
            .ValidateAsync(requestValidator)  // First validation
            .BindAsync(async req => await ProcessRequestAsync(req))
            .ValidateAsync(dataValidator)  // Second validation on processed data
            .MapAsync(async data => await FinalizeAsync(data));

        if (result.IsSuccess)
        {
            return Ok(new { result.Value.Id, result.Value.Status, Message = "Operation completed successfully" });
        }
        return BadRequest(new { errors = result.Errors.Select(e => new { e.Code, e.Message }) });
    }

    /// <summary>
    /// Example 5: Conditional validation
    /// </summary>
    [HttpPost("conditional")]
    public IActionResult ConditionalValidation([FromBody] ConditionalRequest request)
    {
        // Choose validator based on request type
        IValidator<ConditionalRequest> validator = request.Type switch
        {
            "premium" => new PremiumRequestValidator(),
            "basic" => new BasicRequestValidator(),
            _ => new StandardRequestValidator()
        };

        var validationResult = validator.Validate(request);
        var result = validationResult.ToResult(request)
            .Bind(req => ProcessConditionalRequest(req));

        return result.ToActionResult();
    }

    // Helper methods
    private async Task<Result<UserProfile>> CreateProfileInternalAsync(CreateProfileRequest request)
    {
        await Task.Delay(100); // Simulate async operation

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Bio = request.Bio,
            Website = request.Website
        };

        return Result<UserProfile>.Success(profile);
    }

    private async Task<UserProfile> EnrichProfileAsync(UserProfile profile)
    {
        await Task.Delay(50);
        profile.CreatedAt = DateTime.UtcNow;
        return profile;
    }

    private Result<Unit> ApplySettings(int userId, UpdateSettingsRequest request)
    {
        _logger.LogInformation($"Applying settings for user {userId}");
        // Apply settings logic here
        return Result<Unit>.Success(Unit.Value);
    }

    private async Task<Result<ProcessedData>> ProcessRequestAsync(ComplexRequest request)
    {
        await Task.Delay(100);

        var data = new ProcessedData
        {
            Id = Guid.NewGuid(),
            OriginalValue = request.Value,
            ProcessedValue = request.Value * 2,
            Status = "Processed"
        };

        return Result<ProcessedData>.Success(data);
    }

    private async Task<ProcessedData> FinalizeAsync(ProcessedData data)
    {
        await Task.Delay(50);
        data.Status = "Finalized";
        return data;
    }

    private Result<string> ProcessConditionalRequest(ConditionalRequest request)
    {
        var result = request.Type switch
        {
            "premium" => "Premium processing completed",
            "basic" => "Basic processing completed",
            _ => "Standard processing completed"
        };

        return Result<string>.Success(result);
    }
}

// DTOs and Models
public class RegisterUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class CreateProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? Website { get; set; }
}

public class UpdateSettingsRequest
{
    public bool EmailNotifications { get; set; }
    public string? Theme { get; set; }
}

public class ComplexRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class ProcessedData
{
    public Guid Id { get; set; }
    public decimal OriginalValue { get; set; }
    public decimal ProcessedValue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ConditionalRequest
{
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

public class UserProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; }
}

public struct Unit
{
    public static Unit Value => default;
}

// Validators
public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithErrorCode("EMAIL_INVALID");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("PASSWORD_REQUIRED")
            .MinimumLength(8).WithErrorCode("PASSWORD_TOO_SHORT")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letter").WithErrorCode("PASSWORD_NO_UPPERCASE")
            .Matches(@"[0-9]").WithMessage("Password must contain number").WithErrorCode("PASSWORD_NO_NUMBER");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords must match").WithErrorCode("PASSWORD_MISMATCH");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithErrorCode("AGE_UNDERAGE");
    }
}

public class CreateProfileRequestValidator : AbstractValidator<CreateProfileRequest>
{
    public CreateProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Bio)
            .MaximumLength(500);

        RuleFor(x => x.Website)
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid URL format");
    }
}

public class UpdateSettingsRequestValidator : AbstractValidator<UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.Theme)
            .Must(theme => string.IsNullOrEmpty(theme) || new[] { "light", "dark", "auto" }.Contains(theme))
            .WithMessage("Theme must be: light, dark, or auto");
    }
}

public class ComplexRequestValidator : AbstractValidator<ComplexRequest>
{
    public ComplexRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000000);
    }
}

public class ProcessedDataValidator : AbstractValidator<ProcessedData>
{
    public ProcessedDataValidator()
    {
        RuleFor(x => x.ProcessedValue)
            .GreaterThan(x => x.OriginalValue)
            .WithMessage("Processed value must be greater than original");

        RuleFor(x => x.Status)
            .NotEmpty();
    }
}

public class PremiumRequestValidator : AbstractValidator<ConditionalRequest>
{
    public PremiumRequestValidator()
    {
        RuleFor(x => x.Data)
            .NotEmpty()
            .MinimumLength(10)
            .WithMessage("Premium requests require at least 10 characters");
    }
}

public class BasicRequestValidator : AbstractValidator<ConditionalRequest>
{
    public BasicRequestValidator()
    {
        RuleFor(x => x.Data)
            .NotEmpty();
    }
}

public class StandardRequestValidator : AbstractValidator<ConditionalRequest>
{
    public StandardRequestValidator()
    {
        RuleFor(x => x.Data)
            .NotEmpty()
            .MinimumLength(5);
    }
}
