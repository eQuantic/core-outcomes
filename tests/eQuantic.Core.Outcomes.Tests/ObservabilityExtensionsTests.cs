using eQuantic.Core.Outcomes.Errors;
using eQuantic.Core.Outcomes.Extensions;
using Xunit;

namespace eQuantic.Core.Outcomes.Tests;

public class ObservabilityExtensionsTests
{
    [Fact]
    public void WithCorrelationId_ShouldAddCorrelationIdToGenericResult()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var correlationId = "corr-123";

        // Act
        var resultWithCorrelation = result.WithCorrelationId(correlationId);

        // Assert
        Assert.Equal(correlationId, resultWithCorrelation.CorrelationId);
        Assert.Equal(42, resultWithCorrelation.Value);
    }

    [Fact]
    public void WithCorrelationId_ShouldAddCorrelationIdToNonGenericResult()
    {
        // Arrange
        var result = Result.Success();
        var correlationId = "corr-456";

        // Act
        var resultWithCorrelation = result.WithCorrelationId(correlationId);

        // Assert
        Assert.Equal(correlationId, resultWithCorrelation.CorrelationId);
        Assert.True(resultWithCorrelation.IsSuccess);
    }

    [Fact]
    public void WithTraceId_ShouldAddTraceIdToGenericResult()
    {
        // Arrange
        var result = Result<string>.Success("test");
        var traceId = "trace-789";

        // Act
        var resultWithTrace = result.WithTraceId(traceId);

        // Assert
        Assert.Equal(traceId, resultWithTrace.TraceId);
        Assert.Equal("test", resultWithTrace.Value);
    }

    [Fact]
    public void WithTraceId_ShouldAddTraceIdToNonGenericResult()
    {
        // Arrange
        var result = Result.Success();
        var traceId = "trace-abc";

        // Act
        var resultWithTrace = result.WithTraceId(traceId);

        // Assert
        Assert.Equal(traceId, resultWithTrace.TraceId);
        Assert.True(resultWithTrace.IsSuccess);
    }

    [Fact]
    public void WithExecutionTime_ShouldAddExecutionTimeToGenericResult()
    {
        // Arrange
        var result = Result<bool>.Success(true);
        var executionTime = TimeSpan.FromMilliseconds(150);

        // Act
        var resultWithTime = result.WithExecutionTime(executionTime);

        // Assert
        Assert.Equal(executionTime, resultWithTime.ExecutionTime);
        Assert.True(resultWithTime.Value);
    }

    [Fact]
    public void WithExecutionTime_ShouldAddExecutionTimeToNonGenericResult()
    {
        // Arrange
        var result = Result.Success();
        var executionTime = TimeSpan.FromSeconds(2);

        // Act
        var resultWithTime = result.WithExecutionTime(executionTime);

        // Assert
        Assert.Equal(executionTime, resultWithTime.ExecutionTime);
        Assert.True(resultWithTime.IsSuccess);
    }

    [Fact]
    public void WithMetadata_ShouldAddSingleMetadataEntryToGenericResult()
    {
        // Arrange
        var result = Result<int>.Success(100);

        // Act
        var resultWithMeta = result.WithMetadata("userId", "user-123");

        // Assert
        Assert.Contains("userId", resultWithMeta.Metadata.Keys);
        Assert.Equal("user-123", resultWithMeta.Metadata["userId"]);
        Assert.Equal(100, resultWithMeta.Value);
    }

    [Fact]
    public void WithMetadata_ShouldAddSingleMetadataEntryToNonGenericResult()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var resultWithMeta = result.WithMetadata("requestId", "req-456");

        // Assert
        Assert.Contains("requestId", resultWithMeta.Metadata.Keys);
        Assert.Equal("req-456", resultWithMeta.Metadata["requestId"]);
    }

    [Fact]
    public void WithMetadata_ShouldAddMultipleMetadataEntriesToGenericResult()
    {
        // Arrange
        var result = Result<string>.Success("data");
        var metadata = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42,
            ["key3"] = true
        };

        // Act
        var resultWithMeta = result.WithMetadata(metadata);

        // Assert
        Assert.Equal(3, resultWithMeta.Metadata.Count);
        Assert.Equal("value1", resultWithMeta.Metadata["key1"]);
        Assert.Equal(42, resultWithMeta.Metadata["key2"]);
        Assert.Equal(true, resultWithMeta.Metadata["key3"]);
    }

    [Fact]
    public void WithMetadata_ShouldAddMultipleMetadataEntriesToNonGenericResult()
    {
        // Arrange
        var result = Result.Success();
        var metadata = new Dictionary<string, object>
        {
            ["source"] = "api",
            ["version"] = 2
        };

        // Act
        var resultWithMeta = result.WithMetadata(metadata);

        // Assert
        Assert.Equal(2, resultWithMeta.Metadata.Count);
        Assert.Equal("api", resultWithMeta.Metadata["source"]);
        Assert.Equal(2, resultWithMeta.Metadata["version"]);
    }

    [Fact]
    public void WithMetadata_ShouldMergeWithExistingMetadata()
    {
        // Arrange
        var result = Result<int>.Success(50)
            .WithMetadata("existing", "value");

        // Act
        var resultWithNewMeta = result.WithMetadata("new", "data");

        // Assert
        Assert.Equal(2, resultWithNewMeta.Metadata.Count);
        Assert.Equal("value", resultWithNewMeta.Metadata["existing"]);
        Assert.Equal("data", resultWithNewMeta.Metadata["new"]);
    }

    [Fact]
    public void ChainedObservabilityMethods_ShouldWorkTogether()
    {
        // Arrange
        var result = Result<string>.Success("test");

        // Act
        var enrichedResult = result
            .WithCorrelationId("corr-123")
            .WithTraceId("trace-456")
            .WithExecutionTime(TimeSpan.FromMilliseconds(100))
            .WithMetadata("env", "production")
            .WithMetadata("region", "us-east-1");

        // Assert
        Assert.Equal("corr-123", enrichedResult.CorrelationId);
        Assert.Equal("trace-456", enrichedResult.TraceId);
        Assert.Equal(TimeSpan.FromMilliseconds(100), enrichedResult.ExecutionTime);
        Assert.Equal("production", enrichedResult.Metadata["env"]);
        Assert.Equal("us-east-1", enrichedResult.Metadata["region"]);
        Assert.Equal("test", enrichedResult.Value);
    }

    [Fact]
    public void Timed_ShouldMeasureExecutionTimeForGenericResult()
    {
        // Arrange & Act
        var result = ObservabilityExtensions.Timed(() =>
        {
            Thread.Sleep(50); // Simulate work
            return Result<int>.Success(42);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.NotNull(result.ExecutionTime);
        Assert.True(result.ExecutionTime.Value.TotalMilliseconds >= 50);
    }

    [Fact]
    public void Timed_ShouldMeasureExecutionTimeForNonGenericResult()
    {
        // Arrange & Act
        var result = ObservabilityExtensions.Timed(() =>
        {
            Thread.Sleep(50); // Simulate work
            return Result.Success();
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ExecutionTime);
        Assert.True(result.ExecutionTime.Value.TotalMilliseconds >= 50);
    }

    [Fact]
    public void Timed_ShouldCaptureFailureResult()
    {
        // Arrange & Act
        var result = ObservabilityExtensions.Timed(() =>
        {
            Thread.Sleep(30);
            return Result<int>.Failure(Error.Validation("ERR001", "Invalid input"));
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.ExecutionTime);
        Assert.True(result.ExecutionTime.Value.TotalMilliseconds >= 30);
        Assert.Equal("ERR001", result.FirstError?.Code);
    }

    [Fact]
    public async Task TimedAsync_ShouldMeasureExecutionTimeForGenericResult()
    {
        // Arrange & Act
        var result = await ObservabilityExtensions.TimedAsync(async () =>
        {
            await Task.Delay(50); // Simulate async work
            return Result<string>.Success("async result");
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("async result", result.Value);
        Assert.NotNull(result.ExecutionTime);
        Assert.True(result.ExecutionTime.Value.TotalMilliseconds >= 50);
    }

    [Fact]
    public async Task TimedAsync_ShouldMeasureExecutionTimeForNonGenericResult()
    {
        // Arrange & Act
        var result = await ObservabilityExtensions.TimedAsync(async () =>
        {
            await Task.Delay(50); // Simulate async work
            return Result.Success();
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ExecutionTime);
        Assert.True(result.ExecutionTime.Value.TotalMilliseconds >= 50);
    }

    [Fact]
    public async Task TimedAsync_ShouldCaptureFailureResult()
    {
        // Arrange & Act
        var result = await ObservabilityExtensions.TimedAsync(async () =>
        {
            await Task.Delay(30);
            return Result.Failure(Error.NotFound("ERR404", "Resource not found"));
        });

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.ExecutionTime);
        Assert.True(result.ExecutionTime.Value.TotalMilliseconds >= 30);
        Assert.Equal("ERR404", result.FirstError?.Code);
    }

    [Fact]
    public void ObservabilityData_ShouldPersistOnFailedResults()
    {
        // Arrange
        var error = Error.Validation("VAL001", "Validation failed");
        var result = Result<int>.Failure(error)
            .WithCorrelationId("corr-999")
            .WithTraceId("trace-999")
            .WithMetadata("source", "test");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("corr-999", result.CorrelationId);
        Assert.Equal("trace-999", result.TraceId);
        Assert.Equal("test", result.Metadata["source"]);
        Assert.Equal("VAL001", result.FirstError?.Code);
    }

    [Fact]
    public void ObservabilityMetadata_ShouldBeImmutable()
    {
        // Arrange
        var result1 = Result<int>.Success(1)
            .WithMetadata("key1", "value1");

        // Act
        var result2 = result1.WithMetadata("key2", "value2");

        // Assert
        Assert.Single(result1.Metadata); // Original unchanged
        Assert.Equal(2, result2.Metadata.Count); // New result has both
    }

    [Fact]
    public void CompleteObservabilityScenario_ShouldWorkEndToEnd()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var traceId = Guid.NewGuid().ToString();

        // Act
        var result = ObservabilityExtensions.Timed(() =>
        {
            Thread.Sleep(20);
            var data = "important-data";
            return Result<string>.Success(data);
        })
        .WithCorrelationId(correlationId)
        .WithTraceId(traceId)
        .WithMetadata("service", "user-service")
        .WithMetadata("operation", "GetUserById")
        .WithMetadata("userId", 12345);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("important-data", result.Value);
        Assert.Equal(correlationId, result.CorrelationId);
        Assert.Equal(traceId, result.TraceId);
        Assert.NotNull(result.ExecutionTime);
        Assert.Equal("user-service", result.Metadata["service"]);
        Assert.Equal("GetUserById", result.Metadata["operation"]);
        Assert.Equal(12345, result.Metadata["userId"]);
    }
}
