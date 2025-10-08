namespace eQuantic.Core.Outcomes.Extensions;

/// <summary>
/// Extension methods for adding observability metadata to results, supporting distributed tracing,
/// APM (Application Performance Monitoring), and microservices monitoring scenarios.
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Adds a correlation ID to the result for distributed tracing across microservices.
    /// Correlation IDs enable tracking a single business transaction across multiple services,
    /// making it easier to trace requests through complex distributed systems and identify bottlenecks.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result instance to which the correlation ID will be added.</param>
    /// <param name="correlationId">The unique correlation identifier that tracks this operation across service boundaries.
    /// Typically propagated from incoming HTTP headers (e.g., X-Correlation-ID) or generated at the entry point.</param>
    /// <returns>A new result instance with the correlation ID attached, preserving all other result properties.</returns>
    /// <example>
    /// <code>
    /// // In a microservices architecture, propagate correlation ID across services
    /// public async Task&lt;Result&lt;Order&gt;&gt; ProcessOrderAsync(string orderId, string correlationId)
    /// {
    ///     var result = await orderService.GetOrderAsync(orderId);
    ///
    ///     // Attach correlation ID for distributed tracing
    ///     result = result.WithCorrelationId(correlationId);
    ///
    ///     // Log with correlation ID for centralized log aggregation (ELK, Splunk, etc.)
    ///     logger.LogInformation("Order {OrderId} processed. CorrelationId: {CorrelationId}",
    ///         orderId, result.CorrelationId);
    ///
    ///     return result;
    /// }
    ///
    /// // In API Gateway or entry point service
    /// var correlationId = HttpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
    ///     ?? Guid.NewGuid().ToString();
    /// var result = await orderService.ProcessOrderAsync(orderId, correlationId);
    /// </code>
    /// </example>
    public static Result<T> WithCorrelationId<T>(this Result<T> result, string correlationId)
    {
        return result with { CorrelationId = correlationId };
    }

    /// <summary>
    /// Adds a correlation ID to the result for distributed tracing across microservices.
    /// Correlation IDs enable tracking a single business transaction across multiple services,
    /// making it easier to trace requests through complex distributed systems and identify bottlenecks.
    /// </summary>
    /// <param name="result">The result instance to which the correlation ID will be added.</param>
    /// <param name="correlationId">The unique correlation identifier that tracks this operation across service boundaries.
    /// Typically propagated from incoming HTTP headers (e.g., X-Correlation-ID) or generated at the entry point.</param>
    /// <returns>A new result instance with the correlation ID attached, preserving all other result properties.</returns>
    /// <example>
    /// <code>
    /// // In a validation or authentication service
    /// public Result ValidateCredentials(string username, string password, string correlationId)
    /// {
    ///     var result = authenticationService.Authenticate(username, password);
    ///
    ///     // Attach correlation ID for end-to-end tracing
    ///     result = result.WithCorrelationId(correlationId);
    ///
    ///     // APM tools can correlate this with frontend requests
    ///     telemetryClient.TrackEvent("Authentication", new Dictionary&lt;string, string&gt;
    ///     {
    ///         ["CorrelationId"] = result.CorrelationId,
    ///         ["Success"] = result.IsSuccess.ToString()
    ///     });
    ///
    ///     return result;
    /// }
    /// </code>
    /// </example>
    public static Result WithCorrelationId(this Result result, string correlationId)
    {
        return result with { CorrelationId = correlationId };
    }

    /// <summary>
    /// Adds a trace ID to the result for integration with distributed tracing systems (OpenTelemetry, Jaeger, Zipkin).
    /// Trace IDs represent the unique identifier for an entire distributed trace, enabling you to visualize
    /// the complete request flow across all services and identify performance bottlenecks in your microservices architecture.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result instance to which the trace ID will be added.</param>
    /// <param name="traceId">The distributed trace identifier, typically obtained from the current Activity.TraceId
    /// or from tracing context propagated via W3C Trace Context headers (traceparent, tracestate).</param>
    /// <returns>A new result instance with the trace ID attached, enabling trace correlation in APM systems.</returns>
    /// <example>
    /// <code>
    /// // Integration with OpenTelemetry for distributed tracing
    /// using System.Diagnostics;
    ///
    /// public async Task&lt;Result&lt;Payment&gt;&gt; ProcessPaymentAsync(decimal amount)
    /// {
    ///     // Get the current trace ID from Activity (OpenTelemetry/System.Diagnostics)
    ///     var traceId = Activity.Current?.TraceId.ToString();
    ///
    ///     var result = await paymentGateway.ChargeAsync(amount);
    ///
    ///     // Attach trace ID for correlation in APM dashboards (Datadog, New Relic, Application Insights)
    ///     if (traceId != null)
    ///     {
    ///         result = result.WithTraceId(traceId);
    ///     }
    ///
    ///     // This allows APM tools to correlate logs, metrics, and traces
    ///     logger.LogInformation("Payment processed. TraceId: {TraceId}, Amount: {Amount}",
    ///         result.TraceId, amount);
    ///
    ///     return result;
    /// }
    ///
    /// // The trace ID can be visualized in tools like Jaeger to see the complete request flow:
    /// // API Gateway -> Order Service -> Payment Service -> Bank API
    /// </code>
    /// </example>
    public static Result<T> WithTraceId<T>(this Result<T> result, string traceId)
    {
        return result with { TraceId = traceId };
    }

    /// <summary>
    /// Adds a trace ID to the result for integration with distributed tracing systems (OpenTelemetry, Jaeger, Zipkin).
    /// Trace IDs represent the unique identifier for an entire distributed trace, enabling you to visualize
    /// the complete request flow across all services and identify performance bottlenecks in your microservices architecture.
    /// </summary>
    /// <param name="result">The result instance to which the trace ID will be added.</param>
    /// <param name="traceId">The distributed trace identifier, typically obtained from the current Activity.TraceId
    /// or from tracing context propagated via W3C Trace Context headers (traceparent, tracestate).</param>
    /// <returns>A new result instance with the trace ID attached, enabling trace correlation in APM systems.</returns>
    /// <example>
    /// <code>
    /// // Using trace IDs in a health check or diagnostic endpoint
    /// using System.Diagnostics;
    ///
    /// public Result PerformHealthCheck()
    /// {
    ///     var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
    ///     var result = databaseService.CheckConnection();
    ///
    ///     // Attach trace ID for troubleshooting in production
    ///     result = result.WithTraceId(traceId);
    ///
    ///     // Export to APM for monitoring system health
    ///     metrics.RecordHealthCheck(result.IsSuccess, result.TraceId);
    ///
    ///     return result;
    /// }
    /// </code>
    /// </example>
    public static Result WithTraceId(this Result result, string traceId)
    {
        return result with { TraceId = traceId };
    }

    /// <summary>
    /// Adds execution time metadata to the result for performance monitoring and SLA tracking.
    /// This is essential for Application Performance Monitoring (APM), identifying slow operations,
    /// tracking service-level agreements (SLAs), and optimizing critical paths in your application.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result instance to which the execution time will be added.</param>
    /// <param name="executionTime">The duration of the operation execution. Typically measured using a Stopwatch
    /// or calculated as the difference between DateTime.UtcNow timestamps before and after the operation.</param>
    /// <returns>A new result instance with the execution time attached, enabling performance analysis in monitoring systems.</returns>
    /// <example>
    /// <code>
    /// // Manual performance tracking for critical operations
    /// public async Task&lt;Result&lt;Report&gt;&gt; GenerateMonthlyReportAsync()
    /// {
    ///     var startTime = DateTime.UtcNow;
    ///
    ///     var result = await reportService.GenerateAsync();
    ///     var executionTime = DateTime.UtcNow - startTime;
    ///
    ///     // Attach execution time for APM and performance dashboards
    ///     result = result.WithExecutionTime(executionTime);
    ///
    ///     // Alert if operation exceeds SLA threshold (e.g., 5 seconds)
    ///     if (result.ExecutionTime > TimeSpan.FromSeconds(5))
    ///     {
    ///         logger.LogWarning("Report generation exceeded SLA: {ExecutionTime}ms",
    ///             result.ExecutionTime?.TotalMilliseconds);
    ///         alertingService.SendAlert($"Slow report generation: {executionTime.TotalSeconds}s");
    ///     }
    ///
    ///     // Export metrics to monitoring systems (Prometheus, CloudWatch, etc.)
    ///     metrics.RecordHistogram("report_generation_duration_seconds",
    ///         result.ExecutionTime?.TotalSeconds ?? 0);
    ///
    ///     return result;
    /// }
    /// </code>
    /// </example>
    public static Result<T> WithExecutionTime<T>(this Result<T> result, TimeSpan executionTime)
    {
        return result with { ExecutionTime = executionTime };
    }

    /// <summary>
    /// Adds execution time metadata to the result for performance monitoring and SLA tracking.
    /// This is essential for Application Performance Monitoring (APM), identifying slow operations,
    /// tracking service-level agreements (SLAs), and optimizing critical paths in your application.
    /// </summary>
    /// <param name="result">The result instance to which the execution time will be added.</param>
    /// <param name="executionTime">The duration of the operation execution. Typically measured using a Stopwatch
    /// or calculated as the difference between DateTime.UtcNow timestamps before and after the operation.</param>
    /// <returns>A new result instance with the execution time attached, enabling performance analysis in monitoring systems.</returns>
    /// <example>
    /// <code>
    /// // Performance monitoring for validation operations
    /// public Result ValidateBusinessRules(Order order)
    /// {
    ///     var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    ///
    ///     var result = businessRuleValidator.Validate(order);
    ///     stopwatch.Stop();
    ///
    ///     // Track validation performance
    ///     result = result.WithExecutionTime(stopwatch.Elapsed);
    ///
    ///     // Monitor for performance degradation over time
    ///     telemetry.TrackMetric("validation_duration_ms", stopwatch.Elapsed.TotalMilliseconds);
    ///
    ///     return result;
    /// }
    /// </code>
    /// </example>
    public static Result WithExecutionTime(this Result result, TimeSpan executionTime)
    {
        return result with { ExecutionTime = executionTime };
    }

    /// <summary>
    /// Adds a custom metadata entry to the result for enriched observability and contextual information.
    /// Metadata enables capturing additional context about operations that can be used for debugging,
    /// analytics, business intelligence, and integration with observability platforms.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result instance to which the metadata will be added.</param>
    /// <param name="key">The metadata key identifier. Use descriptive names that follow your team's naming conventions
    /// (e.g., "service.name", "user.id", "request.method", "db.query.rows_affected").</param>
    /// <param name="value">The metadata value. Can be any serializable object (string, number, boolean, complex objects).
    /// Consider using primitive types for better compatibility with logging and APM systems.</param>
    /// <returns>A new result instance with the metadata entry added, preserving all existing metadata and properties.</returns>
    /// <example>
    /// <code>
    /// // Enriching results with operational metadata for observability
    /// public async Task&lt;Result&lt;Customer&gt;&gt; GetCustomerAsync(string customerId)
    /// {
    ///     var result = await customerRepository.GetByIdAsync(customerId);
    ///
    ///     // Add contextual metadata for distributed tracing and analytics
    ///     result = result
    ///         .WithMetadata("service.name", "CustomerService")
    ///         .WithMetadata("service.version", "2.3.1")
    ///         .WithMetadata("cache.hit", false)
    ///         .WithMetadata("db.query.duration_ms", 45)
    ///         .WithMetadata("environment", "production");
    ///
    ///     // APM tools can use this metadata for filtering and dashboards
    ///     logger.LogInformation("Customer retrieved. Metadata: {@Metadata}", result.Metadata);
    ///
    ///     // Export to OpenTelemetry as span attributes
    ///     Activity.Current?.SetTag("customer.id", customerId);
    ///     foreach (var meta in result.Metadata)
    ///     {
    ///         Activity.Current?.SetTag(meta.Key, meta.Value);
    ///     }
    ///
    ///     return result;
    /// }
    ///
    /// // Business metrics and feature flags
    /// result = result
    ///     .WithMetadata("feature.premium_enabled", true)
    ///     .WithMetadata("business.revenue_impact", 15000.50m)
    ///     .WithMetadata("user.segment", "enterprise");
    /// </code>
    /// </example>
    public static Result<T> WithMetadata<T>(this Result<T> result, string key, object value)
    {
        var newMetadata = new Dictionary<string, object>(result.Metadata)
        {
            [key] = value
        };

        // Create a new result by copying all properties and updating metadata
        if (result.IsSuccess)
        {
            var newResult = Result<T>.Success(result.Value);
            return CopyObservabilityData(result, newResult, newMetadata);
        }
        else
        {
            var newResult = Result<T>.Failure(result.Errors);
            return CopyObservabilityData(result, newResult, newMetadata);
        }
    }

    /// <summary>
    /// Adds a custom metadata entry to the result for enriched observability and contextual information.
    /// Metadata enables capturing additional context about operations that can be used for debugging,
    /// analytics, business intelligence, and integration with observability platforms.
    /// </summary>
    /// <param name="result">The result instance to which the metadata will be added.</param>
    /// <param name="key">The metadata key identifier. Use descriptive names that follow your team's naming conventions
    /// (e.g., "service.name", "user.id", "request.method", "db.query.rows_affected").</param>
    /// <param name="value">The metadata value. Can be any serializable object (string, number, boolean, complex objects).
    /// Consider using primitive types for better compatibility with logging and APM systems.</param>
    /// <returns>A new result instance with the metadata entry added, preserving all existing metadata and properties.</returns>
    /// <example>
    /// <code>
    /// // Adding operational context for monitoring and troubleshooting
    /// public Result ProcessBatchJob(int batchId)
    /// {
    ///     var result = batchProcessor.Process(batchId);
    ///
    ///     // Enrich with metadata for ops dashboards and alerting
    ///     result = result
    ///         .WithMetadata("batch.id", batchId)
    ///         .WithMetadata("batch.size", 1000)
    ///         .WithMetadata("batch.type", "daily_reconciliation")
    ///         .WithMetadata("worker.id", Environment.MachineName);
    ///
    ///     return result;
    /// }
    /// </code>
    /// </example>
    public static Result WithMetadata(this Result result, string key, object value)
    {
        var newMetadata = new Dictionary<string, object>(result.Metadata)
        {
            [key] = value
        };

        // Create a new result by copying all properties and updating metadata
        if (result.IsSuccess)
        {
            var newResult = Result.Success();
            return CopyObservabilityData(result, newResult, newMetadata);
        }
        else
        {
            var newResult = Result.Failure(result.Errors);
            return CopyObservabilityData(result, newResult, newMetadata);
        }
    }

    /// <summary>
    /// Adds multiple metadata entries to the result for comprehensive observability enrichment.
    /// This overload is ideal for bulk metadata attachment, such as propagating context from HTTP headers,
    /// adding OpenTelemetry baggage, or attaching standardized observability tags across your microservices.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result instance to which the metadata entries will be added.</param>
    /// <param name="metadata">A dictionary of metadata entries to add. Keys should follow consistent naming conventions
    /// (e.g., OpenTelemetry semantic conventions: "http.method", "db.system", "messaging.destination").
    /// Values can be any serializable objects.</param>
    /// <returns>A new result instance with all metadata entries added, merging with any existing metadata.</returns>
    /// <example>
    /// <code>
    /// // Propagating observability context in microservices
    /// public async Task&lt;Result&lt;Order&gt;&gt; CreateOrderAsync(OrderRequest request, HttpContext httpContext)
    /// {
    ///     // Extract observability metadata from incoming request
    ///     var observabilityContext = new Dictionary&lt;string, object&gt;
    ///     {
    ///         ["http.method"] = httpContext.Request.Method,
    ///         ["http.url"] = httpContext.Request.Path,
    ///         ["http.user_agent"] = httpContext.Request.Headers["User-Agent"].ToString(),
    ///         ["client.ip"] = httpContext.Connection.RemoteIpAddress?.ToString(),
    ///         ["service.name"] = "OrderService",
    ///         ["service.namespace"] = "ecommerce",
    ///         ["deployment.environment"] = "production",
    ///         ["cloud.region"] = "us-east-1",
    ///         ["cloud.provider"] = "aws"
    ///     };
    ///
    ///     var result = await orderRepository.CreateAsync(request);
    ///
    ///     // Bulk attach observability metadata
    ///     result = result.WithMetadata(observabilityContext);
    ///
    ///     // This metadata is now available for:
    ///     // - Distributed tracing (Jaeger, Zipkin, X-Ray)
    ///     // - APM dashboards (Datadog, New Relic, Dynatrace)
    ///     // - Log aggregation (ELK, Splunk, CloudWatch)
    ///     // - Metrics and analytics (Prometheus, Grafana)
    ///
    ///     return result;
    /// }
    ///
    /// // Using OpenTelemetry baggage for context propagation
    /// var baggageMetadata = Baggage.Current.Select(kvp =&gt;
    ///     new KeyValuePair&lt;string, object&gt;(kvp.Key, kvp.Value)).ToDictionary(k =&gt; k.Key, v =&gt; v.Value);
    /// result = result.WithMetadata(baggageMetadata);
    /// </code>
    /// </example>
    public static Result<T> WithMetadata<T>(this Result<T> result, IReadOnlyDictionary<string, object> metadata)
    {
        var newMetadata = new Dictionary<string, object>(result.Metadata);
        foreach (var kvp in metadata)
        {
            newMetadata[kvp.Key] = kvp.Value;
        }

        // Create a new result by copying all properties and updating metadata
        if (result.IsSuccess)
        {
            var newResult = Result<T>.Success(result.Value);
            return CopyObservabilityData(result, newResult, newMetadata);
        }
        else
        {
            var newResult = Result<T>.Failure(result.Errors);
            return CopyObservabilityData(result, newResult, newMetadata);
        }
    }

    /// <summary>
    /// Adds multiple metadata entries to the result for comprehensive observability enrichment.
    /// This overload is ideal for bulk metadata attachment, such as propagating context from HTTP headers,
    /// adding OpenTelemetry baggage, or attaching standardized observability tags across your microservices.
    /// </summary>
    /// <param name="result">The result instance to which the metadata entries will be added.</param>
    /// <param name="metadata">A dictionary of metadata entries to add. Keys should follow consistent naming conventions
    /// (e.g., OpenTelemetry semantic conventions: "http.method", "db.system", "messaging.destination").
    /// Values can be any serializable objects.</param>
    /// <returns>A new result instance with all metadata entries added, merging with any existing metadata.</returns>
    /// <example>
    /// <code>
    /// // Standard observability tags for consistent monitoring
    /// public Result ExecuteCommand(Command cmd)
    /// {
    ///     var standardTags = new Dictionary&lt;string, object&gt;
    ///     {
    ///         ["service.name"] = "CommandProcessor",
    ///         ["service.version"] = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
    ///         ["host.name"] = Environment.MachineName,
    ///         ["process.id"] = Environment.ProcessId,
    ///         ["thread.id"] = Thread.CurrentThread.ManagedThreadId
    ///     };
    ///
    ///     var result = commandExecutor.Execute(cmd);
    ///     return result.WithMetadata(standardTags);
    /// }
    /// </code>
    /// </example>
    public static Result WithMetadata(this Result result, IReadOnlyDictionary<string, object> metadata)
    {
        var newMetadata = new Dictionary<string, object>(result.Metadata);
        foreach (var kvp in metadata)
        {
            newMetadata[kvp.Key] = kvp.Value;
        }

        // Create a new result by copying all properties and updating metadata
        if (result.IsSuccess)
        {
            var newResult = Result.Success();
            return CopyObservabilityData(result, newResult, newMetadata);
        }
        else
        {
            var newResult = Result.Failure(result.Errors);
            return CopyObservabilityData(result, newResult, newMetadata);
        }
    }

    // Helper method to copy observability data to a new Result<T> instance
    private static Result<T> CopyObservabilityData<T>(
        Result<T> source,
        Result<T> target,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        // Use internal constructor to create a new result with all observability data
        return new Result<T>(
            source.IsSuccess ? source.Value : default,
            target.Errors,
            target.IsSuccess,
            metadata ?? source.Metadata,
            source.CorrelationId,
            source.TraceId,
            source.ExecutionTime);
    }

    // Helper method to copy observability data to a new Result instance
    private static Result CopyObservabilityData(
        Result source,
        Result target,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        // Use internal constructor to create a new result with all observability data
        return new Result(
            target.Errors,
            target.IsSuccess,
            metadata ?? source.Metadata,
            source.CorrelationId,
            source.TraceId,
            source.ExecutionTime);
    }

    /// <summary>
    /// Automatically measures and attaches execution time to a synchronous operation's result.
    /// This is a convenience method for APM that wraps an operation with timing instrumentation,
    /// eliminating manual timestamp tracking and ensuring consistent performance measurement across your codebase.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result returned by the operation.</typeparam>
    /// <param name="operation">The synchronous operation to execute and measure. This function should return a Result&lt;T&gt;
    /// and will be automatically timed from invocation to completion.</param>
    /// <returns>The result from the operation with ExecutionTime automatically attached, ready for performance analysis.</returns>
    /// <example>
    /// <code>
    /// // Simplified performance tracking without manual instrumentation
    /// public Result&lt;Invoice&gt; GenerateInvoice(Order order)
    /// {
    ///     // Timed automatically wraps the operation and measures execution time
    ///     return ObservabilityExtensions.Timed(() =&gt;
    ///     {
    ///         var invoice = invoiceGenerator.Generate(order);
    ///         return Result&lt;Invoice&gt;.Success(invoice);
    ///     });
    /// }
    ///
    /// // The result automatically includes execution time for APM
    /// var result = GenerateInvoice(order);
    /// logger.LogInformation("Invoice generated in {Duration}ms", result.ExecutionTime?.TotalMilliseconds);
    ///
    /// // Chaining with other observability features
    /// var correlationId = Guid.NewGuid().ToString();
    /// var result = ObservabilityExtensions.Timed(() =&gt; ProcessOrder(orderId))
    ///     .WithCorrelationId(correlationId)
    ///     .WithMetadata("order.id", orderId)
    ///     .WithMetadata("customer.tier", "premium");
    ///
    /// // Export to monitoring systems
    /// if (result.ExecutionTime.HasValue)
    /// {
    ///     metricsCollector.RecordDuration("order_processing_seconds",
    ///         result.ExecutionTime.Value.TotalSeconds,
    ///         new[] { new KeyValuePair&lt;string, object&gt;("status", result.IsSuccess ? "success" : "failure") });
    /// }
    /// </code>
    /// </example>
    public static Result<T> Timed<T>(Func<Result<T>> operation)
    {
        var startTime = DateTime.UtcNow;
        var result = operation();
        var executionTime = DateTime.UtcNow - startTime;

        return result.WithExecutionTime(executionTime);
    }

    /// <summary>
    /// Automatically measures and attaches execution time to a synchronous operation's result.
    /// This is a convenience method for APM that wraps an operation with timing instrumentation,
    /// eliminating manual timestamp tracking and ensuring consistent performance measurement across your codebase.
    /// </summary>
    /// <param name="operation">The synchronous operation to execute and measure. This function should return a Result
    /// and will be automatically timed from invocation to completion.</param>
    /// <returns>The result from the operation with ExecutionTime automatically attached, ready for performance analysis.</returns>
    /// <example>
    /// <code>
    /// // Measuring operations without return values
    /// public Result SendNotification(string userId, string message)
    /// {
    ///     return ObservabilityExtensions.Timed(() =&gt;
    ///     {
    ///         notificationService.Send(userId, message);
    ///         return Result.Success();
    ///     });
    /// }
    ///
    /// // Monitor SLA compliance
    /// var result = SendNotification(userId, message);
    /// if (result.ExecutionTime &gt; TimeSpan.FromSeconds(2))
    /// {
    ///     // Alert on SLA violation
    ///     slaMonitor.RecordViolation("notification_send", result.ExecutionTime.Value);
    /// }
    /// </code>
    /// </example>
    public static Result Timed(Func<Result> operation)
    {
        var startTime = DateTime.UtcNow;
        var result = operation();
        var executionTime = DateTime.UtcNow - startTime;

        return result.WithExecutionTime(executionTime);
    }

    /// <summary>
    /// Automatically measures and attaches execution time to an asynchronous operation's result.
    /// This is essential for monitoring async operations in microservices, measuring API response times,
    /// tracking database query performance, and identifying slow external service calls in distributed systems.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result returned by the async operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute and measure. This function should return a Task&lt;Result&lt;T&gt;&gt;
    /// and will be automatically timed from invocation to await completion.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with ExecutionTime automatically attached.</returns>
    /// <example>
    /// <code>
    /// // Monitoring async API calls in microservices
    /// public async Task&lt;Result&lt;Product&gt;&gt; GetProductFromExternalServiceAsync(string productId)
    /// {
    ///     return await ObservabilityExtensions.TimedAsync(async () =&gt;
    ///     {
    ///         var response = await httpClient.GetAsync($"https://api.supplier.com/products/{productId}");
    ///         response.EnsureSuccessStatusCode();
    ///         var product = await response.Content.ReadFromJsonAsync&lt;Product&gt;();
    ///         return Result&lt;Product&gt;.Success(product);
    ///     });
    /// }
    ///
    /// // Track performance with full observability context
    /// var traceId = Activity.Current?.TraceId.ToString();
    /// var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    ///
    /// var result = await ObservabilityExtensions.TimedAsync(async () =&gt;
    ///     await productService.GetProductAsync(productId))
    ///     .WithTraceId(traceId)
    ///     .WithCorrelationId(correlationId)
    ///     .WithMetadata("service.name", "ProductCatalog")
    ///     .WithMetadata("external.api", "supplier-api");
    ///
    /// // Monitor for performance degradation and circuit breaker patterns
    /// if (result.ExecutionTime &gt; TimeSpan.FromSeconds(3))
    /// {
    ///     logger.LogWarning(
    ///         "Slow external API call detected. TraceId: {TraceId}, Duration: {Duration}ms, ProductId: {ProductId}",
    ///         result.TraceId, result.ExecutionTime?.TotalMilliseconds, productId);
    ///
    ///     // Trigger circuit breaker or rate limiting
    ///     circuitBreaker.RecordFailure("supplier-api");
    /// }
    ///
    /// // Export to APM for dashboard visualization
    /// telemetry.TrackDependency(
    ///     "HTTP",
    ///     "supplier-api",
    ///     "GetProduct",
    ///     result.ExecutionTime ?? TimeSpan.Zero,
    ///     result.IsSuccess);
    /// </code>
    /// </example>
    public static async Task<Result<T>> TimedAsync<T>(Func<Task<Result<T>>> operation)
    {
        var startTime = DateTime.UtcNow;
        var result = await operation();
        var executionTime = DateTime.UtcNow - startTime;

        return result.WithExecutionTime(executionTime);
    }

    /// <summary>
    /// Automatically measures and attaches execution time to an asynchronous operation's result.
    /// This is essential for monitoring async operations in microservices, measuring API response times,
    /// tracking database query performance, and identifying slow external service calls in distributed systems.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute and measure. This function should return a Task&lt;Result&gt;
    /// and will be automatically timed from invocation to await completion.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with ExecutionTime automatically attached.</returns>
    /// <example>
    /// <code>
    /// // Monitoring async database operations
    /// public async Task&lt;Result&gt; UpdateCustomerStatusAsync(string customerId, string status)
    /// {
    ///     return await ObservabilityExtensions.TimedAsync(async () =&gt;
    ///     {
    ///         await database.ExecuteAsync(
    ///             "UPDATE Customers SET Status = @Status WHERE Id = @Id",
    ///             new { Status = status, Id = customerId });
    ///         return Result.Success();
    ///     });
    /// }
    ///
    /// // Full observability for background jobs and message processing
    /// public async Task ProcessMessageAsync(Message message)
    /// {
    ///     var result = await ObservabilityExtensions.TimedAsync(async () =&gt;
    ///         await messageHandler.HandleAsync(message))
    ///         .WithCorrelationId(message.CorrelationId)
    ///         .WithMetadata("message.type", message.Type)
    ///         .WithMetadata("queue.name", "order-processing")
    ///         .WithMetadata("retry.count", message.RetryCount);
    ///
    ///     // Track message processing metrics
    ///     metricsPublisher.PublishHistogram(
    ///         "message_processing_duration_seconds",
    ///         result.ExecutionTime?.TotalSeconds ?? 0,
    ///         new Dictionary&lt;string, string&gt;
    ///         {
    ///             ["message_type"] = message.Type,
    ///             ["status"] = result.IsSuccess ? "success" : "failure"
    ///         });
    ///
    ///     // Log with structured data for log aggregation systems
    ///     logger.LogInformation(
    ///         "Message processed. CorrelationId: {CorrelationId}, Type: {Type}, Duration: {Duration}ms, Success: {Success}",
    ///         result.CorrelationId, message.Type, result.ExecutionTime?.TotalMilliseconds, result.IsSuccess);
    /// }
    /// </code>
    /// </example>
    public static async Task<Result> TimedAsync(Func<Task<Result>> operation)
    {
        var startTime = DateTime.UtcNow;
        var result = await operation();
        var executionTime = DateTime.UtcNow - startTime;

        return result.WithExecutionTime(executionTime);
    }
}
