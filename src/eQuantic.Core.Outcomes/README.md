# eQuantic.Core.Outcomes

A modern, type-safe **Result Pattern** for .NET with **Railway-Oriented Programming**: handle
success and failure elegantly, without exceptions — immutable results, typed errors with
severity/categorization, combinators, full async support and built-in observability
(correlation IDs, execution timing).

```csharp
using eQuantic.Core.Outcomes;
using eQuantic.Core.Outcomes.Errors;
using eQuantic.Core.Outcomes.Extensions;

Result<User> result = GetUser(id)                 // Result<User>.Success(user) / .Failure(error)
    .Ensure(user => user.IsActive, new Error("user.inactive", "User is inactive"))
    .Bind(user => LoadProfile(user))              // Result-returning step
    .Map(profile => profile.DisplayName)          // plain transformation
    .Tap(name => logger.LogInformation("Loaded {Name}", name));

var response = result.Match(
    onSuccess: name => $"Hello {name}",
    onFailure: errors => $"Failed: {errors[0].Message}");

// Async pipelines: MapAsync / BindAsync / TapAsync mirror the whole surface.
```

- **Dependency-free core** — no ASP.NET Core reference; usable in any app model (console,
  workers, libraries).
- **Typed errors** (`Error`, `ValidationError`) with `ErrorType`/`ErrorSeverity` driving HTTP
  mapping downstream.
- **Result shapes**: `BasicResult`, `ItemResult<T>`, `ListResult<T>`, `PagedListResult<T>` with
  fluent builders and System.Text.Json-friendly serialization.
- **Combinators** to aggregate multiple results; async variants throughout.

## Companions

| Package | Purpose |
|---------|---------|
| [eQuantic.Core.Outcomes.AspNetCore](https://www.nuget.org/packages/eQuantic.Core.Outcomes.AspNetCore) | `Result` → `IActionResult`/HTTP with automatic status mapping and RFC 7807 Problem Details. |
| [eQuantic.Core.Outcomes.FluentValidation](https://www.nuget.org/packages/eQuantic.Core.Outcomes.FluentValidation) | FluentValidation results as typed `Result` failures. |

Targets net8.0/net10.0. Full documentation: <https://github.com/eQuantic/core-outcomes>

MIT © eQuantic Tech
