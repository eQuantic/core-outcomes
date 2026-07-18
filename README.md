# eQuantic Core Outcomes Library

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.Outcomes.svg)](https://www.nuget.org/packages/eQuantic.Core.Outcomes/)
[![CI/CD](https://github.com/eQuantic/core-outcomes/actions/workflows/ci.yml/badge.svg)](https://github.com/eQuantic/core-outcomes/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A modern, type-safe **Result Pattern** implementation for .NET with **Railway-Oriented Programming** support. Handle success and failure cases elegantly without exceptions.

## 🚀 What's New in v3.0

- ✅ **.NET 8 & .NET 10** support (.NET 6 dropped — end of life)
- ✅ **Dependency-free core** — the ASP.NET Core integration moved to its own package,
  `eQuantic.Core.Outcomes.AspNetCore`; the core no longer references `Microsoft.AspNetCore.App`
- ✅ **System.Text.Json** serialization attributes (Newtonsoft.Json dependency removed)
- ✅ **FluentValidation 12** in the integration package
- ✅ **Automated releases** — semantic-release computes versions from commit messages
  (see [docs/releasing.md](docs/releasing.md))

Everything from v2 is still here: immutable records, Railway-Oriented Programming
(Map/Bind/Match), typed errors, full async support, result combinators, observability and
RFC 7807 integration.

## 📦 Installation

| Package | Purpose |
|---------|---------|
| `eQuantic.Core.Outcomes` | The Result Pattern core — dependency-free |
| `eQuantic.Core.Outcomes.AspNetCore` | `Result` → HTTP responses with Problem Details |
| `eQuantic.Core.Outcomes.FluentValidation` | FluentValidation results as typed failures |

```bash
dotnet add package eQuantic.Core.Outcomes
dotnet add package eQuantic.Core.Outcomes.AspNetCore        # for web APIs
dotnet add package eQuantic.Core.Outcomes.FluentValidation  # for validation pipelines
```

## 🏆 Why Choose eQuantic.Core.Outcomes?

### Unique Features (Not Available in Competing Libraries)

**🔍 Built-in Observability & Distributed Tracing**
- ✅ **CorrelationId & TraceId** - Native OpenTelemetry, Jaeger, Zipkin integration
- ✅ **ExecutionTime** - Automatic performance measurement with `Timed()`/`TimedAsync()`
- ✅ **Metadata** - Rich contextual data for APM tools (Datadog, New Relic, Application Insights)
- 🔥 **UNIQUE**: Only Result library with native observability support for microservices!

**🔗 Most Complete Result Combinators**
- ✅ `Combine()` - All-or-nothing aggregation
- ✅ `Zip()` - Type-safe combination of 2-4 results
- ✅ `FirstSuccess()` - Fallback/retry patterns
- ✅ `SuccessfulValues()` - Graceful degradation
- ✅ `MergeErrors()` - Comprehensive error collection
- ✅ `Partition()` - Success/failure separation
- 🔥 **UNIQUE**: 6 powerful combinators vs. 0-1 in other libraries!

**✨ Seamless FluentValidation Integration**
- ✅ Optional package: `eQuantic.Core.Outcomes.FluentValidation`
- ✅ `.ToResult()` automatic conversion
- ✅ `.Validate()` / `.ValidateAsync()` pipeline integration
- 🔥 **UNIQUE**: Only library with native FluentValidation support!

**🌐 Advanced ASP.NET Core Integration**
- ✅ Automatic RFC 7807 Problem Details conversion
- ✅ Smart ErrorType → HTTP Status mapping
- ✅ `ToActionResult()`, `ToCreatedAtActionResult()`, `ToNoContentResult()`
- ✅ REST API best practices built-in

**⚡ Complete Async/Await Support**
- ✅ All operations: `MapAsync`, `BindAsync`, `MatchAsync`, `TapAsync`, `EnsureAsync`
- ✅ Task unwrapping for cleaner pipelines
- ✅ `CancellationToken` support throughout
- ✅ Optimized with `ConfigureAwait(false)`

**🎨 Rich Typed Error System**
```csharp
// 8 semantic error types with factory methods
Error.Validation()    // Validation failures
Error.NotFound()      // Resource not found
Error.Conflict()      // Resource conflicts
Error.Unauthorized()  // Authentication failures
Error.Forbidden()     // Permission denials
Error.BusinessRule()  // Domain rule violations
Error.Technical()     // Infrastructure failures
Error.External()      // Third-party failures
```

### Comparison with Popular Libraries

| Feature | eQuantic.Outcomes | FluentResults | ErrorOr | Ardalis.Result |
|---------|------------------|---------------|---------|----------------|
| **Observability/APM** | ✅ Built-in | ❌ None | ❌ None | ❌ None |
| **Result Combinators** | ✅ 6 patterns | ⚠️ 1 basic | ❌ None | ❌ None |
| **FluentValidation** | ✅ Native | ❌ Manual | ❌ Manual | ❌ Manual |
| **RFC 7807 Auto** | ✅ Yes | ⚠️ Basic | ❌ No | ⚠️ Partial |
| **Async Support** | ✅ Complete | ⚠️ Partial | ❌ Limited | ⚠️ Basic |
| **Railway-Oriented** | ✅ Full | ⚠️ Partial | ⚠️ Partial | ⚠️ Partial |
| **Typed Errors** | ✅ 8 types | ⚠️ Generic | ⚠️ Basic | ⚠️ Basic |
| **.NET 8/10** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **XML Docs** | ✅ Complete | ⚠️ Basic | ⚠️ Basic | ⚠️ Basic |

**Our Advantage**: We're the **ONLY** library combining observability, comprehensive combinators, FluentValidation integration, and full Railway-Oriented Programming in a modern, well-documented package.

## 🎯 Quick Start

### Basic Usage

```csharp
using eQuantic.Core.Outcomes;
using eQuantic.Core.Outcomes.Errors;

// Success case
var successResult = Result<int>.Success(42);
Console.WriteLine(successResult.Value); // 42

// Failure case
var error = new Error("USER_001", "User not found", ErrorType.NotFound);
var failureResult = Result<User>.Failure(error);

if (failureResult.IsFailure)
{
    Console.WriteLine(failureResult.FirstError.Message);
}
```

### Implicit Conversions

```csharp
// Implicitly convert from value to Result
Result<string> result = "Hello, World!";

// Implicitly convert from error to Result
Result<int> errorResult = new Error("ERR001", "Something went wrong");
```

## 🛤️ Railway-Oriented Programming

Chain operations elegantly with automatic short-circuiting on failures:

```csharp
using eQuantic.Core.Outcomes.Extensions;

var result = GetUser(userId)
    .Map(user => user.Email)
    .Ensure(email => email.Contains("@"),
            Error.Validation("VAL001", "Invalid email format"))
    .Bind(email => SendWelcomeEmail(email))
    .Tap(email => _logger.LogInformation($"Email sent to {email}"));

return result.ToActionResult();
```

### Map (Functor)

Transform the value inside a successful result:

```csharp
Result<int> numberResult = Result<int>.Success(5);

Result<string> stringResult = numberResult.Map(n => n.ToString());
// Result: Success("5")
```

### Bind (Monad / FlatMap)

Chain operations that return Results:

```csharp
Result<User> GetUser(int id) { /* ... */ }
Result<Profile> GetProfile(User user) { /* ... */ }

var profileResult = GetUser(userId)
    .Bind(user => GetProfile(user));
```

### Match (Pattern Matching)

Handle both success and failure cases:

```csharp
var message = result.Match(
    onSuccess: user => $"Welcome, {user.Name}!",
    onFailure: errors => $"Error: {errors.First().Message}"
);
```

### Ensure (Inline Validation)

Add validation inline in your chain:

```csharp
var result = Result<int>.Success(5)
    .Ensure(n => n > 0, Error.Validation("VAL001", "Must be positive"))
    .Ensure(n => n < 100, Error.Validation("VAL002", "Must be less than 100"));
```

### Tap (Side Effects)

Execute side effects without breaking the chain:

```csharp
var result = GetUser(userId)
    .Tap(user => _logger.LogInformation($"User {user.Id} retrieved"))
    .TapError(errors => _logger.LogError($"Failed: {errors.First().Message}"))
    .Map(user => new UserDto(user));
```

## ⚡ Async Support

All operations have async variants:

```csharp
using eQuantic.Core.Outcomes.Extensions;

var result = await GetUserAsync(userId)
    .MapAsync(user => TransformUserAsync(user))
    .BindAsync(user => ValidateUserAsync(user))
    .EnsureAsync(user => CheckPermissionsAsync(user),
                 Error.Forbidden("AUTH001", "Access denied"))
    .TapAsync(user => LogActivityAsync(user));
```

### ToResultAsync

Wrap async operations with automatic exception handling:

```csharp
var result = await _httpClient
    .GetStringAsync("https://api.example.com/data")
    .ToResultAsync();

if (result.IsSuccess)
{
    var data = result.Value;
}
```

## 🎨 Error Types

Create strongly-typed errors with semantic meaning:

```csharp
// Validation errors
var error = Error.Validation("VAL001", "Email is required", "Email");

// Not found errors
var error = Error.NotFound("USER_001", "User not found", userId);

// Business rule errors
var error = Error.BusinessRule("BIZ001", "Cannot delete active subscription");

// Conflict errors
var error = Error.Conflict("CONF001", "Email already exists");

// Authorization errors
var error = Error.Unauthorized("AUTH001", "Invalid credentials");
var error = Error.Forbidden("AUTH002", "Insufficient permissions");

// Technical errors
var error = Error.Technical("TECH001", "Database connection failed", exception);

// External service errors
var error = Error.External("EXT001", "Payment gateway timeout", exception);

// From exceptions
var error = Error.FromException(exception, "ERR001", ErrorType.Technical);
```

### Validation Errors

```csharp
var validationError = new ValidationError(
    code: "VAL001",
    message: "Email format is invalid",
    propertyName: "Email",
    attemptedValue: "invalid-email"
);
```

## 🔗 FluentValidation Integration

**Optional Package**: `eQuantic.Core.Outcomes.FluentValidation`

Seamlessly integrate FluentValidation with the Result Pattern:

### Installation

```bash
dotnet add package eQuantic.Core.Outcomes.FluentValidation
```

### Usage

```csharp
using eQuantic.Core.Outcomes.FluentValidation;
using FluentValidation;

// Define your validator
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18);
    }
}

// Use directly with validator
var validator = new CreateUserRequestValidator();
var result = validator.Validate(request);

if (result.IsSuccess)
{
    // Process valid request
}

// Or chain with Railway-Oriented Programming
var createResult = Result<CreateUserRequest>.Success(request)
    .Validate(validator)
    .Bind(req => CreateUserAsync(req))
    .Map(user => new UserDto(user));
```

### Convert ValidationResult to Result

```csharp
var validationResult = validator.Validate(request);

// Convert to Result<T>
var result = validationResult.ToResult(request);

// Or non-generic Result
var result = validationResult.ToResult();
```

### Async Validation

```csharp
// Direct async validation
var result = await validator.ValidateAsync(request);

// Chain async validation
var finalResult = await Result<CreateUserRequest>.Success(request)
    .ValidateAsync(validator)
    .BindAsync(req => CreateUserAsync(req));

// Validate Task<Result<T>>
var result = await GetUserAsync(id)
    .ValidateAsync(validator);
```

### Integration with Existing Results

```csharp
var result = GetUser(userId)
    .Validate(userValidator)  // Validate if successful
    .Bind(user => GetProfile(user.Id))
    .Validate(profileValidator)  // Chain validations
    .Map(profile => new ProfileDto(profile));
```

### Error Mapping

FluentValidation errors are automatically converted to `ValidationError`:

```csharp
var result = validator.Validate(invalidRequest);

result.IsFailure.Should().BeTrue();
result.Errors.Should().AllBeOfType<ValidationError>();

var emailError = result.Errors
    .OfType<ValidationError>()
    .FirstOrDefault(e => e.PropertyName == "Email");

// ValidationError properties:
// - Code (from ErrorCode)
// - Message (from ErrorMessage)
// - PropertyName
// - AttemptedValue
```

## 🌐 ASP.NET Core Integration

Automatic conversion to HTTP responses with Problem Details (RFC 7807). Since v3 this lives in
its own package, keeping the core dependency-free:

```bash
dotnet add package eQuantic.Core.Outcomes.AspNetCore
```

```csharp
using eQuantic.Core.Outcomes.AspNetCore;

[HttpGet("{id}")]
public IActionResult GetUser(int id)
{
    var result = _userService.GetById(id);
    return result.ToActionResult();
}

// Automatic mapping:
// Success → 200 OK
// NotFound → 404 Not Found with ProblemDetails
// Validation → 400 Bad Request with ValidationProblemDetails
// Unauthorized → 401 Unauthorized
// Forbidden → 403 Forbidden
// Conflict → 409 Conflict
// BusinessRule → 422 Unprocessable Entity
// Others → 500 Internal Server Error
```

### Custom Status Codes

```csharp
// Custom success status code
return result.ToActionResult(201); // 201 Created

// Created at action
return result.ToCreatedAtActionResult("GetUser", new { id = user.Id });

// No content
return result.ToNoContentResult(); // 204 No Content
```

## 🔗 Result Combinators

Combine and aggregate multiple Results efficiently:

### Combine - All or Nothing

Combine multiple results into one. If all succeed, get all values. If any fails, get all errors:

```csharp
using eQuantic.Core.Outcomes.Extensions;

// Validate multiple fields in parallel
var emailResult = ValidateEmail(request.Email);
var passwordResult = ValidatePassword(request.Password);
var ageResult = ValidateAge(request.Age);

var validationResult = ResultCombinators.Combine(
    emailResult,
    passwordResult,
    ageResult
);

if (validationResult.IsSuccess)
{
    var (email, password, age) = validationResult.Value;
    // All validations passed
}
else
{
    // Contains all validation errors
    return BadRequest(validationResult.Errors);
}
```

### Zip - Combine Different Types

Combine results of different types into tuples:

```csharp
var userResult = GetUser(userId);
var profileResult = GetProfile(userId);

// Combine into tuple
var combined = userResult.Zip(profileResult);

if (combined.IsSuccess)
{
    var (user, profile) = combined.Value;
    return new UserWithProfile(user, profile);
}

// Supports up to 4 results
var result = result1.Zip(result2, result3, result4);
```

### FirstSuccess - Fallback Pattern

Try multiple sources, return first success:

```csharp
// Try cache first, then database, then API
var result = ResultCombinators.FirstSuccess(
    GetFromCache(key),
    GetFromDatabase(key),
    GetFromApi(key)
);

// Returns first successful result
// If all fail, returns failure with all errors
```

### SuccessfulValues - Partial Success

Get only successful values, ignore failures:

```csharp
var processResults = items.Select(item => ProcessItem(item));

var successful = ResultCombinators.SuccessfulValues(processResults);

// Returns Result<IEnumerable<T>> with only successful values
// Useful when some failures are acceptable
```

### Partition - Separate Successes and Failures

Split results into successful values and errors:

```csharp
var results = items.Select(item => ValidateItem(item));

var (successes, errors) = results.Partition();

Console.WriteLine($"Processed: {successes.Count()}, Failed: {errors.Count()}");
```

### MergeErrors - Collect All Errors

Useful for validation scenarios:

```csharp
var validationResults = new[]
{
    ValidateField1(),
    ValidateField2(),
    ValidateField3()
};

var merged = ResultCombinators.MergeErrors(validationResults);
// Contains all validation errors from all failed results
```

## 🔍 Observability & Tracing

Track and monitor your operations with built-in observability support:

### Adding Correlation and Trace IDs

```csharp
var result = await GetUserAsync(userId)
    .WithCorrelationId(httpContext.TraceIdentifier)
    .WithTraceId(Activity.Current?.Id)
    .WithMetadata("userId", userId)
    .WithMetadata("source", "api");

// Access observability data
Console.WriteLine($"CorrelationId: {result.CorrelationId}");
Console.WriteLine($"TraceId: {result.TraceId}");
Console.WriteLine($"Metadata: {result.Metadata["source"]}");
```

### Measuring Execution Time

Automatically measure operation execution time:

```csharp
// Synchronous
var result = ObservabilityExtensions.Timed(() =>
{
    return PerformExpensiveOperation();
});

Console.WriteLine($"Operation took: {result.ExecutionTime}");

// Asynchronous
var result = await ObservabilityExtensions.TimedAsync(async () =>
{
    return await PerformAsyncOperation();
});
```

### Distributed Tracing Integration

Perfect for microservices and distributed systems:

```csharp
public async Task<Result<Order>> ProcessOrderAsync(OrderRequest request, string correlationId)
{
    return await ObservabilityExtensions.TimedAsync(async () =>
    {
        var result = await _orderService.CreateOrderAsync(request);
        return result
            .WithCorrelationId(correlationId)
            .WithTraceId(Activity.Current?.Id)
            .WithMetadata("orderId", result.Value?.Id)
            .WithMetadata("service", "order-processing")
            .WithMetadata("environment", _env.EnvironmentName);
    });
}
```

### Custom Metadata

Add any additional context to your results:

```csharp
var result = Result<User>.Success(user)
    .WithMetadata("ipAddress", "192.168.1.1")
    .WithMetadata("userAgent", "Mozilla/5.0...")
    .WithMetadata("apiVersion", "2.0");

// Add multiple metadata entries at once
var metadata = new Dictionary<string, object>
{
    ["requestId"] = Guid.NewGuid(),
    ["timestamp"] = DateTimeOffset.UtcNow,
    ["region"] = "us-east-1"
};

result = result.WithMetadata(metadata);
```

## 📊 Legacy Support (v1.x API)

The v1.x Builder pattern is still available for backward compatibility:

```csharp
var result = Outcome.FromItemResult<User>()
    .WithSuccess()
    .WithItem(user)
    .Result();
```

However, we recommend migrating to the new API for better type safety and functional programming support.

## 🔄 Migration Guide (v1.x → v2.0)

### Before (v1.x)
```csharp
var resultBuilder = Outcome.FromItemResult<User>();
try
{
    var user = await _repository.GetByIdAsync(id);
    if (user == null)
    {
        resultBuilder = resultBuilder
            .WithError()
            .WithStatus(ResultStatus.NotFound)
            .WithMessage("User not found");
        return NotFound(resultBuilder.Result());
    }

    resultBuilder = resultBuilder
        .WithSuccess()
        .WithItem(user);
    return Ok(resultBuilder.Result());
}
catch (Exception ex)
{
    resultBuilder = resultBuilder.WithException(ex);
    return StatusCode(500, resultBuilder.Result());
}
```

### After (v2.0)
```csharp
var result = await _repository
    .GetByIdAsync(id)
    .ToResultAsync()
    .Ensure(user => user != null,
            Error.NotFound("USER_001", "User not found", id.ToString()));

return result.ToActionResult();
```

## 📚 Advanced Examples

### Combining Multiple Results

```csharp
var userResult = GetUser(userId);
var profileResult = GetProfile(userId);

var combinedResult = userResult.Bind(user =>
    profileResult.Map(profile => new UserWithProfile(user, profile))
);
```

### Conditional Chains

```csharp
var result = GetUser(userId)
    .Ensure(user => user.IsActive,
            Error.BusinessRule("BIZ001", "User is not active"))
    .Ensure(user => user.EmailVerified,
            Error.BusinessRule("BIZ002", "Email not verified"))
    .Bind(user => ProcessUser(user));
```

### Multiple Error Accumulation

```csharp
var errors = new List<IError>();

if (string.IsNullOrEmpty(request.Email))
    errors.Add(Error.Validation("VAL001", "Email is required", "Email"));

if (request.Age < 18)
    errors.Add(Error.Validation("VAL002", "Must be 18 or older", "Age"));

if (errors.Any())
    return Result<User>.Failure(errors);

return Result<User>.Success(new User(request));
```

## 🧪 Testing

The library includes comprehensive test coverage. Example:

```csharp
[Fact]
public void Map_OnSuccessResult_ShouldMapValue()
{
    // Arrange
    var result = Result<int>.Success(5);

    // Act
    var mappedResult = result.Map(x => x * 2);

    // Assert
    mappedResult.IsSuccess.Should().BeTrue();
    mappedResult.Value.Should().Be(10);
}
```

## 🏗️ Architecture

- **Immutable by design** - Results use C# records for immutability
- **Type-safe** - Compile-time guarantees with generic types
- **Functional** - Railway-Oriented Programming patterns
- **Async-first** - Full Task<Result<T>> support
- **Zero allocations** - Optimized for performance
- **Nullable reference types** - Full C# 8+ support

## 📖 Documentation

For more examples and detailed documentation, see:
- [Analysis and Improvements](ANALYSIS_AND_IMPROVEMENTS.md)
- [Samples](samples/)

## 🔧 Development

### Building & Testing

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

### Versioning

This project uses **GitVersion** for automatic semantic versioning:

- **master**: Stable releases (e.g., `2.0.0`)
- **develop**: Preview releases (e.g., `2.1.0-alpha.5`)
- **feature/***: Feature branches (not published)
- **release/***: Beta releases (e.g., `2.0.0-beta.1`)

Use [Conventional Commits](https://www.conventionalcommits.org/) to control version increments:

```bash
# Patch: 2.0.0 → 2.0.1
git commit -m "fix: resolve memory leak"

# Minor: 2.0.0 → 2.1.0
git commit -m "feat: add new combinator"

# Major: 2.0.0 → 3.0.0
git commit -m "feat!: breaking API change"
```

### Publishing

Packages are automatically published to NuGet.org via GitHub Actions:

- **Push to master**: Publishes stable version
- **Push to develop**: Publishes preview version
- **Create tag** (e.g., `v2.0.1`): Publishes specific version

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Inspired by functional programming patterns from:
- Railway-Oriented Programming (Scott Wlaschin)
- Result types in Rust, F#, and Haskell
- FluentResults, ErrorOr, and other .NET libraries

---

**eQuantic Systems** © 2019-2025
