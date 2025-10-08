using eQuantic.Core.Outcomes.Errors;
using eQuantic.Core.Outcomes.Extensions;
using FluentAssertions;
using Xunit;

namespace eQuantic.Core.Outcomes.Tests;

public class ResultCombinatorsTests
{
    #region Combine Tests

    [Fact]
    public void Combine_WithAllSuccessfulResults_ShouldReturnSuccessWithAllValues()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);

        // Act
        var combined = ResultCombinators.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Combine_WithSomeFailedResults_ShouldReturnFailureWithAllErrors()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result3 = Result<int>.Failure(new Error("ERR002", "Error 2"));

        // Act
        var combined = ResultCombinators.Combine(result1, result2, result3);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
        combined.Errors.Should().Contain(e => e.Code == "ERR001");
        combined.Errors.Should().Contain(e => e.Code == "ERR002");
    }

    [Fact]
    public void Combine_WithEmptyArray_ShouldReturnSuccessWithEmptyCollection()
    {
        // Arrange & Act
        var combined = ResultCombinators.Combine<int>();

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEmpty();
    }

    [Fact]
    public void Combine_IEnumerable_ShouldWorkCorrectly()
    {
        // Arrange
        var results = new List<Result<int>>
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3)
        };

        // Act
        var combined = results.Combine();

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Combine_NonGeneric_WithAllSuccess_ShouldReturnSuccess()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        // Act
        var combined = ResultCombinators.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_NonGeneric_WithSomeFailures_ShouldReturnFailureWithAllErrors()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Failure(new Error("ERR001", "Error 1"));
        var result3 = Result.Failure(new Error("ERR002", "Error 2"));

        // Act
        var combined = ResultCombinators.Combine(result1, result2, result3);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Zip Tests

    [Fact]
    public void Zip_TwoSuccessfulResults_ShouldReturnSuccessWithTuple()
    {
        // Arrange
        var result1 = Result<int>.Success(42);
        var result2 = Result<string>.Success("test");

        // Act
        var zipped = result1.Zip(result2);

        // Assert
        zipped.IsSuccess.Should().BeTrue();
        zipped.Value.Should().Be((42, "test"));
    }

    [Fact]
    public void Zip_WithFirstFailure_ShouldReturnFailure()
    {
        // Arrange
        var result1 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result2 = Result<string>.Success("test");

        // Act
        var zipped = result1.Zip(result2);

        // Assert
        zipped.IsFailure.Should().BeTrue();
        zipped.Errors.Should().ContainSingle();
        zipped.FirstError!.Code.Should().Be("ERR001");
    }

    [Fact]
    public void Zip_WithBothFailures_ShouldReturnFailureWithAllErrors()
    {
        // Arrange
        var result1 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result2 = Result<string>.Failure(new Error("ERR002", "Error 2"));

        // Act
        var zipped = result1.Zip(result2);

        // Assert
        zipped.IsFailure.Should().BeTrue();
        zipped.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Zip_ThreeSuccessfulResults_ShouldReturnSuccessWithTuple()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<string>.Success("two");
        var result3 = Result<bool>.Success(true);

        // Act
        var zipped = result1.Zip(result2, result3);

        // Assert
        zipped.IsSuccess.Should().BeTrue();
        zipped.Value.Should().Be((1, "two", true));
    }

    [Fact]
    public void Zip_FourSuccessfulResults_ShouldReturnSuccessWithTuple()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<string>.Success("two");
        var result3 = Result<bool>.Success(true);
        var result4 = Result<double>.Success(4.0);

        // Act
        var zipped = result1.Zip(result2, result3, result4);

        // Assert
        zipped.IsSuccess.Should().BeTrue();
        zipped.Value.Should().Be((1, "two", true, 4.0));
    }

    #endregion

    #region FirstSuccess Tests

    [Fact]
    public void FirstSuccess_WithFirstResultSuccessful_ShouldReturnFirstResult()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);

        // Act
        var first = ResultCombinators.FirstSuccess(result1, result2, result3);

        // Assert
        first.IsSuccess.Should().BeTrue();
        first.Value.Should().Be(1);
    }

    [Fact]
    public void FirstSuccess_WithSecondResultSuccessful_ShouldReturnSecondResult()
    {
        // Arrange
        var result1 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);

        // Act
        var first = ResultCombinators.FirstSuccess(result1, result2, result3);

        // Assert
        first.IsSuccess.Should().BeTrue();
        first.Value.Should().Be(2);
    }

    [Fact]
    public void FirstSuccess_WithAllFailures_ShouldReturnFailureWithAllErrors()
    {
        // Arrange
        var result1 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result2 = Result<int>.Failure(new Error("ERR002", "Error 2"));
        var result3 = Result<int>.Failure(new Error("ERR003", "Error 3"));

        // Act
        var first = ResultCombinators.FirstSuccess(result1, result2, result3);

        // Assert
        first.IsFailure.Should().BeTrue();
        first.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void FirstSuccess_WithEmptyArray_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => ResultCombinators.FirstSuccess<int>();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one result*");
    }

    [Fact]
    public void FirstSuccess_IEnumerable_ShouldWorkCorrectly()
    {
        // Arrange
        var results = new List<Result<int>>
        {
            Result<int>.Failure(new Error("ERR001", "Error 1")),
            Result<int>.Success(42),
            Result<int>.Success(100)
        };

        // Act
        var first = results.FirstSuccess();

        // Assert
        first.IsSuccess.Should().BeTrue();
        first.Value.Should().Be(42);
    }

    #endregion

    #region MergeErrors Tests

    [Fact]
    public void MergeErrors_WithMultipleFailures_ShouldCombineAllErrors()
    {
        // Arrange
        var result1 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result2 = Result<int>.Failure(new Error("ERR002", "Error 2"));
        var result3 = Result<int>.Success(3);

        // Act
        var merged = ResultCombinators.MergeErrors(result1, result2, result3);

        // Assert
        merged.IsFailure.Should().BeTrue();
        merged.Errors.Should().HaveCount(2);
        merged.Errors.Should().Contain(e => e.Code == "ERR001");
        merged.Errors.Should().Contain(e => e.Code == "ERR002");
    }

    [Fact]
    public void MergeErrors_WithAllSuccesses_ShouldReturnFirstSuccess()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);

        // Act
        var merged = ResultCombinators.MergeErrors(result1, result2);

        // Assert
        merged.IsSuccess.Should().BeTrue();
        merged.Value.Should().Be(1);
    }

    [Fact]
    public void MergeErrors_NonGeneric_ShouldCombineErrors()
    {
        // Arrange
        var result1 = Result.Failure(new Error("ERR001", "Error 1"));
        var result2 = Result.Failure(new Error("ERR002", "Error 2"));

        // Act
        var merged = ResultCombinators.MergeErrors(result1, result2);

        // Assert
        merged.IsFailure.Should().BeTrue();
        merged.Errors.Should().HaveCount(2);
    }

    #endregion

    #region SuccessfulValues Tests

    [Fact]
    public void SuccessfulValues_ShouldReturnOnlySuccessfulValues()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Failure(new Error("ERR001", "Error"));
        var result3 = Result<int>.Success(3);
        var result4 = Result<int>.Failure(new Error("ERR002", "Error"));
        var result5 = Result<int>.Success(5);

        // Act
        var successful = ResultCombinators.SuccessfulValues(result1, result2, result3, result4, result5);

        // Assert
        successful.IsSuccess.Should().BeTrue();
        successful.Value.Should().BeEquivalentTo(new[] { 1, 3, 5 });
    }

    [Fact]
    public void SuccessfulValues_WithAllFailures_ShouldReturnEmptyCollection()
    {
        // Arrange
        var result1 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result2 = Result<int>.Failure(new Error("ERR002", "Error 2"));

        // Act
        var successful = ResultCombinators.SuccessfulValues(result1, result2);

        // Assert
        successful.IsSuccess.Should().BeTrue();
        successful.Value.Should().BeEmpty();
    }

    #endregion

    #region Partition Tests

    [Fact]
    public void Partition_ShouldSeparateSuccessesAndErrors()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Failure(new Error("ERR001", "Error 1"));
        var result3 = Result<int>.Success(3);
        var result4 = Result<int>.Failure(new Error("ERR002", "Error 2"));

        // Act
        var (successes, errors) = ResultCombinators.Partition(result1, result2, result3, result4);

        // Assert
        successes.Should().BeEquivalentTo(new[] { 1, 3 });
        errors.Should().HaveCount(2);
        errors.Should().Contain(e => e.Code == "ERR001");
        errors.Should().Contain(e => e.Code == "ERR002");
    }

    [Fact]
    public void Partition_WithAllSuccesses_ShouldReturnAllValuesNoErrors()
    {
        // Arrange
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);

        // Act
        var (successes, errors) = ResultCombinators.Partition(result1, result2);

        // Assert
        successes.Should().BeEquivalentTo(new[] { 1, 2 });
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Partition_IEnumerable_ShouldWorkCorrectly()
    {
        // Arrange
        var results = new List<Result<string>>
        {
            Result<string>.Success("A"),
            Result<string>.Failure(new Error("ERR001", "Error")),
            Result<string>.Success("B")
        };

        // Act
        var (successes, errors) = results.Partition();

        // Assert
        successes.Should().BeEquivalentTo(new[] { "A", "B" });
        errors.Should().ContainSingle();
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void Scenario_ValidateMultipleFields_ShouldCombineAllValidationErrors()
    {
        // Arrange - Validating a form with multiple fields
        var emailValidation = ValidateEmail("invalid-email");
        var passwordValidation = ValidatePassword("123"); // Too short
        var ageValidation = ValidateAge(15); // Too young

        // Act
        var formValidation = ResultCombinators.MergeErrors(
            emailValidation.ToResult(),
            passwordValidation.ToResult(),
            ageValidation.ToResult()
        );

        // Assert
        formValidation.IsFailure.Should().BeTrue();
        formValidation.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void Scenario_FetchFromMultipleSources_UseFirstSuccess()
    {
        // Arrange - Try to get data from cache, then DB, then API
        var cacheResult = GetFromCache();   // Fails
        var dbResult = GetFromDatabase();   // Success
        var apiResult = GetFromApi();       // Not evaluated

        // Act
        var data = ResultCombinators.FirstSuccess(cacheResult, dbResult, apiResult);

        // Assert
        data.IsSuccess.Should().BeTrue();
        data.Value.Should().Be("from-database");
    }

    [Fact]
    public void Scenario_CombineUserAndProfile_ShouldZipResults()
    {
        // Arrange
        var userResult = GetUser(1);
        var profileResult = GetProfile(1);

        // Act
        var combined = userResult.Zip(profileResult);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Item1.Should().Be("User1");
        combined.Value.Item2.Should().Be("Profile1");
    }

    [Fact]
    public void Scenario_PartialSuccess_GetSuccessfulValues()
    {
        // Arrange - Process multiple items, some may fail
        var processResults = new[]
        {
            ProcessItem("item1"), // Success
            ProcessItem("item2"), // Success
            ProcessItem("bad"),   // Failure
            ProcessItem("item4")  // Success
        };

        // Act
        var successfulItems = ResultCombinators.SuccessfulValues(processResults);

        // Assert
        successfulItems.Value.Should().HaveCount(3);
        successfulItems.Value.Should().Contain("item1-processed");
        successfulItems.Value.Should().Contain("item2-processed");
        successfulItems.Value.Should().Contain("item4-processed");
    }

    // Helper methods for scenarios
    private Result<string> ValidateEmail(string email) =>
        email.Contains("@")
            ? Result<string>.Success(email)
            : Result<string>.Failure(Error.Validation("VAL001", "Invalid email"));

    private Result<string> ValidatePassword(string password) =>
        password.Length >= 8
            ? Result<string>.Success(password)
            : Result<string>.Failure(Error.Validation("VAL002", "Password too short"));

    private Result<int> ValidateAge(int age) =>
        age >= 18
            ? Result<int>.Success(age)
            : Result<int>.Failure(Error.Validation("VAL003", "Must be 18 or older"));

    private Result<string> GetFromCache() =>
        Result<string>.Failure(Error.NotFound("CACHE_MISS", "Not in cache"));

    private Result<string> GetFromDatabase() =>
        Result<string>.Success("from-database");

    private Result<string> GetFromApi() =>
        Result<string>.Success("from-api");

    private Result<string> GetUser(int id) =>
        Result<string>.Success($"User{id}");

    private Result<string> GetProfile(int id) =>
        Result<string>.Success($"Profile{id}");

    private Result<string> ProcessItem(string item) =>
        item == "bad"
            ? Result<string>.Failure(new Error("PROC001", "Bad item"))
            : Result<string>.Success($"{item}-processed");

    #endregion
}
