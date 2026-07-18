using eQuantic.Core.Outcomes.AspNetCore;
using eQuantic.Core.Outcomes.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace eQuantic.Core.Outcomes.AspNetCore.Tests;

public class ActionResultExtensionsTests
{
    [Fact]
    public void ToActionResult_success_returns_ok_with_value()
    {
        var result = Result<string>.Success("hello").ToActionResult();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be("hello");
    }

    [Fact]
    public void ToActionResult_void_success_returns_ok()
    {
        Result.Success().ToActionResult().Should().BeOfType<OkResult>();
    }

    [Theory]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.BusinessRule, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(ErrorType.Technical, StatusCodes.Status500InternalServerError)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void ToActionResult_failure_maps_error_type_to_status(ErrorType type, int expectedStatus)
    {
        var result = Result<string>.Failure(new Error("code", "message", type)).ToActionResult();

        var problem = ProblemOf(result, expectedStatus);
        problem.Status.Should().Be(expectedStatus);
        problem.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void ToActionResult_validation_failure_groups_errors_by_property()
    {
        var errors = new IError[]
        {
            new Error("v1", "Name is required", ErrorType.Validation,
                metadata: new Dictionary<string, object> { ["PropertyName"] = "Name" }),
            new Error("v2", "Name is too short", ErrorType.Validation,
                metadata: new Dictionary<string, object> { ["PropertyName"] = "Name" }),
            new Error("v3", "Email is invalid", ErrorType.Validation,
                metadata: new Dictionary<string, object> { ["PropertyName"] = "Email" }),
        };

        var result = Result<string>.Failure(errors).ToActionResult();

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problem = badRequest.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problem.Errors["Name"].Should().HaveCount(2);
        problem.Errors["Email"].Should().ContainSingle().Which.Should().Be("Email is invalid");
    }

    [Fact]
    public void ToActionResult_with_custom_success_status()
    {
        var result = Result<string>.Success("queued").ToActionResult(StatusCodes.Status202Accepted);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        objectResult.Value.Should().Be("queued");
    }

    [Fact]
    public void ToCreatedAtActionResult_success_builds_location()
    {
        var result = Result<string>.Success("created")
            .ToCreatedAtActionResult("GetItem", new { id = 7 });

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be("GetItem");
        created.Value.Should().Be("created");
    }

    [Fact]
    public void ToNoContentResult_success_discards_value()
    {
        Result<string>.Success("ignored").ToNoContentResult().Should().BeOfType<NoContentResult>();
        Result.Success().ToNoContentResult().Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToNoContentResult_failure_still_maps_errors()
    {
        var result = Result.Failure(new Error("code", "gone", ErrorType.NotFound)).ToNoContentResult();

        ProblemOf(result, StatusCodes.Status404NotFound).Status.Should().Be(StatusCodes.Status404NotFound);
    }

    private static ProblemDetails ProblemOf(IActionResult result, int expectedStatus)
    {
        var objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(expectedStatus);
        return objectResult.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
    }
}
