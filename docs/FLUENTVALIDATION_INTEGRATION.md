# FluentValidation Integration Guide

## 📦 Package Information

**Package Name**: `eQuantic.Core.Outcomes.FluentValidation`
**Version**: 2.0.0
**Type**: Optional Extension Package

This package provides seamless integration between FluentValidation and the eQuantic.Core.Outcomes Result Pattern.

---

## 🎯 Why FluentValidation Integration?

FluentValidation is one of the most popular validation libraries for .NET. This integration allows you to:

- ✅ Convert FluentValidation results to `Result<T>` types
- ✅ Chain validations in Railway-Oriented Programming style
- ✅ Automatically map validation errors to structured `ValidationError` objects
- ✅ Use async validation seamlessly
- ✅ Maintain separation of concerns with optional dependency

---

## 📥 Installation

### Core Package (Required)
```bash
dotnet add package eQuantic.Core.Outcomes
```

### FluentValidation Integration (Optional)
```bash
dotnet add package eQuantic.Core.Outcomes.FluentValidation
```

---

## 🚀 Quick Start

### 1. Define Your Validator

```csharp
using FluentValidation;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithErrorCode("EMAIL_INVALID");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithErrorCode("AGE_UNDERAGE");
    }
}
```

### 2. Use with Result Pattern

```csharp
using eQuantic.Core.Outcomes.FluentValidation;

var validator = new CreateUserRequestValidator();

// Direct validation - returns Result<CreateUserRequest>
var result = validator.Validate(request);

if (result.IsSuccess)
{
    var user = await CreateUser(result.Value);
}
```

---

## 📚 Usage Patterns

### Pattern 1: Direct Validation

Convert validator directly to Result:

```csharp
var validator = new UserValidator();
var result = validator.Validate(user);

// result is Result<User>
if (result.IsSuccess)
{
    // Process valid user
}
else
{
    // Handle validation errors
    foreach (var error in result.Errors.OfType<ValidationError>())
    {
        Console.WriteLine($"{error.PropertyName}: {error.Message}");
    }
}
```

### Pattern 2: ValidationResult Conversion

Convert FluentValidation's `ValidationResult` to `Result`:

```csharp
var validator = new UserValidator();
var validationResult = validator.Validate(user);

// Convert to Result<User>
var result = validationResult.ToResult(user);

// Or non-generic Result
var simpleResult = validationResult.ToResult();
```

### Pattern 3: Railway-Oriented Programming

Chain validation with other operations:

```csharp
var result = Result<CreateUserRequest>.Success(request)
    .Validate(validator)                    // Validate
    .Bind(req => CreateUserAsync(req))      // Create user
    .Map(user => new UserDto(user))         // Map to DTO
    .Tap(dto => _logger.LogInfo($"Created user {dto.Id}"));

return result.ToActionResult();
```

### Pattern 4: Async Validation

```csharp
// Direct async validation
var result = await validator.ValidateAsync(request);

// Chain async validation
var finalResult = await Result<CreateUserRequest>.Success(request)
    .ValidateAsync(validator)
    .BindAsync(req => CreateUserAsync(req))
    .MapAsync(user => EnrichUserAsync(user));
```

### Pattern 5: Validate Existing Results

Validate a value that's already in a Result:

```csharp
var result = GetUser(userId)              // Returns Result<User>
    .Validate(userValidator)               // Validate if successful
    .Bind(user => GetProfile(user.Id))     // Continue chain
    .Validate(profileValidator);           // Validate again
```

### Pattern 6: Task<Result<T>> Validation

```csharp
// Validate async result
var result = await GetUserAsync(id)
    .ValidateAsync(validator);

// Or in a chain
var finalResult = await FetchDataAsync()
    .ValidateAsync(dataValidator)
    .BindAsync(data => ProcessAsync(data));
```

---

## 🔧 Extension Methods Reference

### IValidator<T> Extensions

| Method | Description | Returns |
|--------|-------------|---------|
| `Validate<T>(instance)` | Validates and returns Result | `Result<T>` |
| `ValidateAsync<T>(instance, ct)` | Async validation | `Task<Result<T>>` |

### ValidationResult Extensions

| Method | Description | Returns |
|--------|-------------|---------|
| `ToResult<T>(value)` | Convert to Result with value | `Result<T>` |
| `ToResult()` | Convert to non-generic Result | `Result` |

### Result<T> Extensions

| Method | Description | Returns |
|--------|-------------|---------|
| `Validate<T>(validator)` | Validate result value | `Result<T>` |
| `ValidateAsync<T>(validator, ct)` | Async validate result | `Task<Result<T>>` |

### Task<Result<T>> Extensions

| Method | Description | Returns |
|--------|-------------|---------|
| `ValidateAsync<T>(validator, ct)` | Validate async result | `Task<Result<T>>` |

---

## 🎨 Error Mapping

FluentValidation errors are automatically converted to `ValidationError`:

```csharp
var result = validator.Validate(invalidRequest);

// All errors are ValidationError type
foreach (var error in result.Errors.OfType<ValidationError>())
{
    Console.WriteLine($"Code: {error.Code}");              // From ErrorCode
    Console.WriteLine($"Message: {error.Message}");        // From ErrorMessage
    Console.WriteLine($"Property: {error.PropertyName}");  // Property that failed
    Console.WriteLine($"Attempted: {error.AttemptedValue}"); // Value attempted
    Console.WriteLine($"Type: {error.Type}");              // ErrorType.Validation
    Console.WriteLine($"Timestamp: {error.Timestamp}");    // When error occurred
}
```

### Custom Error Codes

Use FluentValidation's `WithErrorCode()` for structured error codes:

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("USER_EMAIL_REQUIRED")
            .EmailAddress().WithErrorCode("USER_EMAIL_INVALID");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithErrorCode("USER_AGE_UNDERAGE");
    }
}
```

---

## 🌐 ASP.NET Core Integration

Combine FluentValidation with ASP.NET Core integration:

```csharp
[HttpPost]
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
    var validator = new CreateUserRequestValidator();

    var result = validator.Validate(request)
        .Bind(req => _userService.CreateUser(req));

    return result.ToActionResult();
}

// Automatic responses:
// ✅ Valid request → 200 OK with user data
// ❌ Invalid request → 400 Bad Request with ValidationProblemDetails
```

### Validation Problem Details

Invalid requests automatically return RFC 7807 ValidationProblemDetails:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "Email": ["Email is required", "Invalid email format"],
    "Age": ["Must be 18 or older"]
  },
  "extensions": {
    "errors": [
      {
        "code": "EMAIL_REQUIRED",
        "message": "Email is required",
        "type": "Validation",
        "severity": "Error",
        "timestamp": "2025-10-07T23:00:00Z"
      }
    ]
  }
}
```

---

## 🧪 Testing

The package includes comprehensive tests demonstrating all features:

```csharp
[Fact]
public void Validate_WithInvalidData_ShouldReturnValidationErrors()
{
    // Arrange
    var user = new User { Email = "", Age = 15 };
    var validator = new UserValidator();

    // Act
    var result = validator.Validate(user);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().HaveCount(2);
    result.Errors.Should().AllBeOfType<ValidationError>();

    var emailError = result.Errors
        .OfType<ValidationError>()
        .FirstOrDefault(e => e.PropertyName == "Email");

    emailError.Should().NotBeNull();
    emailError!.Code.Should().Be("EMAIL_REQUIRED");
}
```

---

## 💡 Best Practices

### 1. Define Validators Separately

```csharp
// ✅ Good - Reusable validator
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
    }
}

// Use in controller
var result = validator.Validate(request);
```

### 2. Use Error Codes

```csharp
// ✅ Good - Structured error codes
RuleFor(x => x.Email)
    .NotEmpty().WithErrorCode("USER_EMAIL_REQUIRED")
    .EmailAddress().WithErrorCode("USER_EMAIL_INVALID");

// ❌ Avoid - Generic error codes
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress();
```

### 3. Chain Validations

```csharp
// ✅ Good - Chain multiple validations
var result = Result<CreateUserRequest>.Success(request)
    .Validate(requestValidator)
    .Bind(req => CreateUser(req))
    .Validate(userValidator)
    .Map(user => new UserDto(user));
```

### 4. Use Dependency Injection

```csharp
// ✅ Good - Inject validators
public class UserController : ControllerBase
{
    private readonly IValidator<CreateUserRequest> _validator;

    public UserController(IValidator<CreateUserRequest> validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        return _validator.Validate(request)
            .Bind(req => _service.Create(req))
            .ToActionResult();
    }
}

// Startup.cs
services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
```

### 5. Handle Async Properly

```csharp
// ✅ Good - Use async methods
var result = await validator.ValidateAsync(request, cancellationToken);

// ✅ Good - Chain async operations
var result = await Result<CreateUserRequest>.Success(request)
    .ValidateAsync(validator)
    .BindAsync(req => CreateUserAsync(req));
```

---

## 📊 Architecture

```
┌─────────────────────────────────────┐
│  eQuantic.Core.Outcomes             │
│  (Core Package)                     │
│  ┌─────────────┐ ┌────────────────┐│
│  │  Result<T>  │ │  Error Types   ││
│  │  Result     │ │  - Error       ││
│  │  Extensions │ │  - Validation  ││
│  └─────────────┘ └────────────────┘│
└─────────────────────────────────────┘
                  ▲
                  │ References
                  │
┌─────────────────────────────────────┐
│  eQuantic.Core.Outcomes             │
│  .FluentValidation                  │
│  (Optional Package)                 │
│  ┌─────────────────────────────────┐│
│  │  FluentValidationExtensions     ││
│  │  - ToResult()                   ││
│  │  - Validate()                   ││
│  │  - ValidateAsync()              ││
│  └─────────────────────────────────┘│
│                                     │
│  Depends on:                        │
│  - FluentValidation (11.9.0)        │
└─────────────────────────────────────┘
```

---

## 🔗 Related Documentation

- [Main README](README.md) - Core library documentation
- [CHANGELOG](CHANGELOG.md) - Version history
- [ANALYSIS_AND_IMPROVEMENTS](ANALYSIS_AND_IMPROVEMENTS.md) - Design decisions

---

## 📝 Examples

Complete examples are available in:
- [ValidationExampleController.cs](samples/eQuantic.Core.Outcomes.Sample/Controllers/ValidationExampleController.cs)
- [FluentValidationExtensionsTests.cs](tests/eQuantic.Core.Outcomes.FluentValidation.Tests/FluentValidationExtensionsTests.cs)

---

**eQuantic Systems** © 2019-2025
