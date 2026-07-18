# eQuantic.Core.Outcomes.AspNetCore

ASP.NET Core integration for
[eQuantic.Core.Outcomes](https://www.nuget.org/packages/eQuantic.Core.Outcomes): convert `Result`
types into HTTP responses with automatic status-code mapping and **RFC 7807 Problem Details**.

**MVC controllers:**

```csharp
using eQuantic.Core.Outcomes.AspNetCore;

[HttpGet("{id}")]
public IActionResult Get(int id) =>
    userService.GetUser(id).ToActionResult();
    // Success → 200 with the value; NotFound → 404; Validation → 400;
    // Conflict → 409; Unauthorized → 401 … all as Problem Details.
```

**Minimal APIs** (`TypedResults`-based):

```csharp
app.MapGet("/users/{id}", (int id, IUserService users) =>
    users.GetUser(id).ToHttpResult());

app.MapPost("/users", (CreateUser request, IUserService users) =>
    users.Create(request).ToCreatedHttpResult(user => $"/users/{user.Id}"));

app.MapDelete("/users/{id}", (int id, IUserService users) =>
    users.Delete(id).ToNoContentHttpResult());
```

The `ErrorType` of each failure drives the HTTP status; validation errors are grouped into the
Problem Details `errors` dictionary. The whole translation is **extensible**: derive from
`OutcomeHttpMapping`, override what you need and pass it to any of the extensions —

```csharp
public sealed class MyMapping : OutcomeHttpMapping
{
    public override int GetStatusCode(IError error) =>
        error.Type == ErrorType.BusinessRule ? StatusCodes.Status409Conflict
                                             : base.GetStatusCode(error);
}

result.ToHttpResult(new MyMapping());     // both adapters honor the same mapping
result.ToActionResult(new MyMapping());
```

> Until v2, these extensions shipped inside the core package (forcing every consumer to reference
> `Microsoft.AspNetCore.App`). From v3 they live here, and the core stays dependency-free.

Targets net8.0/net10.0. Full documentation: <https://github.com/eQuantic/core-outcomes>

MIT © eQuantic Tech
