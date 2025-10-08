using eQuantic.Core.Outcomes.Errors;
using eQuantic.Core.Outcomes.Extensions;
using eQuantic.Core.Outcomes.FluentValidation;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace eQuantic.Core.Outcomes.FluentValidation.Tests;

public class FluentValidationExtensionsTests
{
    [Fact]
    public void ToResult_WithValidValidationResult_ShouldReturnSuccessResult()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var validator = new UserValidator();
        var validationResult = validator.Validate(user);

        // Act
        var result = validationResult.ToResult(user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(user);
    }

    [Fact]
    public void ToResult_WithInvalidValidationResult_ShouldReturnFailureResult()
    {
        // Arrange
        var user = new User { Email = "", Age = 15 }; // Invalid
        var validator = new UserValidator();
        var validationResult = validator.Validate(user);

        // Act
        var result = validationResult.ToResult(user);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2); // NotEmpty + EmailAddress for Email, + Age
        result.Errors.Should().AllBeOfType<ValidationError>();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.PropertyName == "Email");
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.PropertyName == "Age");
    }

    [Fact]
    public void ToResult_NonGeneric_WithValidValidationResult_ShouldReturnSuccess()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var validator = new UserValidator();
        var validationResult = validator.Validate(user);

        // Act
        var result = validationResult.ToResult();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidator_ShouldReturnSuccessForValidObject()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var validator = new UserValidator();

        // Act
        var validationResult = validator.Validate(user);
        var result = validationResult.ToResult(user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(user);
    }

    [Fact]
    public void Validate_WithValidator_ShouldReturnFailureForInvalidObject()
    {
        // Arrange
        var user = new User { Email = "invalid", Age = 15 };
        var validator = new UserValidator();

        // Act
        var validationResult = validator.Validate(user);
        var result = validationResult.ToResult(user);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.PropertyName == "Email");
        result.Errors.OfType<ValidationError>().Should().Contain(e => e.PropertyName == "Age");
    }

    [Fact]
    public async Task ValidateAsync_WithValidator_ShouldReturnSuccessForValidObject()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var validator = new UserValidator();

        // Act
        var validationResult = await validator.ValidateAsync(user);
        var result = validationResult.ToResult(user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(user);
    }

    [Fact]
    public void ValidateResultExtension_OnSuccessResult_ShouldValidate()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var result = Result<User>.Success(user);
        var validator = new UserValidator();

        // Act
        var validatedResult = result.Validate(validator);

        // Assert
        validatedResult.IsSuccess.Should().BeTrue();
        validatedResult.Value.Should().Be(user);
    }

    [Fact]
    public void ValidateResultExtension_OnSuccessResultWithInvalidData_ShouldReturnFailure()
    {
        // Arrange
        var user = new User { Email = "", Age = 15 };
        var result = Result<User>.Success(user);
        var validator = new UserValidator();

        // Act
        var validatedResult = result.Validate(validator);

        // Assert
        validatedResult.IsFailure.Should().BeTrue();
        validatedResult.Errors.Should().HaveCountGreaterThanOrEqualTo(2); // NotEmpty + EmailAddress + Age
    }

    [Fact]
    public void ValidateResultExtension_OnFailedResult_ShouldReturnOriginalFailure()
    {
        // Arrange
        var originalError = Error.NotFound("USER_001", "User not found");
        var result = Result<User>.Failure(originalError);
        var validator = new UserValidator();

        // Act
        var validatedResult = result.Validate(validator);

        // Assert
        validatedResult.IsFailure.Should().BeTrue();
        validatedResult.FirstError.Should().Be(originalError);
    }

    [Fact]
    public async Task ValidateAsyncResultExtension_OnSuccessResult_ShouldValidate()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var result = Result<User>.Success(user);
        var validator = new UserValidator();

        // Act
        var validatedResult = await result.ValidateAsync(validator);

        // Assert
        validatedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsyncTaskResult_ShouldValidate()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var resultTask = Task.FromResult(Result<User>.Success(user));
        var validator = new UserValidator();

        // Act
        var validatedResult = await resultTask.ValidateAsync(validator);

        // Assert
        validatedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidationError_ShouldContainPropertyNameAndAttemptedValue()
    {
        // Arrange
        var user = new User { Email = "invalid", Age = 15 };
        var validator = new UserValidator();

        // Act
        var validationResult = validator.Validate(user);
        var result = validationResult.ToResult(user);

        // Assert
        result.IsFailure.Should().BeTrue();

        var emailError = result.Errors.OfType<ValidationError>().FirstOrDefault(e => e.PropertyName == "Email");
        emailError.Should().NotBeNull();
        emailError!.AttemptedValue.Should().Be("invalid");
        emailError.Code.Should().Be("EmailValidator");

        var ageError = result.Errors.OfType<ValidationError>().FirstOrDefault(e => e.PropertyName == "Age");
        ageError.Should().NotBeNull();
        ageError!.AttemptedValue.Should().Be(15);
    }

    [Fact]
    public void IntegrationWithRailway_ValidateInChain_ShouldWork()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Age = 25 };
        var validator = new UserValidator();

        // Act
        var result = Result<User>.Success(user)
            .Validate(validator)
            .Map(u => u.Email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void IntegrationWithRailway_InvalidDataInChain_ShouldShortCircuit()
    {
        // Arrange
        var user = new User { Email = "", Age = 15 };
        var validator = new UserValidator();
        var mapExecuted = false;

        // Act
        var result = Result<User>.Success(user)
            .Validate(validator)
            .Map(u =>
            {
                mapExecuted = true;
                return u.Email;
            });

        // Assert
        result.IsFailure.Should().BeTrue();
        mapExecuted.Should().BeFalse();
    }
}

// Test models and validators
public class User
{
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Must be 18 or older");
    }
}
