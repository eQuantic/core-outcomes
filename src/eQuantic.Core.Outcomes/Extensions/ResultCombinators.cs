using eQuantic.Core.Outcomes.Errors;

namespace eQuantic.Core.Outcomes.Extensions;

/// <summary>
/// Extension methods for combining and aggregating multiple Result instances.
/// </summary>
public static class ResultCombinators
{
    #region Combine

    /// <summary>
    /// Combines multiple results into a single result containing a collection using an "all-or-nothing" aggregation pattern.
    /// This combinator is ideal for scenarios where you need all operations to succeed before proceeding,
    /// such as batch validation, parallel data fetching, or multi-step workflows where partial success is not acceptable.
    /// </summary>
    /// <typeparam name="T">The type of the values contained in the results.</typeparam>
    /// <param name="results">The array of results to combine. Cannot be null.</param>
    /// <returns>
    /// A Result containing an IEnumerable of all values if all input results are successful,
    /// or a failure Result with all accumulated errors if any input result fails.
    /// Returns an empty successful collection if no results are provided.
    /// </returns>
    /// <remarks>
    /// Success Condition: ALL results must be successful.
    /// Failure Condition: ANY result fails (accumulates all errors from failed results).
    /// </remarks>
    /// <example>
    /// Validating multiple user inputs:
    /// <code>
    /// var nameResult = ValidateName(user.Name);
    /// var emailResult = ValidateEmail(user.Email);
    /// var ageResult = ValidateAge(user.Age);
    ///
    /// var combinedResult = ResultCombinators.Combine(nameResult, emailResult, ageResult);
    ///
    /// if (combinedResult.IsSuccess)
    /// {
    ///     // All validations passed - proceed with user creation
    ///     var validatedInputs = combinedResult.Value.ToArray();
    ///     CreateUser(validatedInputs[0], validatedInputs[1], validatedInputs[2]);
    /// }
    /// else
    /// {
    ///     // Return all validation errors to the user
    ///     return BadRequest(combinedResult.Errors);
    /// }
    /// </code>
    ///
    /// Parallel data fetching where all data is required:
    /// <code>
    /// var userResult = await userService.GetUserAsync(userId);
    /// var ordersResult = await orderService.GetOrdersAsync(userId);
    /// var preferencesResult = await preferenceService.GetPreferencesAsync(userId);
    ///
    /// var combinedData = ResultCombinators.Combine(userResult, ordersResult, preferencesResult);
    ///
    /// return combinedData.IsSuccess
    ///     ? Ok(new { User = combinedData.Value.ElementAt(0),
    ///                Orders = combinedData.Value.ElementAt(1),
    ///                Preferences = combinedData.Value.ElementAt(2) })
    ///     : StatusCode(500, combinedData.Errors);
    /// </code>
    /// </example>
    public static Result<IEnumerable<T>> Combine<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (results.Length == 0)
        {
            return Result<IEnumerable<T>>.Success(Enumerable.Empty<T>());
        }

        var errors = new List<IError>();
        var values = new List<T>();

        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                values.Add(result.Value);
            }
            else
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Any()
            ? Result<IEnumerable<T>>.Failure(errors)
            : Result<IEnumerable<T>>.Success(values);
    }

    /// <summary>
    /// Combines multiple results from a collection into a single result using an "all-or-nothing" aggregation pattern.
    /// This is an extension method variant of Combine that operates on IEnumerable, making it chainable with LINQ operations.
    /// Use this when processing results from enumerable sources like lists, queries, or LINQ projections.
    /// </summary>
    /// <typeparam name="T">The type of the values contained in the results.</typeparam>
    /// <param name="results">The enumerable collection of results to combine. Cannot be null.</param>
    /// <returns>
    /// A Result containing an IEnumerable of all values if all input results are successful,
    /// or a failure Result with all accumulated errors if any input result fails.
    /// </returns>
    /// <remarks>
    /// Success Condition: ALL results in the collection must be successful.
    /// Failure Condition: ANY result fails (accumulates all errors from failed results).
    /// </remarks>
    /// <example>
    /// Processing a list of validation operations with LINQ:
    /// <code>
    /// var items = new[] { "item1", "item2", "item3" };
    ///
    /// var validationResults = items
    ///     .Select(item => ValidateItem(item))
    ///     .Combine();
    ///
    /// if (validationResults.IsSuccess)
    /// {
    ///     // All items validated successfully
    ///     ProcessItems(validationResults.Value);
    /// }
    /// else
    /// {
    ///     // Handle validation errors
    ///     LogErrors(validationResults.Errors);
    /// }
    /// </code>
    ///
    /// Batch processing with result aggregation:
    /// <code>
    /// var userIds = new[] { 1, 2, 3, 4, 5 };
    ///
    /// var updateResults = await userIds
    ///     .Select(id => userService.UpdateUserAsync(id))
    ///     .ToAsyncEnumerable()
    ///     .ToEnumerable()
    ///     .Combine();
    ///
    /// return updateResults.IsSuccess
    ///     ? Ok($"Successfully updated {updateResults.Value.Count()} users")
    ///     : BadRequest(updateResults.Errors);
    /// </code>
    /// </example>
    public static Result<IEnumerable<T>> Combine<T>(this IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return Combine(results.ToArray());
    }

    /// <summary>
    /// Combines multiple non-generic results into a single result using an "all-or-nothing" aggregation pattern.
    /// This overload is designed for operations that don't return values but only indicate success or failure,
    /// such as save operations, delete operations, or side-effect producing actions.
    /// Use this when you need to ensure multiple operations all complete successfully before proceeding.
    /// </summary>
    /// <param name="results">The array of non-generic results to combine. Cannot be null.</param>
    /// <returns>
    /// A successful Result if all input results are successful,
    /// or a failure Result with all accumulated errors if any input result fails.
    /// Returns success if no results are provided.
    /// </returns>
    /// <remarks>
    /// Success Condition: ALL results must be successful.
    /// Failure Condition: ANY result fails (accumulates all errors from failed results).
    /// </remarks>
    /// <example>
    /// Combining multiple write operations:
    /// <code>
    /// var saveUserResult = userRepository.SaveAsync(user);
    /// var saveAuditResult = auditRepository.LogActionAsync(action);
    /// var sendEmailResult = emailService.SendNotificationAsync(user.Email);
    ///
    /// var combinedResult = ResultCombinators.Combine(saveUserResult, saveAuditResult, sendEmailResult);
    ///
    /// if (combinedResult.IsSuccess)
    /// {
    ///     await transaction.CommitAsync();
    ///     return Ok("All operations completed successfully");
    /// }
    /// else
    /// {
    ///     await transaction.RollbackAsync();
    ///     return StatusCode(500, combinedResult.Errors);
    /// }
    /// </code>
    ///
    /// Validating preconditions before executing an action:
    /// <code>
    /// var hasPermissionResult = authService.CheckPermission(userId, "write");
    /// var isValidStateResult = ValidateState(entity);
    /// var rateLimitResult = rateLimiter.CheckLimit(userId);
    ///
    /// var preconditionsResult = ResultCombinators.Combine(
    ///     hasPermissionResult,
    ///     isValidStateResult,
    ///     rateLimitResult);
    ///
    /// if (preconditionsResult.IsSuccess)
    /// {
    ///     // All preconditions met - proceed with operation
    ///     return await ExecuteOperation();
    /// }
    /// else
    /// {
    ///     // Return all precondition failures
    ///     return Forbidden(preconditionsResult.Errors);
    /// }
    /// </code>
    /// </example>
    public static Result Combine(params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (results.Length == 0)
        {
            return Result.Success();
        }

        var errors = new List<IError>();

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Any()
            ? Result.Failure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Combines multiple non-generic results from a collection into a single result using an "all-or-nothing" aggregation pattern.
    /// This is an extension method variant that operates on IEnumerable of non-generic Results, making it chainable with LINQ.
    /// Use this when processing results from enumerable sources of operations that don't return values.
    /// </summary>
    /// <param name="results">The enumerable collection of non-generic results to combine. Cannot be null.</param>
    /// <returns>
    /// A successful Result if all input results are successful,
    /// or a failure Result with all accumulated errors if any input result fails.
    /// </returns>
    /// <remarks>
    /// Success Condition: ALL results in the collection must be successful.
    /// Failure Condition: ANY result fails (accumulates all errors from failed results).
    /// </remarks>
    /// <example>
    /// Batch deletion with error aggregation:
    /// <code>
    /// var itemIds = new[] { 1, 2, 3, 4, 5 };
    ///
    /// var deleteResults = itemIds
    ///     .Select(id => repository.DeleteAsync(id))
    ///     .Combine();
    ///
    /// if (deleteResults.IsSuccess)
    /// {
    ///     return Ok($"Successfully deleted {itemIds.Length} items");
    /// }
    /// else
    /// {
    ///     return BadRequest(new
    ///     {
    ///         Message = "Some deletions failed",
    ///         Errors = deleteResults.Errors
    ///     });
    /// }
    /// </code>
    /// </example>
    public static Result Combine(this IEnumerable<Result> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return Combine(results.ToArray());
    }

    #endregion

    #region Zip

    /// <summary>
    /// Combines two results into a single result containing a tuple using an "all-or-nothing" pattern with type preservation.
    /// This combinator is ideal when you need to combine results of different types into a strongly-typed tuple,
    /// such as combining independent validation results, parallel API calls with different return types,
    /// or aggregating heterogeneous data sources where both results are required.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first result.</typeparam>
    /// <typeparam name="T2">The type of the value in the second result.</typeparam>
    /// <param name="result1">The first result to combine. Cannot be null.</param>
    /// <param name="result2">The second result to combine. Cannot be null.</param>
    /// <returns>
    /// A Result containing a tuple (T1, T2) with both values if both input results are successful,
    /// or a failure Result with all accumulated errors if either input result fails.
    /// </returns>
    /// <remarks>
    /// Success Condition: BOTH results must be successful.
    /// Failure Condition: EITHER result fails (accumulates errors from all failed results).
    /// Unlike Combine which returns IEnumerable, Zip preserves the individual types in a tuple.
    /// </remarks>
    /// <example>
    /// Combining validation results of different types:
    /// <code>
    /// var userResult = ValidateUser(userDto);      // Result&lt;User&gt;
    /// var settingsResult = ValidateSettings(dto);  // Result&lt;Settings&gt;
    ///
    /// var combinedResult = userResult.Zip(settingsResult);
    ///
    /// if (combinedResult.IsSuccess)
    /// {
    ///     var (user, settings) = combinedResult.Value;
    ///     return await CreateAccount(user, settings);
    /// }
    /// else
    /// {
    ///     // Returns all validation errors from both validations
    ///     return BadRequest(combinedResult.Errors);
    /// }
    /// </code>
    ///
    /// Parallel API calls with different response types:
    /// <code>
    /// var profileResult = await userService.GetProfileAsync(userId);      // Result&lt;UserProfile&gt;
    /// var statisticsResult = await statsService.GetStatsAsync(userId);   // Result&lt;UserStatistics&gt;
    ///
    /// var dashboardData = profileResult.Zip(statisticsResult);
    ///
    /// return dashboardData.IsSuccess
    ///     ? Ok(new Dashboard
    ///     {
    ///         Profile = dashboardData.Value.Item1,
    ///         Statistics = dashboardData.Value.Item2
    ///     })
    ///     : StatusCode(500, dashboardData.Errors);
    /// </code>
    ///
    /// Dependency resolution where multiple components are required:
    /// <code>
    /// var configResult = configService.LoadConfiguration();     // Result&lt;Configuration&gt;
    /// var connectionResult = dbService.EstablishConnection();   // Result&lt;DbConnection&gt;
    ///
    /// var initResult = configResult.Zip(connectionResult);
    ///
    /// if (initResult.IsSuccess)
    /// {
    ///     var (config, connection) = initResult.Value;
    ///     return InitializeApplication(config, connection);
    /// }
    /// else
    /// {
    ///     LogCritical("Application initialization failed", initResult.Errors);
    ///     throw new ApplicationException("Cannot start application");
    /// }
    /// </code>
    /// </example>
    public static Result<(T1, T2)> Zip<T1, T2>(
        this Result<T1> result1,
        Result<T2> result2)
    {
        ArgumentNullException.ThrowIfNull(result1);
        ArgumentNullException.ThrowIfNull(result2);

        if (result1.IsSuccess && result2.IsSuccess)
        {
            return Result<(T1, T2)>.Success((result1.Value, result2.Value));
        }

        var errors = new List<IError>();
        if (result1.IsFailure) errors.AddRange(result1.Errors);
        if (result2.IsFailure) errors.AddRange(result2.Errors);

        return Result<(T1, T2)>.Failure(errors);
    }

    /// <summary>
    /// Combines three results into a single result containing a tuple using an "all-or-nothing" pattern with type preservation.
    /// This combinator extends Zip to handle three independent results of different types,
    /// useful for scenarios requiring three distinct validated inputs, parallel data fetching from three sources,
    /// or combining three separate operation results where all must succeed.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first result.</typeparam>
    /// <typeparam name="T2">The type of the value in the second result.</typeparam>
    /// <typeparam name="T3">The type of the value in the third result.</typeparam>
    /// <param name="result1">The first result to combine. Cannot be null.</param>
    /// <param name="result2">The second result to combine. Cannot be null.</param>
    /// <param name="result3">The third result to combine. Cannot be null.</param>
    /// <returns>
    /// A Result containing a tuple (T1, T2, T3) with all three values if all input results are successful,
    /// or a failure Result with all accumulated errors if any input result fails.
    /// </returns>
    /// <remarks>
    /// Success Condition: ALL three results must be successful.
    /// Failure Condition: ANY result fails (accumulates errors from all failed results).
    /// Preserves individual types in a strongly-typed tuple for type-safe access.
    /// </remarks>
    /// <example>
    /// Multi-source data aggregation for a complex view:
    /// <code>
    /// var userResult = await userService.GetUserAsync(id);           // Result&lt;User&gt;
    /// var ordersResult = await orderService.GetOrdersAsync(id);      // Result&lt;OrderList&gt;
    /// var addressResult = await addressService.GetAddressAsync(id);  // Result&lt;Address&gt;
    ///
    /// var pageData = userResult.Zip(ordersResult, addressResult);
    ///
    /// if (pageData.IsSuccess)
    /// {
    ///     var (user, orders, address) = pageData.Value;
    ///     return Ok(new CustomerProfileView
    ///     {
    ///         User = user,
    ///         RecentOrders = orders,
    ///         ShippingAddress = address
    ///     });
    /// }
    /// else
    /// {
    ///     return StatusCode(500, new { Errors = pageData.Errors });
    /// }
    /// </code>
    ///
    /// Complex form validation with multiple object types:
    /// <code>
    /// var personalInfoResult = ValidatePersonalInfo(dto.PersonalInfo);     // Result&lt;PersonalInfo&gt;
    /// var paymentInfoResult = ValidatePaymentInfo(dto.PaymentInfo);        // Result&lt;PaymentInfo&gt;
    /// var preferencesResult = ValidatePreferences(dto.Preferences);        // Result&lt;Preferences&gt;
    ///
    /// var validationResult = personalInfoResult.Zip(paymentInfoResult, preferencesResult);
    ///
    /// if (validationResult.IsSuccess)
    /// {
    ///     var (personal, payment, preferences) = validationResult.Value;
    ///     return await accountService.CreateAccountAsync(personal, payment, preferences);
    /// }
    /// else
    /// {
    ///     // All validation errors are collected and returned together
    ///     return BadRequest(new { ValidationErrors = validationResult.Errors });
    /// }
    /// </code>
    /// </example>
    public static Result<(T1, T2, T3)> Zip<T1, T2, T3>(
        this Result<T1> result1,
        Result<T2> result2,
        Result<T3> result3)
    {
        ArgumentNullException.ThrowIfNull(result1);
        ArgumentNullException.ThrowIfNull(result2);
        ArgumentNullException.ThrowIfNull(result3);

        if (result1.IsSuccess && result2.IsSuccess && result3.IsSuccess)
        {
            return Result<(T1, T2, T3)>.Success((result1.Value, result2.Value, result3.Value));
        }

        var errors = new List<IError>();
        if (result1.IsFailure) errors.AddRange(result1.Errors);
        if (result2.IsFailure) errors.AddRange(result2.Errors);
        if (result3.IsFailure) errors.AddRange(result3.Errors);

        return Result<(T1, T2, T3)>.Failure(errors);
    }

    /// <summary>
    /// Combines four results into a single result containing a tuple using an "all-or-nothing" pattern with type preservation.
    /// This combinator extends Zip to handle four independent results of different types,
    /// useful for complex scenarios requiring four distinct validated inputs, parallel data fetching from multiple sources,
    /// or combining four separate operation results where all must succeed for the overall operation to proceed.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first result.</typeparam>
    /// <typeparam name="T2">The type of the value in the second result.</typeparam>
    /// <typeparam name="T3">The type of the value in the third result.</typeparam>
    /// <typeparam name="T4">The type of the value in the fourth result.</typeparam>
    /// <param name="result1">The first result to combine. Cannot be null.</param>
    /// <param name="result2">The second result to combine. Cannot be null.</param>
    /// <param name="result3">The third result to combine. Cannot be null.</param>
    /// <param name="result4">The fourth result to combine. Cannot be null.</param>
    /// <returns>
    /// A Result containing a tuple (T1, T2, T3, T4) with all four values if all input results are successful,
    /// or a failure Result with all accumulated errors if any input result fails.
    /// </returns>
    /// <remarks>
    /// Success Condition: ALL four results must be successful.
    /// Failure Condition: ANY result fails (accumulates errors from all failed results).
    /// Preserves individual types in a strongly-typed tuple for type-safe access.
    /// </remarks>
    /// <example>
    /// Complex dashboard with multiple data sources:
    /// <code>
    /// var profileResult = await profileService.GetAsync(userId);         // Result&lt;UserProfile&gt;
    /// var activityResult = await activityService.GetRecentAsync(userId); // Result&lt;ActivityLog&gt;
    /// var metricsResult = await metricsService.GetMetricsAsync(userId);  // Result&lt;UserMetrics&gt;
    /// var notifResult = await notifService.GetUnreadAsync(userId);       // Result&lt;Notifications&gt;
    ///
    /// var dashboardResult = profileResult.Zip(activityResult, metricsResult, notifResult);
    ///
    /// if (dashboardResult.IsSuccess)
    /// {
    ///     var (profile, activity, metrics, notifications) = dashboardResult.Value;
    ///     return Ok(new CompleteDashboard
    ///     {
    ///         Profile = profile,
    ///         RecentActivity = activity,
    ///         Metrics = metrics,
    ///         Notifications = notifications
    ///     });
    /// }
    /// else
    /// {
    ///     // Log all failures and return appropriate error response
    ///     logger.LogError("Dashboard load failed: {Errors}", dashboardResult.Errors);
    ///     return StatusCode(500, "Unable to load complete dashboard data");
    /// }
    /// </code>
    ///
    /// Multi-step validation for complex business operation:
    /// <code>
    /// var authResult = ValidateAuthentication(token);              // Result&lt;AuthContext&gt;
    /// var permResult = ValidatePermissions(userId, operation);     // Result&lt;PermissionSet&gt;
    /// var quotaResult = ValidateQuota(userId, resourceType);       // Result&lt;QuotaStatus&gt;
    /// var stateResult = ValidateResourceState(resourceId);         // Result&lt;ResourceState&gt;
    ///
    /// var preconditionsResult = authResult.Zip(permResult, quotaResult, stateResult);
    ///
    /// if (preconditionsResult.IsSuccess)
    /// {
    ///     var (auth, permissions, quota, state) = preconditionsResult.Value;
    ///     // All preconditions met - execute the operation
    ///     return await ExecuteComplexOperation(auth, permissions, quota, state);
    /// }
    /// else
    /// {
    ///     // Return detailed validation failures to client
    ///     return BadRequest(new
    ///     {
    ///         Message = "Operation preconditions not met",
    ///         Failures = preconditionsResult.Errors
    ///     });
    /// }
    /// </code>
    /// </example>
    public static Result<(T1, T2, T3, T4)> Zip<T1, T2, T3, T4>(
        this Result<T1> result1,
        Result<T2> result2,
        Result<T3> result3,
        Result<T4> result4)
    {
        ArgumentNullException.ThrowIfNull(result1);
        ArgumentNullException.ThrowIfNull(result2);
        ArgumentNullException.ThrowIfNull(result3);
        ArgumentNullException.ThrowIfNull(result4);

        if (result1.IsSuccess && result2.IsSuccess && result3.IsSuccess && result4.IsSuccess)
        {
            return Result<(T1, T2, T3, T4)>.Success((result1.Value, result2.Value, result3.Value, result4.Value));
        }

        var errors = new List<IError>();
        if (result1.IsFailure) errors.AddRange(result1.Errors);
        if (result2.IsFailure) errors.AddRange(result2.Errors);
        if (result3.IsFailure) errors.AddRange(result3.Errors);
        if (result4.IsFailure) errors.AddRange(result4.Errors);

        return Result<(T1, T2, T3, T4)>.Failure(errors);
    }

    #endregion

    #region FirstSuccess

    /// <summary>
    /// Returns the first successful result from a collection of results using a "first-wins" or "fallback" pattern.
    /// This combinator implements a short-circuit evaluation strategy where the first success is immediately returned.
    /// Ideal for retry logic, fallback scenarios, trying multiple data sources in priority order,
    /// or implementing resilience patterns where you want the first available successful result.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the results.</typeparam>
    /// <param name="results">The array of results to evaluate in order. Cannot be null or empty.</param>
    /// <returns>
    /// The first successful Result encountered in the array,
    /// or a failure Result with all accumulated errors if all results fail.
    /// </returns>
    /// <remarks>
    /// Success Condition: At least ONE result is successful (returns immediately on first success).
    /// Failure Condition: ALL results fail (accumulates all errors from all failed results).
    /// Results are evaluated in the order provided, stopping at the first success.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when no results are provided.</exception>
    /// <example>
    /// Fallback pattern for multiple data sources:
    /// <code>
    /// var cacheResult = await cacheService.GetUserAsync(userId);
    /// var dbResult = await databaseService.GetUserAsync(userId);
    /// var apiResult = await externalApiService.GetUserAsync(userId);
    ///
    /// var userResult = ResultCombinators.FirstSuccess(cacheResult, dbResult, apiResult);
    ///
    /// if (userResult.IsSuccess)
    /// {
    ///     // Got user from the first available source (cache preferred, then db, then API)
    ///     return Ok(userResult.Value);
    /// }
    /// else
    /// {
    ///     // All sources failed - return all errors for debugging
    ///     logger.LogError("All user data sources failed: {Errors}", userResult.Errors);
    ///     return NotFound(userResult.Errors);
    /// }
    /// </code>
    ///
    /// Retry logic with different strategies:
    /// <code>
    /// var quickAttempt = await service.TryQuickOperationAsync();
    /// var normalAttempt = await service.TryNormalOperationAsync();
    /// var slowAttempt = await service.TrySlowButReliableOperationAsync();
    ///
    /// var result = ResultCombinators.FirstSuccess(quickAttempt, normalAttempt, slowAttempt);
    ///
    /// return result.IsSuccess
    ///     ? Ok(result.Value)
    ///     : StatusCode(503, "Service temporarily unavailable after all retry attempts");
    /// </code>
    ///
    /// Multi-provider service with priority order:
    /// <code>
    /// var primaryProvider = await primaryService.ProcessPaymentAsync(payment);
    /// var secondaryProvider = await secondaryService.ProcessPaymentAsync(payment);
    /// var backupProvider = await backupService.ProcessPaymentAsync(payment);
    ///
    /// var paymentResult = ResultCombinators.FirstSuccess(
    ///     primaryProvider,
    ///     secondaryProvider,
    ///     backupProvider);
    ///
    /// if (paymentResult.IsSuccess)
    /// {
    ///     await confirmationService.SendReceiptAsync(paymentResult.Value);
    ///     return Ok(paymentResult.Value);
    /// }
    /// else
    /// {
    ///     // All payment providers failed
    ///     return BadRequest(new
    ///     {
    ///         Message = "Payment processing failed",
    ///         Errors = paymentResult.Errors
    ///     });
    /// }
    /// </code>
    /// </example>
    public static Result<T> FirstSuccess<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (results.Length == 0)
        {
            throw new ArgumentException("At least one result must be provided.", nameof(results));
        }

        var errors = new List<IError>();

        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                return result;
            }

            errors.AddRange(result.Errors);
        }

        return Result<T>.Failure(errors);
    }

    /// <summary>
    /// Returns the first successful result from a collection of results using a "first-wins" or "fallback" pattern.
    /// This is an extension method variant that operates on IEnumerable, making it chainable with LINQ operations.
    /// Use this when evaluating results from enumerable sources where you want the first successful result.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the results.</typeparam>
    /// <param name="results">The enumerable collection of results to evaluate in order. Cannot be null or empty.</param>
    /// <returns>
    /// The first successful Result encountered in the collection,
    /// or a failure Result with all accumulated errors if all results fail.
    /// </returns>
    /// <remarks>
    /// Success Condition: At least ONE result is successful (returns immediately on first success).
    /// Failure Condition: ALL results fail (accumulates all errors from all failed results).
    /// Results are evaluated in iteration order, stopping at the first success.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when no results are provided.</exception>
    /// <example>
    /// LINQ-based data source fallback:
    /// <code>
    /// var dataSources = new[] { cacheService, dbService, apiService };
    ///
    /// var result = dataSources
    ///     .Select(source => source.GetDataAsync(key))
    ///     .FirstSuccess();
    ///
    /// return result.IsSuccess
    ///     ? Ok(result.Value)
    ///     : NotFound("Data not available from any source");
    /// </code>
    /// </example>
    public static Result<T> FirstSuccess<T>(this IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return FirstSuccess(results.ToArray());
    }

    /// <summary>
    /// Returns the first successful result from a collection of non-generic results using a "first-wins" or "fallback" pattern.
    /// This overload is designed for operations that don't return values but only indicate success or failure.
    /// Use this for retry logic, fallback scenarios, or resilience patterns with void-returning operations.
    /// </summary>
    /// <param name="results">The array of non-generic results to evaluate in order. Cannot be null or empty.</param>
    /// <returns>
    /// The first successful Result encountered in the array,
    /// or a failure Result with all accumulated errors if all results fail.
    /// </returns>
    /// <remarks>
    /// Success Condition: At least ONE result is successful (returns immediately on first success).
    /// Failure Condition: ALL results fail (accumulates all errors from all failed results).
    /// Results are evaluated in the order provided, stopping at the first success.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when no results are provided.</exception>
    /// <example>
    /// Multi-channel notification with fallback:
    /// <code>
    /// var emailResult = await emailService.SendAsync(notification);
    /// var smsResult = await smsService.SendAsync(notification);
    /// var pushResult = await pushService.SendAsync(notification);
    ///
    /// var notificationResult = ResultCombinators.FirstSuccess(emailResult, smsResult, pushResult);
    ///
    /// if (notificationResult.IsSuccess)
    /// {
    ///     logger.LogInformation("Notification sent successfully via at least one channel");
    ///     return Ok();
    /// }
    /// else
    /// {
    ///     logger.LogError("All notification channels failed: {Errors}", notificationResult.Errors);
    ///     return StatusCode(500, "Failed to deliver notification");
    /// }
    /// </code>
    /// </example>
    public static Result FirstSuccess(params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (results.Length == 0)
        {
            throw new ArgumentException("At least one result must be provided.", nameof(results));
        }

        var errors = new List<IError>();

        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                return result;
            }

            errors.AddRange(result.Errors);
        }

        return Result.Failure(errors);
    }

    #endregion

    #region Merge

    /// <summary>
    /// Merges errors from multiple results into a single result using an error-aggregation pattern.
    /// This combinator collects all errors from failed results while ignoring successful results' values.
    /// Ideal for validation scenarios where you want to accumulate all validation errors before returning,
    /// error reporting from multiple sources, or collecting diagnostic information from parallel operations.
    /// Use when you care about collecting all errors but don't need all successful values.
    /// </summary>
    /// <typeparam name="T">The type of the value in the results.</typeparam>
    /// <param name="results">The array of results to merge errors from. Cannot be null.</param>
    /// <returns>
    /// A failure Result containing all accumulated errors if any result failed,
    /// or a success Result with the first successful value if no errors exist.
    /// Returns failure with a "NO_RESULTS" error if no results are provided and none are successful.
    /// </returns>
    /// <remarks>
    /// Success Condition: No failures exist (returns first successful value arbitrarily).
    /// Failure Condition: Any result fails (accumulates ALL errors from ALL failed results).
    /// Unlike Combine, this focuses on error collection rather than value collection.
    /// </remarks>
    /// <example>
    /// Multi-field validation with complete error reporting:
    /// <code>
    /// var nameValidation = ValidateName(user.Name);
    /// var emailValidation = ValidateEmail(user.Email);
    /// var passwordValidation = ValidatePassword(user.Password);
    ///
    /// var validationResult = ResultCombinators.MergeErrors(
    ///     nameValidation,
    ///     emailValidation,
    ///     passwordValidation);
    ///
    /// if (validationResult.IsFailure)
    /// {
    ///     // Return all validation errors to the user at once
    ///     return BadRequest(new
    ///     {
    ///         Message = "Validation failed",
    ///         Errors = validationResult.Errors.Select(e => new
    ///         {
    ///             Field = e.Code,
    ///             Message = e.Message
    ///         })
    ///     });
    /// }
    /// // Proceed with valid data
    /// return await CreateUser(user);
    /// </code>
    ///
    /// Collecting diagnostics from multiple health checks:
    /// <code>
    /// var dbHealthResult = await healthService.CheckDatabaseAsync();
    /// var cacheHealthResult = await healthService.CheckCacheAsync();
    /// var apiHealthResult = await healthService.CheckExternalApiAsync();
    ///
    /// var overallHealth = ResultCombinators.MergeErrors(
    ///     dbHealthResult,
    ///     cacheHealthResult,
    ///     apiHealthResult);
    ///
    /// if (overallHealth.IsFailure)
    /// {
    ///     // Report all system issues
    ///     logger.LogWarning("System health issues detected: {Issues}", overallHealth.Errors);
    ///     return StatusCode(503, new
    ///     {
    ///         Status = "Degraded",
    ///         Issues = overallHealth.Errors
    ///     });
    /// }
    /// return Ok(new { Status = "Healthy" });
    /// </code>
    /// </example>
    public static Result<T> MergeErrors<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var errors = results
            .Where(r => r.IsFailure)
            .SelectMany(r => r.Errors)
            .ToList();

        if (errors.Any())
        {
            return Result<T>.Failure(errors);
        }

        // If all successful, return the first value (arbitrary choice)
        var firstSuccess = results.FirstOrDefault(r => r.IsSuccess);
        return firstSuccess ?? Result<T>.Failure(new Error("NO_RESULTS", "No results provided"));
    }

    /// <summary>
    /// Merges errors from multiple non-generic results into a single result using an error-aggregation pattern.
    /// This overload is designed for operations that don't return values but only indicate success or failure.
    /// Use this for collecting errors from multiple validation checks, diagnostic operations, or health checks
    /// where you want to report all issues at once rather than stopping at the first failure.
    /// </summary>
    /// <param name="results">The array of non-generic results to merge errors from. Cannot be null.</param>
    /// <returns>
    /// A failure Result containing all accumulated errors if any result failed,
    /// or a success Result if no errors exist.
    /// </returns>
    /// <remarks>
    /// Success Condition: No failures exist.
    /// Failure Condition: Any result fails (accumulates ALL errors from ALL failed results).
    /// Useful when you need comprehensive error reporting rather than fail-fast behavior.
    /// </remarks>
    /// <example>
    /// Comprehensive precondition validation:
    /// <code>
    /// var authCheck = authService.VerifyAuthentication(token);
    /// var permissionCheck = authService.VerifyPermission(userId, "write");
    /// var rateLimitCheck = rateLimiter.CheckLimit(userId);
    /// var quotaCheck = quotaService.CheckQuota(userId, resourceType);
    ///
    /// var preconditionsResult = ResultCombinators.MergeErrors(
    ///     authCheck,
    ///     permissionCheck,
    ///     rateLimitCheck,
    ///     quotaCheck);
    ///
    /// if (preconditionsResult.IsFailure)
    /// {
    ///     // Return all precondition failures to help the client understand all issues
    ///     return Forbidden(new
    ///     {
    ///         Message = "Operation not allowed",
    ///         Reasons = preconditionsResult.Errors.Select(e => e.Message)
    ///     });
    /// }
    /// // All preconditions met - proceed with operation
    /// return await ExecuteOperation();
    /// </code>
    ///
    /// System readiness check reporting all issues:
    /// <code>
    /// var configLoaded = configService.Validate();
    /// var dbConnected = dbService.TestConnection();
    /// var cacheAvailable = cacheService.TestConnection();
    /// var filesystemWritable = fileService.TestWriteAccess();
    ///
    /// var systemReady = ResultCombinators.MergeErrors(
    ///     configLoaded,
    ///     dbConnected,
    ///     cacheAvailable,
    ///     filesystemWritable);
    ///
    /// if (systemReady.IsFailure)
    /// {
    ///     logger.LogCritical("System not ready: {Errors}", systemReady.Errors);
    ///     throw new InvalidOperationException(
    ///         $"Cannot start application. Issues: {string.Join(", ", systemReady.Errors.Select(e => e.Message))}");
    /// }
    /// logger.LogInformation("All system checks passed - application ready");
    /// </code>
    /// </example>
    public static Result MergeErrors(params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var errors = results
            .Where(r => r.IsFailure)
            .SelectMany(r => r.Errors)
            .ToList();

        return errors.Any()
            ? Result.Failure(errors)
            : Result.Success();
    }

    #endregion

    #region TryGet

    /// <summary>
    /// Extracts values from all successful results while ignoring failures using a "success-filtering" pattern.
    /// This combinator implements a best-effort approach where partial success is acceptable and failures are silently ignored.
    /// Ideal for scenarios where you want to process whatever data is available, such as aggregating results from
    /// multiple optional data sources, collecting partial results from parallel operations, or implementing
    /// graceful degradation where some failures are acceptable.
    /// </summary>
    /// <typeparam name="T">The type of the values contained in the results.</typeparam>
    /// <param name="results">The array of results to extract successful values from. Cannot be null.</param>
    /// <returns>
    /// A successful Result containing an IEnumerable of values from all successful results.
    /// Always returns success, even if all results failed (returns empty collection in that case).
    /// Failed results are silently ignored and do not contribute to the output.
    /// </returns>
    /// <remarks>
    /// Success Condition: ALWAYS succeeds (even with zero successful results).
    /// Failure Condition: NEVER fails.
    /// Only extracts values from successful results; failures are silently discarded.
    /// Use this when partial success is acceptable and you want a best-effort result collection.
    /// </remarks>
    /// <example>
    /// Aggregating data from multiple optional sources:
    /// <code>
    /// var cacheResult = cacheService.GetRecommendations(userId);      // May fail
    /// var dbResult = dbService.GetRecommendations(userId);            // May fail
    /// var mlResult = mlService.GetRecommendations(userId);            // May fail
    ///
    /// var recommendations = ResultCombinators.SuccessfulValues(cacheResult, dbResult, mlResult);
    ///
    /// // Always succeeds - get whatever recommendations are available
    /// var availableRecs = recommendations.Value.ToList();
    ///
    /// if (availableRecs.Any())
    /// {
    ///     return Ok(new { Recommendations = availableRecs, Source = "Mixed" });
    /// }
    /// else
    /// {
    ///     // No sources succeeded - return default recommendations
    ///     return Ok(new { Recommendations = GetDefaultRecommendations(), Source = "Default" });
    /// }
    /// </code>
    ///
    /// Parallel processing with graceful degradation:
    /// <code>
    /// var itemIds = new[] { 1, 2, 3, 4, 5 };
    /// var enrichmentResults = itemIds
    ///     .Select(id => externalService.EnrichItemAsync(id))
    ///     .ToArray();
    ///
    /// var successfulEnrichments = ResultCombinators.SuccessfulValues(enrichmentResults);
    ///
    /// // Process whatever enrichments succeeded
    /// logger.LogInformation(
    ///     "Enriched {SuccessCount} of {TotalCount} items",
    ///     successfulEnrichments.Value.Count(),
    ///     itemIds.Length);
    ///
    /// return Ok(new
    /// {
    ///     EnrichedItems = successfulEnrichments.Value,
    ///     TotalRequested = itemIds.Length,
    ///     SuccessRate = $"{successfulEnrichments.Value.Count() * 100 / itemIds.Length}%"
    /// });
    /// </code>
    ///
    /// Collecting metrics from multiple services (best effort):
    /// <code>
    /// var cpuMetrics = metricsService.GetCpuMetrics();
    /// var memoryMetrics = metricsService.GetMemoryMetrics();
    /// var diskMetrics = metricsService.GetDiskMetrics();
    /// var networkMetrics = metricsService.GetNetworkMetrics();
    ///
    /// var availableMetrics = ResultCombinators.SuccessfulValues(
    ///     cpuMetrics,
    ///     memoryMetrics,
    ///     diskMetrics,
    ///     networkMetrics);
    ///
    /// // Display whatever metrics are available
    /// return Ok(new
    /// {
    ///     Metrics = availableMetrics.Value,
    ///     Status = availableMetrics.Value.Any() ? "Partial" : "Unavailable"
    /// });
    /// </code>
    /// </example>
    public static Result<IEnumerable<T>> SuccessfulValues<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = results
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .ToList();

        return Result<IEnumerable<T>>.Success(values);
    }

    /// <summary>
    /// Extracts values from all successful results while ignoring failures using a "success-filtering" pattern.
    /// This is an extension method variant that operates on IEnumerable, making it chainable with LINQ operations.
    /// Use this when processing results from enumerable sources where partial success is acceptable.
    /// </summary>
    /// <typeparam name="T">The type of the values contained in the results.</typeparam>
    /// <param name="results">The enumerable collection of results to extract successful values from. Cannot be null.</param>
    /// <returns>
    /// A successful Result containing an IEnumerable of values from all successful results.
    /// Always returns success, even if all results failed (returns empty collection in that case).
    /// </returns>
    /// <remarks>
    /// Success Condition: ALWAYS succeeds (even with zero successful results).
    /// Failure Condition: NEVER fails.
    /// Only extracts values from successful results; failures are silently discarded.
    /// </remarks>
    /// <example>
    /// LINQ-based batch processing with graceful degradation:
    /// <code>
    /// var userIds = new[] { 1, 2, 3, 4, 5 };
    ///
    /// var profiles = userIds
    ///     .Select(id => profileService.GetProfileAsync(id))
    ///     .SuccessfulValues();
    ///
    /// // Process all successfully retrieved profiles
    /// logger.LogInformation("Retrieved {Count} profiles", profiles.Value.Count());
    /// return Ok(profiles.Value);
    /// </code>
    /// </example>
    public static Result<IEnumerable<T>> SuccessfulValues<T>(this IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return SuccessfulValues(results.ToArray());
    }

    #endregion

    #region Partition

    /// <summary>
    /// Partitions a collection of results into two separate groups: successful values and errors.
    /// This combinator implements a "split" pattern that separates successes from failures without losing information.
    /// Ideal for scenarios where you need to process both successful and failed results differently,
    /// such as partial batch processing, error reporting with successful item tracking,
    /// or implementing partial success responses where you want to report both what succeeded and what failed.
    /// </summary>
    /// <typeparam name="T">The type of the values contained in the successful results.</typeparam>
    /// <param name="results">The array of results to partition. Cannot be null.</param>
    /// <returns>
    /// A tuple containing two collections:
    /// - Successes: An IEnumerable of values from all successful results.
    /// - Errors: An IEnumerable of all errors from all failed results.
    /// Both collections may be empty if no results match their category.
    /// </returns>
    /// <remarks>
    /// Always succeeds and returns both successful values and errors.
    /// Unlike other combinators, this doesn't return a Result - it returns raw data for custom processing.
    /// Use this when you need to handle successes and failures separately in the same operation.
    /// </remarks>
    /// <example>
    /// Batch processing with detailed reporting:
    /// <code>
    /// var itemIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    ///
    /// var processingResults = itemIds
    ///     .Select(id => processService.ProcessItemAsync(id))
    ///     .ToArray();
    ///
    /// var (successfulItems, errors) = ResultCombinators.Partition(processingResults);
    ///
    /// // Report both successes and failures
    /// return Ok(new
    /// {
    ///     SuccessCount = successfulItems.Count(),
    ///     FailureCount = errors.Count(),
    ///     ProcessedItems = successfulItems,
    ///     Errors = errors.Select(e => new { e.Code, e.Message })
    /// });
    /// </code>
    ///
    /// Partial import with error tracking:
    /// <code>
    /// var importResults = csvRecords
    ///     .Select(record => ValidateAndImportRecord(record))
    ///     .ToArray();
    ///
    /// var (importedRecords, validationErrors) = ResultCombinators.Partition(importResults);
    ///
    /// if (importedRecords.Any())
    /// {
    ///     await repository.SaveBatchAsync(importedRecords);
    /// }
    ///
    /// return Ok(new ImportSummary
    /// {
    ///     TotalRecords = csvRecords.Length,
    ///     SuccessfulImports = importedRecords.Count(),
    ///     FailedImports = validationErrors.Count(),
    ///     ImportedData = importedRecords.ToList(),
    ///     ValidationErrors = validationErrors
    ///         .GroupBy(e => e.Code)
    ///         .Select(g => new { ErrorType = g.Key, Count = g.Count() })
    ///         .ToList()
    /// });
    /// </code>
    ///
    /// Multi-user operation with individual status tracking:
    /// <code>
    /// var userIds = new[] { 1, 2, 3, 4, 5 };
    ///
    /// var updateResults = userIds
    ///     .Select(id => userService.UpdateUserAsync(id, newData))
    ///     .ToArray();
    ///
    /// var (updatedUsers, updateErrors) = ResultCombinators.Partition(updateResults);
    ///
    /// // Send notifications only to successfully updated users
    /// foreach (var user in updatedUsers)
    /// {
    ///     await notificationService.NotifyUserUpdatedAsync(user);
    /// }
    ///
    /// // Log failures for investigation
    /// if (updateErrors.Any())
    /// {
    ///     logger.LogWarning(
    ///         "Some user updates failed. Success: {SuccessCount}, Failed: {FailureCount}",
    ///         updatedUsers.Count(),
    ///         updateErrors.Count());
    /// }
    ///
    /// return Accepted(new
    /// {
    ///     Message = "Update completed with partial success",
    ///     UpdatedUserIds = updatedUsers.Select(u => u.Id),
    ///     FailedUpdates = updateErrors.Select(e => e.Message)
    /// });
    /// </code>
    /// </example>
    public static (IEnumerable<T> Successes, IEnumerable<IError> Errors) Partition<T>(
        params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var successes = results
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .ToList();

        var errors = results
            .Where(r => r.IsFailure)
            .SelectMany(r => r.Errors)
            .ToList();

        return (successes, errors);
    }

    /// <summary>
    /// Partitions a collection of results into two separate groups: successful values and errors.
    /// This is an extension method variant that operates on IEnumerable, making it chainable with LINQ operations.
    /// Use this when processing results from enumerable sources where you need to handle successes and failures separately.
    /// </summary>
    /// <typeparam name="T">The type of the values contained in the successful results.</typeparam>
    /// <param name="results">The enumerable collection of results to partition. Cannot be null.</param>
    /// <returns>
    /// A tuple containing two collections:
    /// - Successes: An IEnumerable of values from all successful results.
    /// - Errors: An IEnumerable of all errors from all failed results.
    /// Both collections may be empty if no results match their category.
    /// </returns>
    /// <remarks>
    /// Always succeeds and returns both successful values and errors.
    /// Unlike other combinators, this doesn't return a Result - it returns raw data for custom processing.
    /// Use this when you need to handle successes and failures separately in the same operation.
    /// </remarks>
    /// <example>
    /// LINQ-based batch processing with separated success and failure handling:
    /// <code>
    /// var records = GetRecordsToProcess();
    ///
    /// var (processed, errors) = records
    ///     .Select(record => ProcessRecord(record))
    ///     .Partition();
    ///
    /// // Handle successes
    /// await SaveSuccessfulRecords(processed);
    ///
    /// // Handle failures
    /// await LogFailures(errors);
    ///
    /// return new ProcessingReport
    /// {
    ///     TotalProcessed = processed.Count(),
    ///     TotalFailed = errors.Count(),
    ///     Errors = errors.ToList()
    /// };
    /// </code>
    /// </example>
    public static (IEnumerable<T> Successes, IEnumerable<IError> Errors) Partition<T>(
        this IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        return Partition(results.ToArray());
    }

    #endregion
}
