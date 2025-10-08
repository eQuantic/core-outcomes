using eQuantic.Core.Outcomes.Errors;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.Outcomes.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange & Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailedResult_ShouldThrowException()
    {
        // Arrange
        var result = Result<int>.Failure(new Error("ERR001", "Test error"));

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result*");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Arrange & Act
        Result<string> result = "test";

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");

        // Act
        Result<int> result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public void NonGenericResult_Success_ShouldCreateSuccessfulResult()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void NonGenericResult_Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("ERR001", "Test error");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.FirstError.Should().Be(error);
    }

    [Fact]
    public void Failure_WithoutErrors_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => Result<int>.Failure(Array.Empty<IError>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one error*");
    }
}
