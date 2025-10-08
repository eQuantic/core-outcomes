# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-10-07

### 🚀 Major Update - Breaking Changes

This is a major rewrite introducing modern functional programming patterns and improved type safety.

### Added

#### Core Features
- ✨ New immutable `Result<T>` and `Result` record types
- ✨ Comprehensive error system with `IError`, `Error`, and `ValidationError`
- ✨ Error type categorization: `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `BusinessRule`, `Technical`, `External`
- ✨ Error severity levels: `Info`, `Warning`, `Error`, `Critical`
- ✨ Error metadata and timestamp support
- ✨ Implicit conversions from values and errors to Results

#### Railway-Oriented Programming
- ✨ `Map` - Transform values in Results (Functor pattern)
- ✨ `Bind` - Chain operations returning Results (Monad pattern)
- ✨ `Match` - Pattern matching for success/failure cases
- ✨ `Tap` - Execute side effects without breaking chains
- ✨ `TapError` - Execute actions on errors
- ✨ `Ensure` - Inline validation with predicates

#### Async Support
- ✨ `MapAsync` - Async value transformation
- ✨ `BindAsync` - Async operation chaining
- ✨ `MatchAsync` - Async pattern matching
- ✨ `TapAsync` - Async side effects
- ✨ `EnsureAsync` - Async validation
- ✨ `ToResultAsync` - Wrap async operations with exception handling

#### ASP.NET Core Integration
- ✨ `ToActionResult()` - Automatic conversion to HTTP responses
- ✨ RFC 7807 Problem Details support
- ✨ Automatic HTTP status code mapping based on error types
- ✨ `ValidationProblemDetails` for validation errors
- ✨ `ToCreatedAtActionResult()` for 201 Created responses
- ✨ `ToNoContentResult()` for 204 No Content responses
- ✨ Custom status code support

#### Result Combinators
- ✨ `Combine()` - Combine multiple results into one (all or nothing)
- ✨ `Zip()` - Combine 2-4 results of different types into tuples
- ✨ `FirstSuccess()` - Return first successful result (fallback pattern)
- ✨ `SuccessfulValues()` - Get only successful values, ignore failures
- ✨ `MergeErrors()` - Collect all errors from multiple results
- ✨ `Partition()` - Separate successful values and errors
- ✨ Support for both generic and non-generic results
- ✨ IEnumerable extension methods for all combinators

#### FluentValidation Integration (Optional Package)
- ✨ New package: `eQuantic.Core.Outcomes.FluentValidation`
- ✨ `ToResult()` extension for `ValidationResult`
- ✨ `Validate()` and `ValidateAsync()` extensions for `IValidator<T>`
- ✨ `Validate()` extension for `Result<T>` to chain validation
- ✨ Automatic conversion of FluentValidation errors to `ValidationError`
- ✨ Full async support for validation operations
- ✨ Comprehensive test suite for FluentValidation integration

#### Testing & Quality
- ✨ Comprehensive unit test suite with xUnit and FluentAssertions
- ✨ Tests for Result, Errors, Extensions, and Async operations
- ✨ Tests for FluentValidation integration
- ✨ High code coverage

#### Project Structure
- ✨ Modular architecture with separate assemblies
- ✨ `eQuantic.Core.Outcomes` - Core library
- ✨ `eQuantic.Core.Outcomes.FluentValidation` - Optional FluentValidation integration
- ✨ Updated solution structure with organized folders

#### Documentation
- ✨ Complete README with examples
- ✨ Analysis and improvements documentation
- ✨ Migration guide from v1.x to v2.0
- ✨ XML documentation for all public APIs
- ✨ Modern sample controller demonstrating best practices

### Changed

#### Breaking Changes
- 🔴 **Target frameworks**: Now targets .NET 6 and .NET 8 only (removed .NET Framework 4.5/4.6, .NET Standard 1.6)
- 🔴 **Result type**: Introduced new immutable `Result<T>` record type (v1.x types still available for compatibility)
- 🔴 **Dependencies**: Removed Newtonsoft.Json in favor of System.Text.Json
- 🔴 **API**: New functional programming API with Map/Bind/Match patterns

#### Improvements
- ⬆️ Updated to modern C# features (records, nullable reference types, pattern matching)
- ⬆️ Enabled implicit usings and latest C# language version
- ⬆️ Improved type safety and compile-time guarantees
- ⬆️ Better error handling with structured error types
- ⬆️ Enhanced developer experience with fluent API

### Fixed
- 🐛 Fixed typo: `Previows` → `Previous` in `PagedListResult<T>`
- 🐛 Fixed typo: `HavePreviows` → `HavePrevious` in `PagedListResult<T>`

### Deprecated
- ⚠️ Builder pattern API (Outcome.FromItemResult, etc.) is still supported but deprecated
- ⚠️ Recommend migrating to new Result<T> API for better type safety

### Removed
- ❌ Removed support for .NET Framework 4.5, 4.6.1, 4.6.2
- ❌ Removed support for .NET Standard 1.6
- ❌ Removed Newtonsoft.Json dependency
- ❌ Removed MSBump package reference

### Migration Notes

#### From v1.x Builder Pattern:
```csharp
// Old (v1.x)
var result = Outcome.FromItemResult<User>()
    .WithSuccess()
    .WithItem(user)
    .Result();

// New (v2.0)
var result = Result<User>.Success(user);
```

#### Error Handling:
```csharp
// Old (v1.x)
resultBuilder
    .WithError()
    .WithStatus(ResultStatus.NotFound)
    .WithMessage("User not found");

// New (v2.0)
Result<User>.Failure(
    Error.NotFound("USER_001", "User not found", userId)
);
```

#### Railway-Oriented Programming:
```csharp
// Old (v1.x)
var user = await _repository.GetByIdAsync(id);
if (user == null)
    return NotFound();
var email = user.Email;
if (string.IsNullOrEmpty(email))
    return BadRequest();
return Ok(email);

// New (v2.0)
return await _repository
    .GetByIdAsync(id)
    .ToResultAsync()
    .Ensure(user => user != null, Error.NotFound("USER_001", "Not found"))
    .Map(user => user.Email)
    .Ensure(email => !string.IsNullOrEmpty(email), Error.Validation("VAL001", "Email required"))
    .ToActionResult();
```

## [1.0.1.2] - 2019

### Added
- Initial release
- Basic Result Pattern implementation
- Builder pattern with fluent API
- Support for BasicResult, ItemResult, ListResult, PagedListResult
- ResultStatus enum (Success, Error, NotFound, Forbidden, NotModified)
- Message collection support
- Exception handling in builders

---

## Upgrading to v2.0

### Prerequisites
- .NET 6.0 or .NET 8.0
- Update project to use modern C# features

### Step-by-Step Guide

1. **Update package reference**:
   ```bash
   dotnet add package eQuantic.Core.Outcomes --version 2.0.0
   ```

2. **Update target framework** in your `.csproj`:
   ```xml
   <TargetFramework>net8.0</TargetFramework>
   ```

3. **Enable nullable reference types** (recommended):
   ```xml
   <Nullable>enable</Nullable>
   ```

4. **Update using statements**:
   ```csharp
   using eQuantic.Core.Outcomes;
   using eQuantic.Core.Outcomes.Errors;
   using eQuantic.Core.Outcomes.Extensions;
   using eQuantic.Core.Outcomes.AspNetCore;
   ```

5. **Migrate to new API** - See migration examples above

6. **Run tests** to ensure everything works

### Compatibility

- ✅ v1.x Builder API is still available for backward compatibility
- ✅ Gradual migration is supported
- ⚠️ Consider migrating to v2.0 API for new code
- ⚠️ Plan to migrate existing code to benefit from new features

---

For detailed examples and documentation, see [README.md](README.md) and [ANALYSIS_AND_IMPROVEMENTS.md](ANALYSIS_AND_IMPROVEMENTS.md).
