using eQuantic.Core.Outcomes.Errors;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.Outcomes.Tests;

public class ErrorTests
{
    [Fact]
    public void Constructor_ShouldCreateErrorWithAllProperties()
    {
        // Arrange
        var code = "ERR001";
        var message = "Test error";
        var type = ErrorType.Validation;
        var severity = ErrorSeverity.Warning;

        // Act
        var error = new Error(code, message, type, severity);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(type);
        error.Severity.Should().Be(severity);
        error.Metadata.Should().NotBeNull();
        error.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Validation_ShouldCreateValidationError()
    {
        // Arrange & Act
        var error = Error.Validation("VAL001", "Invalid email", "Email");

        // Assert
        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("VAL001");
        error.Message.Should().Be("Invalid email");
        error.Metadata.Should().ContainKey("PropertyName");
        error.Metadata["PropertyName"].Should().Be("Email");
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        // Arrange & Act
        var error = Error.NotFound("NF001", "User not found", "user123");

        // Assert
        error.Type.Should().Be(ErrorType.NotFound);
        error.Code.Should().Be("NF001");
        error.Message.Should().Be("User not found");
        error.Metadata.Should().ContainKey("ResourceId");
        error.Metadata["ResourceId"].Should().Be("user123");
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        // Arrange & Act
        var error = Error.Conflict("CNF001", "Email already exists");

        // Assert
        error.Type.Should().Be(ErrorType.Conflict);
        error.Code.Should().Be("CNF001");
        error.Message.Should().Be("Email already exists");
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedError()
    {
        // Arrange & Act
        var error = Error.Unauthorized("AUTH001", "Invalid credentials");

        // Assert
        error.Type.Should().Be(ErrorType.Unauthorized);
        error.Code.Should().Be("AUTH001");
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenError()
    {
        // Arrange & Act
        var error = Error.Forbidden("FORB001", "Access denied");

        // Assert
        error.Type.Should().Be(ErrorType.Forbidden);
        error.Code.Should().Be("FORB001");
    }

    [Fact]
    public void BusinessRule_ShouldCreateBusinessRuleError()
    {
        // Arrange & Act
        var error = Error.BusinessRule("BIZ001", "Cannot delete active user");

        // Assert
        error.Type.Should().Be(ErrorType.BusinessRule);
        error.Code.Should().Be("BIZ001");
    }

    [Fact]
    public void Technical_ShouldCreateTechnicalError()
    {
        // Arrange
        var exception = new InvalidOperationException("Database connection failed");

        // Act
        var error = Error.Technical("TECH001", "System error", exception);

        // Assert
        error.Type.Should().Be(ErrorType.Technical);
        error.Code.Should().Be("TECH001");
        error.Exception.Should().Be(exception);
    }

    [Fact]
    public void External_ShouldCreateExternalError()
    {
        // Arrange
        var exception = new HttpRequestException("API timeout");

        // Act
        var error = Error.External("EXT001", "External service unavailable", exception);

        // Assert
        error.Type.Should().Be(ErrorType.External);
        error.Code.Should().Be("EXT001");
        error.Exception.Should().Be(exception);
    }

    [Fact]
    public void FromException_ShouldCreateErrorFromException()
    {
        // Arrange
        var exception = new ArgumentNullException("param");

        // Act
        var error = Error.FromException(exception);

        // Assert
        error.Code.Should().Be("ArgumentNullException");
        error.Message.Should().Contain("param");
        error.Type.Should().Be(ErrorType.Technical);
        error.Exception.Should().Be(exception);
    }

    [Fact]
    public void FromException_WithCustomCode_ShouldUseCustomCode()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");

        // Act
        var error = Error.FromException(exception, "CUSTOM001", ErrorType.BusinessRule);

        // Assert
        error.Code.Should().Be("CUSTOM001");
        error.Type.Should().Be(ErrorType.BusinessRule);
    }

    [Fact]
    public void ValidationError_ShouldStorePropertyDetails()
    {
        // Arrange & Act
        var error = new ValidationError("VAL001", "Email is required", "Email", "invalid@");

        // Assert
        error.Type.Should().Be(ErrorType.Validation);
        error.PropertyName.Should().Be("Email");
        error.AttemptedValue.Should().Be("invalid@");
    }
}
