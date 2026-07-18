# eQuantic.Core.Outcomes.AspNetCore

ASP.NET Core integration for
[eQuantic.Core.Outcomes](https://www.nuget.org/packages/eQuantic.Core.Outcomes): convert `Result`
types into HTTP responses with automatic status-code mapping and **RFC 7807 Problem Details**.

```csharp
using eQuantic.Core.Outcomes.AspNetCore;

[HttpGet("{id}")]
public IActionResult Get(int id) =>
    userService.GetUser(id).ToActionResult();
    // Success → 200 with the value; NotFound → 404; Validation → 400;
    // Conflict → 409; Unauthorized → 401 … all as Problem Details.
```

The `ErrorType` of each failure drives the HTTP status; validation errors are grouped into the
Problem Details `errors` dictionary.

> Until v2, these extensions shipped inside the core package (forcing every consumer to reference
> `Microsoft.AspNetCore.App`). From v3 they live here, and the core stays dependency-free.

Targets net8.0/net10.0. Full documentation: <https://github.com/eQuantic/core-outcomes>

MIT © eQuantic Tech
