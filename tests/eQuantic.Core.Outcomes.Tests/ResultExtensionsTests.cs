using eQuantic.Core.Outcomes.Errors;
using eQuantic.Core.Outcomes.Extensions;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.Outcomes.Tests;

public class ResultExtensionsTests
{
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

    [Fact]
    public void Map_OnFailedResult_ShouldReturnFailedResult()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");
        var result = Result<int>.Failure(error);

        // Act
        var mappedResult = result.Map(x => x * 2);

        // Assert
        mappedResult.IsFailure.Should().BeTrue();
        mappedResult.FirstError.Should().Be(error);
    }

    [Fact]
    public void Bind_OnSuccessResult_ShouldBindValue()
    {
        // Arrange
        var result = Result<int>.Success(5);

        // Act
        var boundResult = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        boundResult.IsSuccess.Should().BeTrue();
        boundResult.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_OnFailedResult_ShouldReturnFailedResult()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");
        var result = Result<int>.Failure(error);

        // Act
        var boundResult = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        boundResult.IsFailure.Should().BeTrue();
        boundResult.FirstError.Should().Be(error);
    }

    [Fact]
    public void Match_OnSuccessResult_ShouldExecuteSuccessFunction()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var output = result.Match(
            onSuccess: x => $"Success: {x}",
            onFailure: _ => "Failed");

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public void Match_OnFailedResult_ShouldExecuteFailureFunction()
    {
        // Arrange
        var result = Result<int>.Failure(new Error("ERR001", "Test error"));

        // Act
        var output = result.Match(
            onSuccess: x => $"Success: {x}",
            onFailure: errors => $"Failed: {errors.First().Message}");

        // Assert
        output.Should().Be("Failed: Test error");
    }

    [Fact]
    public void Tap_OnSuccessResult_ShouldExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var sideEffect = 0;

        // Act
        var tappedResult = result.Tap(x => sideEffect = x);

        // Assert
        sideEffect.Should().Be(42);
        tappedResult.Should().Be(result);
        tappedResult.Value.Should().Be(42);
    }

    [Fact]
    public void Tap_OnFailedResult_ShouldNotExecuteAction()
    {
        // Arrange
        var result = Result<int>.Failure(new Error("ERR001", "Test error"));
        var sideEffect = 0;

        // Act
        var tappedResult = result.Tap(x => sideEffect = x);

        // Assert
        sideEffect.Should().Be(0);
        tappedResult.Should().Be(result);
    }

    [Fact]
    public void TapError_OnFailedResult_ShouldExecuteAction()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");
        var result = Result<int>.Failure(error);
        IReadOnlyList<IError>? capturedErrors = null;

        // Act
        var tappedResult = result.TapError(errors => capturedErrors = errors);

        // Assert
        capturedErrors.Should().ContainSingle();
        capturedErrors![0].Should().Be(error);
        tappedResult.Should().Be(result);
    }

    [Fact]
    public void Ensure_WithValidPredicate_ShouldReturnOriginalResult()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var ensuredResult = result.Ensure(
            x => x > 0,
            new Error("ERR001", "Value must be positive"));

        // Assert
        ensuredResult.IsSuccess.Should().BeTrue();
        ensuredResult.Value.Should().Be(42);
    }

    [Fact]
    public void Ensure_WithInvalidPredicate_ShouldReturnFailedResult()
    {
        // Arrange
        var result = Result<int>.Success(-5);
        var error = new Error("ERR001", "Value must be positive");

        // Act
        var ensuredResult = result.Ensure(x => x > 0, error);

        // Assert
        ensuredResult.IsFailure.Should().BeTrue();
        ensuredResult.FirstError.Should().Be(error);
    }

    [Fact]
    public void Ensure_OnFailedResult_ShouldReturnOriginalFailedResult()
    {
        // Arrange
        var originalError = new Error("ERR001", "Original error");
        var result = Result<int>.Failure(originalError);

        // Act
        var ensuredResult = result.Ensure(
            x => x > 0,
            new Error("ERR002", "Value must be positive"));

        // Assert
        ensuredResult.IsFailure.Should().BeTrue();
        ensuredResult.FirstError.Should().Be(originalError);
    }

    [Fact]
    public void ToResult_OnSuccessResult_ShouldReturnNonGenericSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var nonGenericResult = result.ToResult();

        // Assert
        nonGenericResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_OnFailedResult_ShouldReturnNonGenericFailure()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");
        var result = Result<int>.Failure(error);

        // Act
        var nonGenericResult = result.ToResult();

        // Assert
        nonGenericResult.IsFailure.Should().BeTrue();
        nonGenericResult.FirstError.Should().Be(error);
    }

    [Fact]
    public void Chaining_MultipleOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var result = Result<int>.Success(5);

        // Act
        var finalResult = result
            .Map(x => x * 2)
            .Bind(x => Result<int>.Success(x + 10))
            .Ensure(x => x > 15, new Error("ERR001", "Too small"))
            .Map(x => x.ToString());

        // Assert
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().Be("20");
    }

    [Fact]
    public void Chaining_WithFailureInMiddle_ShouldShortCircuit()
    {
        // Arrange
        var result = Result<int>.Success(5);
        var executedMap2 = false;

        // Act
        var finalResult = result
            .Map(x => x * 2)
            .Bind(x => Result<int>.Failure(new Error("ERR001", "Failed in bind")))
            .Map(x =>
            {
                executedMap2 = true;
                return x.ToString();
            });

        // Assert
        finalResult.IsFailure.Should().BeTrue();
        executedMap2.Should().BeFalse();
    }
}
