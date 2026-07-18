using eQuantic.Core.Outcomes.AspNetCore;
using eQuantic.Core.Outcomes.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace eQuantic.Core.Outcomes.AspNetCore.Tests;

public class HttpResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_success_returns_ok_with_value()
    {
        var result = Result<string>.Success("hello").ToHttpResult();

        var ok = result.Should().BeOfType<Ok<string>>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().Be("hello");
    }

    [Fact]
    public void ToHttpResult_void_success_returns_ok()
    {
        Result.Success().ToHttpResult().Should().BeOfType<Ok>();
    }

    [Theory]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.BusinessRule, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(ErrorType.External, StatusCodes.Status500InternalServerError)]
    public void ToHttpResult_failure_maps_error_type_to_status(ErrorType type, int expectedStatus)
    {
        var result = Result<string>.Failure(new Error("code", "message", type)).ToHttpResult();

        var problem = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(expectedStatus);
        problem.ProblemDetails.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void ToHttpResult_validation_failure_returns_validation_problem()
    {
        var errors = new IError[]
        {
            new Error("v1", "Name is required", ErrorType.Validation,
                metadata: new Dictionary<string, object> { ["PropertyName"] = "Name" }),
            new Error("v2", "Email is invalid", ErrorType.Validation,
                metadata: new Dictionary<string, object> { ["PropertyName"] = "Email" }),
        };

        var result = Result<string>.Failure(errors).ToHttpResult();

        var validation = result.Should().BeOfType<ValidationProblem>().Subject;
        validation.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        validation.ProblemDetails.Errors["Name"].Should().ContainSingle().Which.Should().Be("Name is required");
        validation.ProblemDetails.Errors["Email"].Should().ContainSingle();
        validation.ProblemDetails.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void ToHttpResult_with_custom_success_status()
    {
        var result = Result<string>.Success("queued").ToHttpResult(StatusCodes.Status202Accepted);

        var json = result.Should().BeOfType<JsonHttpResult<string>>().Subject;
        json.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        json.Value.Should().Be("queued");
    }

    [Fact]
    public void ToCreatedHttpResult_success_with_explicit_location()
    {
        var result = Result<string>.Success("created").ToCreatedHttpResult("/items/7");

        var created = result.Should().BeOfType<Created<string>>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Location.Should().Be("/items/7");
        created.Value.Should().Be("created");
    }

    [Fact]
    public void ToCreatedHttpResult_success_with_location_factory()
    {
        var result = Result<int>.Success(7).ToCreatedHttpResult(id => $"/items/{id}");

        var created = result.Should().BeOfType<Created<int>>().Subject;
        created.Location.Should().Be("/items/7");
    }

    [Fact]
    public void ToNoContentHttpResult_success_discards_value()
    {
        Result<string>.Success("ignored").ToNoContentHttpResult().Should().BeOfType<NoContent>();
        Result.Success().ToNoContentHttpResult().Should().BeOfType<NoContent>();
    }

    [Fact]
    public void ToNoContentHttpResult_failure_still_maps_errors()
    {
        var result = Result.Failure(new Error("code", "gone", ErrorType.NotFound)).ToNoContentHttpResult();

        result.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}

public class OutcomeHttpMappingTests
{
    private sealed class BusinessRuleAsConflictMapping : OutcomeHttpMapping
    {
        public override int GetStatusCode(IError error) =>
            error.Type == ErrorType.BusinessRule
                ? StatusCodes.Status409Conflict
                : base.GetStatusCode(error);
    }

    [Fact]
    public void Custom_mapping_drives_the_minimal_api_adapter()
    {
        var result = Result<string>.Failure(new Error("rule", "violated", ErrorType.BusinessRule))
            .ToHttpResult(new BusinessRuleAsConflictMapping());

        result.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void Custom_mapping_drives_the_mvc_adapter()
    {
        var actionResult = Result<string>.Failure(new Error("rule", "violated", ErrorType.BusinessRule))
            .ToActionResult(new BusinessRuleAsConflictMapping());

        actionResult.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void Default_mapping_titles_and_details_are_stable()
    {
        var error = new Error("code", "message", ErrorType.NotFound);

        OutcomeHttpMapping.Default.GetTitle(error).Should().Be("Not Found");
        OutcomeHttpMapping.Default.GetDetail(error).Should().Be("The requested resource was not found.");
        OutcomeHttpMapping.Default.GetStatusCode(error).Should().Be(StatusCodes.Status404NotFound);
    }
}
