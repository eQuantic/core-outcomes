# eQuantic.Core.Outcomes.FluentValidation

[FluentValidation](https://www.nuget.org/packages/FluentValidation) integration for
[eQuantic.Core.Outcomes](https://www.nuget.org/packages/eQuantic.Core.Outcomes): turn validation
outcomes into typed `Result` failures that flow through the Railway-Oriented pipeline.

```csharp
using eQuantic.Core.Outcomes.FluentValidation;

var validation = await validator.ValidateAsync(request);

Result<User> result = validation.ToResult(request)   // failures become typed ValidationErrors
    .Bind(req => CreateUser(req));
```

Combine with
[eQuantic.Core.Outcomes.AspNetCore](https://www.nuget.org/packages/eQuantic.Core.Outcomes.AspNetCore)
and invalid requests surface as RFC 7807 Problem Details with the field-level `errors` dictionary —
no exceptions, no manual mapping.

Targets net8.0/net10.0, FluentValidation 12. Full documentation:
<https://github.com/eQuantic/core-outcomes>

MIT © eQuantic Tech
