using eQuantic.Core.Outcomes.Errors;

namespace eQuantic.Core.Outcomes.Extensions;

/// <summary>
/// Provides asynchronous extension methods for Result types, enabling Railway-Oriented Programming patterns with async/await support.
/// These methods allow composing asynchronous operations while maintaining error handling through the Result monad.
/// </summary>
public static class ResultAsyncExtensions
{
    #region MapAsync

    /// <summary>
    /// Asynchronously transforms the value of a successful result using an async mapper function.
    /// In Railway-Oriented Programming, this represents staying on the success track while performing an asynchronous transformation.
    /// If the result is a failure, the mapper is not executed and the errors are propagated to the output result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value after transformation.</typeparam>
    /// <param name="result">The source result to transform.</param>
    /// <param name="mapper">An async function that transforms the value from TIn to TOut. This function is only called if the result is successful.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result{TOut} with the transformed value if successful,
    /// or the original errors if the input result was a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Transform a user ID to a user entity by fetching from database asynchronously
    /// Result&lt;int&gt; userIdResult = Result&lt;int&gt;.Success(42);
    /// Result&lt;User&gt; userResult = await userIdResult.MapAsync(async userId =>
    /// {
    ///     return await _userRepository.GetByIdAsync(userId);
    /// });
    ///
    /// // If userIdResult was a failure, the database call is never made
    /// Result&lt;int&gt; failedResult = Result&lt;int&gt;.Failure(Error.NotFound());
    /// Result&lt;User&gt; failedUserResult = await failedResult.MapAsync(async userId =>
    /// {
    ///     return await _userRepository.GetByIdAsync(userId); // This is never executed
    /// });
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return result.IsSuccess
            ? Result<TOut>.Success(await mapper(result.Value).ConfigureAwait(false))
            : Result<TOut>.Failure(result.Errors);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then transforms its value using a synchronous mapper function.
    /// This overload is useful when you have an async operation returning a Result and want to apply a synchronous transformation.
    /// The method first awaits the result task, then applies the mapper if the result is successful.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value after transformation.</typeparam>
    /// <param name="resultTask">A task that produces the source result to transform.</param>
    /// <param name="mapper">A synchronous function that transforms the value from TIn to TOut. This function is only called if the result is successful.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result{TOut} with the transformed value if successful,
    /// or the original errors if the input result was a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Chain an async database call with a synchronous transformation
    /// Task&lt;Result&lt;User&gt;&gt; userTask = _userRepository.GetByIdAsync(42);
    /// Result&lt;string&gt; nameResult = await userTask.MapAsync(user => user.FullName);
    ///
    /// // Useful in fluent chains with async operations
    /// var emailResult = await GetUserAsync(userId)
    ///     .MapAsync(user => user.Email.ToLowerInvariant())
    ///     .MapAsync(email => email.Trim());
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return result.Map(mapper);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then transforms its value using an async mapper function.
    /// This overload enables full async composition where both the source result and the transformation are asynchronous operations.
    /// The method first awaits the result task, then applies the async mapper if the result is successful.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value after transformation.</typeparam>
    /// <param name="resultTask">A task that produces the source result to transform.</param>
    /// <param name="mapper">An async function that transforms the value from TIn to TOut. This function is only called if the result is successful.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result{TOut} with the transformed value if successful,
    /// or the original errors if the input result was a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Chain multiple async operations with transformations
    /// Task&lt;Result&lt;int&gt;&gt; userIdTask = ValidateAndGetUserIdAsync(request);
    /// Result&lt;UserProfile&gt; profileResult = await userIdTask
    ///     .MapAsync(async userId => await _userRepository.GetByIdAsync(userId))
    ///     .MapAsync(async user => await _profileService.BuildProfileAsync(user));
    ///
    /// // Build complex async pipelines
    /// var result = await GetOrderIdAsync()
    ///     .MapAsync(async orderId => await FetchOrderAsync(orderId))
    ///     .MapAsync(async order => await EnrichOrderDataAsync(order));
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await resultTask.ConfigureAwait(false);
        return await result.MapAsync(mapper).ConfigureAwait(false);
    }

    #endregion

    #region BindAsync

    /// <summary>
    /// Asynchronously chains an operation that returns a Result, enabling Railway-Oriented Programming composition with async operations.
    /// Unlike MapAsync which transforms values, BindAsync is used when the async operation itself can fail and returns a Result.
    /// This prevents nested Result types (Result{Result{T}}) by flattening the result.
    /// If the input result is a failure, the binder function is not executed and errors are propagated.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="result">The source result to bind from.</param>
    /// <param name="binder">An async function that takes the input value and returns a new Result{TOut}. This function is only called if the input result is successful.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result{TOut} from the binder function if the input was successful,
    /// or the original errors if the input result was a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="binder"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Chain operations that can each fail independently
    /// Result&lt;int&gt; userIdResult = Result&lt;int&gt;.Success(42);
    /// Result&lt;User&gt; userResult = await userIdResult.BindAsync(async userId =>
    /// {
    ///     var user = await _userRepository.GetByIdAsync(userId);
    ///     return user != null
    ///         ? Result&lt;User&gt;.Success(user)
    ///         : Result&lt;User&gt;.Failure(Error.NotFound("User not found"));
    /// });
    ///
    /// // Build validation and operation chains
    /// var result = await validateRequest
    ///     .BindAsync(async request => await ProcessRequestAsync(request))
    ///     .BindAsync(async data => await SaveToRepositoryAsync(data));
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return result.IsSuccess
            ? await binder(result.Value).ConfigureAwait(false)
            : Result<TOut>.Failure(result.Errors);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then chains a synchronous operation that returns a Result.
    /// This overload is useful when you have an async operation producing a Result, followed by a synchronous operation that can also fail.
    /// The method first awaits the result task, then applies the binder if the result is successful.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="resultTask">A task that produces the source result to bind from.</param>
    /// <param name="binder">A synchronous function that takes the input value and returns a new Result{TOut}. This function is only called if the input result is successful.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result{TOut} from the binder function if the input was successful,
    /// or the original errors if the input result was a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="binder"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Mix async and sync operations in a pipeline
    /// Task&lt;Result&lt;string&gt;&gt; jsonTask = FetchDataFromApiAsync();
    /// Result&lt;Order&gt; orderResult = await jsonTask.BindAsync(json =>
    /// {
    ///     try
    ///     {
    ///         var order = JsonSerializer.Deserialize&lt;Order&gt;(json);
    ///         return order != null
    ///             ? Result&lt;Order&gt;.Success(order)
    ///             : Result&lt;Order&gt;.Failure(Error.Validation("Invalid JSON"));
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         return Result&lt;Order&gt;.Failure(Error.FromException(ex));
    ///     }
    /// });
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(binder);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then chains an async operation that returns a Result.
    /// This overload enables full async composition where both the source result and the bound operation are asynchronous.
    /// The method first awaits the result task, then applies the async binder if the result is successful.
    /// This is the most common pattern for building fully async Railway-Oriented Programming pipelines.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="resultTask">A task that produces the source result to bind from.</param>
    /// <param name="binder">An async function that takes the input value and returns a new Result{TOut}. This function is only called if the input result is successful.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result{TOut} from the binder function if the input was successful,
    /// or the original errors if the input result was a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="binder"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Build fully async operation chains
    /// Task&lt;Result&lt;CreateUserRequest&gt;&gt; validationTask = ValidateUserRequestAsync(request);
    /// Result&lt;User&gt; result = await validationTask
    ///     .BindAsync(async req => await CreateUserInDatabaseAsync(req))
    ///     .BindAsync(async user => await SendWelcomeEmailAsync(user))
    ///     .BindAsync(async user => await AddToDefaultGroupAsync(user));
    ///
    /// // Each operation can fail independently, and the chain stops at the first failure
    /// var orderResult = await ValidateOrderAsync(orderId)
    ///     .BindAsync(async order => await CheckInventoryAsync(order))
    ///     .BindAsync(async order => await ProcessPaymentAsync(order))
    ///     .BindAsync(async order => await ShipOrderAsync(order));
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        var result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(binder).ConfigureAwait(false);
    }

    #endregion

    #region MatchAsync

    /// <summary>
    /// Asynchronously pattern matches on the result, executing one of two async functions based on success or failure state.
    /// This is the async termination point of a Railway-Oriented Programming pipeline, converting the Result into a concrete value.
    /// Both branches must return the same type TOut, ensuring consistent output regardless of the result state.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output value returned by both match functions.</typeparam>
    /// <param name="result">The result to pattern match on.</param>
    /// <param name="onSuccess">An async function to execute if the result is successful. Receives the result value as input.</param>
    /// <param name="onFailure">An async function to execute if the result is a failure. Receives the list of errors as input.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the value produced by either the onSuccess or onFailure function,
    /// depending on whether the result was successful or failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Convert a Result to an HTTP response asynchronously
    /// Result&lt;User&gt; userResult = await GetUserAsync(userId);
    /// IActionResult response = await userResult.MatchAsync(
    ///     onSuccess: async user =>
    ///     {
    ///         var dto = await MapToUserDtoAsync(user);
    ///         return Ok(dto);
    ///     },
    ///     onFailure: async errors =>
    ///     {
    ///         await LogErrorsAsync(errors);
    ///         return BadRequest(errors);
    ///     }
    /// );
    ///
    /// // Generate different outputs based on result state
    /// string message = await processResult.MatchAsync(
    ///     onSuccess: async data => await FormatSuccessMessageAsync(data),
    ///     onFailure: async errors => await FormatErrorMessageAsync(errors)
    /// );
    /// </code>
    /// </example>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> onSuccess,
        Func<IReadOnlyList<IError>, Task<TOut>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return result.IsSuccess
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result.Errors).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then pattern matches using synchronous functions.
    /// This overload is useful when you have an async operation producing a Result, but want to apply synchronous transformations at the end.
    /// The method first awaits the result task, then executes the appropriate synchronous match function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output value returned by both match functions.</typeparam>
    /// <param name="resultTask">A task that produces the result to pattern match on.</param>
    /// <param name="onSuccess">A synchronous function to execute if the result is successful. Receives the result value as input.</param>
    /// <param name="onFailure">A synchronous function to execute if the result is a failure. Receives the list of errors as input.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the value produced by either the onSuccess or onFailure function,
    /// depending on whether the result was successful or failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Await an async operation and convert to a simple value
    /// Task&lt;Result&lt;int&gt;&gt; countTask = GetRecordCountAsync();
    /// string message = await countTask.MatchAsync(
    ///     onSuccess: count => $"Found {count} records",
    ///     onFailure: errors => $"Error: {string.Join(", ", errors.Select(e => e.Message))}"
    /// );
    ///
    /// // Use in API controllers with synchronous response mapping
    /// var response = await ProcessOrderAsync(orderId).MatchAsync(
    ///     onSuccess: order => Ok(order),
    ///     onFailure: errors => BadRequest(new { errors })
    /// );
    /// </code>
    /// </example>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<IReadOnlyList<IError>, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then pattern matches using async functions.
    /// This overload enables full async composition where the source, success handler, and failure handler are all asynchronous.
    /// The method first awaits the result task, then executes the appropriate async match function.
    /// This is the most common pattern for fully async Railway-Oriented Programming pipelines that need to terminate with async operations.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output value returned by both match functions.</typeparam>
    /// <param name="resultTask">A task that produces the result to pattern match on.</param>
    /// <param name="onSuccess">An async function to execute if the result is successful. Receives the result value as input.</param>
    /// <param name="onFailure">An async function to execute if the result is a failure. Receives the list of errors as input.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the value produced by either the onSuccess or onFailure function,
    /// depending on whether the result was successful or failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Build a complete async pipeline with async result handling
    /// Task&lt;Result&lt;Order&gt;&gt; orderTask = ProcessOrderAsync(request);
    /// NotificationResult notification = await orderTask.MatchAsync(
    ///     onSuccess: async order =>
    ///     {
    ///         await _emailService.SendConfirmationAsync(order.CustomerEmail);
    ///         await _logger.LogSuccessAsync(order.Id);
    ///         return NotificationResult.Success("Order processed");
    ///     },
    ///     onFailure: async errors =>
    ///     {
    ///         await _logger.LogErrorsAsync(errors);
    ///         await _alertService.NotifyAdminAsync(errors);
    ///         return NotificationResult.Failed(errors);
    ///     }
    /// );
    ///
    /// // Combine with other async extensions for complete pipelines
    /// var result = await ValidateInputAsync(data)
    ///     .BindAsync(async d => await ProcessAsync(d))
    ///     .BindAsync(async p => await SaveAsync(p))
    ///     .MatchAsync(
    ///         onSuccess: async saved => await GenerateReportAsync(saved),
    ///         onFailure: async errors => await CreateErrorReportAsync(errors)
    ///     );
    /// </code>
    /// </example>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> onSuccess,
        Func<IReadOnlyList<IError>, Task<TOut>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        var result = await resultTask.ConfigureAwait(false);
        return await result.MatchAsync(onSuccess, onFailure).ConfigureAwait(false);
    }

    #endregion

    #region TapAsync

    /// <summary>
    /// Asynchronously executes a side-effect action on the result value if successful, then returns the original result unchanged.
    /// In Railway-Oriented Programming, this allows performing async side effects (like logging, caching, or notifications)
    /// without altering the railway flow. The result passes through unchanged, making this ideal for observation and debugging.
    /// If the result is a failure, the action is not executed.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to tap into.</param>
    /// <param name="action">An async action to execute on the result value. This action cannot modify the result and should be used for side effects only.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original result, unchanged,
    /// after the action has been executed (if the result was successful).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Log successful operations without interrupting the pipeline
    /// Result&lt;User&gt; userResult = Result&lt;User&gt;.Success(user);
    /// Result&lt;User&gt; sameResult = await userResult.TapAsync(async u =>
    /// {
    ///     await _logger.LogInformationAsync($"Processing user: {u.Name}");
    ///     await _cache.SetAsync($"user:{u.Id}", u);
    /// });
    /// // sameResult contains the same user as userResult
    ///
    /// // Chain multiple taps for different side effects
    /// var result = await GetUserAsync(userId)
    ///     .TapAsync(async user => await LogAccessAsync(user))
    ///     .TapAsync(async user => await UpdateLastSeenAsync(user))
    ///     .TapAsync(async user => await IncrementViewCountAsync(user));
    /// </code>
    /// </example>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            await action(result.Value).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously awaits a result task and then executes a synchronous side-effect action if successful, returning the original result unchanged.
    /// This overload is useful when you have an async operation producing a Result, and want to perform a synchronous side effect.
    /// The method first awaits the result task, then executes the action if successful, then returns the original result.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">A task that produces the result to tap into.</param>
    /// <param name="action">A synchronous action to execute on the result value. This action cannot modify the result and should be used for side effects only.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original result, unchanged,
    /// after the action has been executed (if the result was successful).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Await async operation and perform synchronous logging
    /// Task&lt;Result&lt;Order&gt;&gt; orderTask = ProcessOrderAsync(request);
    /// Result&lt;Order&gt; orderResult = await orderTask.TapAsync(order =>
    /// {
    ///     Console.WriteLine($"Order {order.Id} processed");
    ///     _metrics.Increment("orders.processed");
    /// });
    ///
    /// // Use in pipelines mixing async and sync operations
    /// var result = await FetchDataAsync()
    ///     .TapAsync(data => Console.WriteLine($"Received {data.Length} bytes"))
    ///     .MapAsync(data => data.ToUpperInvariant());
    /// </code>
    /// </example>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then executes an async side-effect action if successful, returning the original result unchanged.
    /// This overload enables full async composition where both the source result and the side effect are asynchronous operations.
    /// The method first awaits the result task, then executes the async action if successful, then returns the original result.
    /// This is the most common pattern for adding async side effects to fully async pipelines.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">A task that produces the result to tap into.</param>
    /// <param name="action">An async action to execute on the result value. This action cannot modify the result and should be used for side effects only.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original result, unchanged,
    /// after the action has been executed (if the result was successful).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Build fully async pipelines with side effects
    /// Task&lt;Result&lt;User&gt;&gt; userTask = CreateUserAsync(request);
    /// Result&lt;User&gt; userResult = await userTask
    ///     .TapAsync(async user => await _auditLog.LogCreationAsync(user))
    ///     .TapAsync(async user => await _cache.InvalidateAsync($"users:{user.Id}"))
    ///     .TapAsync(async user => await _notifications.SendWelcomeEmailAsync(user));
    ///
    /// // Combine with other async extensions
    /// var finalResult = await ValidateAsync(input)
    ///     .BindAsync(async data => await ProcessAsync(data))
    ///     .TapAsync(async result => await LogSuccessAsync(result))
    ///     .BindAsync(async result => await SaveAsync(result))
    ///     .TapAsync(async saved => await NotifySubscribersAsync(saved));
    /// </code>
    /// </example>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(action).ConfigureAwait(false);
    }

    #endregion

    #region EnsureAsync

    /// <summary>
    /// Asynchronously validates the result value using an async predicate, converting success to failure if the predicate returns false.
    /// In Railway-Oriented Programming, this acts as a guard that switches from the success track to the failure track if validation fails.
    /// If the input result is already a failure, it passes through unchanged without executing the predicate.
    /// If the predicate returns false, the provided error is used to create a failure result.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">An async function that validates the result value. Should return true if validation passes, false otherwise.</param>
    /// <param name="error">The error to use if the predicate returns false.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original result if it was a failure or if the predicate returned true,
    /// or a new failure result with the specified error if the predicate returned false.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Validate user age asynchronously against database rules
    /// Result&lt;User&gt; userResult = Result&lt;User&gt;.Success(user);
    /// Result&lt;User&gt; validatedResult = await userResult.EnsureAsync(
    ///     async u => await _ageVerificationService.IsAdultAsync(u.DateOfBirth),
    ///     Error.Validation("User must be 18 or older")
    /// );
    ///
    /// // Chain multiple async validations
    /// var result = await GetUserAsync(userId)
    ///     .EnsureAsync(
    ///         async u => await _permissions.HasAccessAsync(u),
    ///         Error.Forbidden("User lacks required permissions"))
    ///     .EnsureAsync(
    ///         async u => await _subscription.IsActiveAsync(u),
    ///         Error.Validation("Subscription expired"));
    /// </code>
    /// </example>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicate,
        IError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Value).ConfigureAwait(false)
            ? result
            : Result<T>.Failure(error);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then validates its value using a synchronous predicate.
    /// This overload is useful when you have an async operation producing a Result, followed by a synchronous validation.
    /// The method first awaits the result task, then applies the predicate if the result is successful.
    /// If the predicate returns false, the provided error is used to create a failure result.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">A task that produces the result to validate.</param>
    /// <param name="predicate">A synchronous function that validates the result value. Should return true if validation passes, false otherwise.</param>
    /// <param name="error">The error to use if the predicate returns false.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original result if it was a failure or if the predicate returned true,
    /// or a new failure result with the specified error if the predicate returned false.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Await async operation then apply synchronous validation
    /// Task&lt;Result&lt;int&gt;&gt; quantityTask = GetQuantityAsync(productId);
    /// Result&lt;int&gt; validatedResult = await quantityTask.EnsureAsync(
    ///     qty => qty > 0,
    ///     Error.Validation("Quantity must be positive")
    /// );
    ///
    /// // Use in pipelines with synchronous business rules
    /// var result = await FetchOrderAsync(orderId)
    ///     .EnsureAsync(
    ///         order => order.Status == OrderStatus.Pending,
    ///         Error.Validation("Order must be in pending status"))
    ///     .EnsureAsync(
    ///         order => order.Total >= 0,
    ///         Error.Validation("Order total cannot be negative"));
    /// </code>
    /// </example>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        IError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        var result = await resultTask.ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>
    /// Asynchronously awaits a result task and then validates its value using an async predicate.
    /// This overload enables full async composition where both the source result and the validation are asynchronous operations.
    /// The method first awaits the result task, then applies the async predicate if the result is successful.
    /// If the predicate returns false, the provided error is used to create a failure result.
    /// This is the most common pattern for async validation in fully async Railway-Oriented Programming pipelines.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">A task that produces the result to validate.</param>
    /// <param name="predicate">An async function that validates the result value. Should return true if validation passes, false otherwise.</param>
    /// <param name="error">The error to use if the predicate returns false.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the original result if it was a failure or if the predicate returned true,
    /// or a new failure result with the specified error if the predicate returned false.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Build fully async validation pipelines
    /// Task&lt;Result&lt;User&gt;&gt; userTask = CreateUserAsync(request);
    /// Result&lt;User&gt; validatedUser = await userTask
    ///     .EnsureAsync(
    ///         async u => await _uniquenessChecker.IsEmailUniqueAsync(u.Email),
    ///         Error.Validation("Email already exists"))
    ///     .EnsureAsync(
    ///         async u => await _blacklistService.IsNotBlacklistedAsync(u.Email),
    ///         Error.Validation("Email is blacklisted"));
    ///
    /// // Combine with other async extensions for complex workflows
    /// var result = await ParseRequestAsync(json)
    ///     .BindAsync(async req => await ValidateSchemaAsync(req))
    ///     .EnsureAsync(
    ///         async req => await _rateLimiter.CheckLimitAsync(req.UserId),
    ///         Error.TooManyRequests("Rate limit exceeded"))
    ///     .BindAsync(async req => await ProcessRequestAsync(req));
    /// </code>
    /// </example>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        IError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        var result = await resultTask.ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error).ConfigureAwait(false);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Converts a Task{T} into a Task{Result{T}}, wrapping successful task completion in a success result and exceptions in failure results.
    /// This is a utility method for integrating async operations that don't use the Result pattern into Railway-Oriented Programming pipelines.
    /// Any exception thrown during task execution is caught and converted into a failure result with the exception details.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the task.</typeparam>
    /// <param name="task">The task to convert into a Result.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a success Result{T} with the task's value if the task completed successfully,
    /// or a failure Result{T} with an error created from the exception if the task threw an exception.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Convert a regular async method to Result-based approach
    /// Task&lt;User&gt; userTask = _userRepository.GetByIdAsync(userId);
    /// Result&lt;User&gt; userResult = await userTask.ToResultAsync();
    /// // If GetByIdAsync throws an exception, it becomes a failure result
    ///
    /// // Use in pipelines to wrap third-party async operations
    /// var result = await httpClient.GetStringAsync(url)
    ///     .ToResultAsync()
    ///     .BindAsync(async json => await ParseJsonAsync(json))
    ///     .BindAsync(async data => await ProcessDataAsync(data));
    ///
    /// // Handle both success and exception cases uniformly
    /// var outcome = await externalService.CallApiAsync()
    ///     .ToResultAsync()
    ///     .MatchAsync(
    ///         onSuccess: async data => await HandleSuccessAsync(data),
    ///         onFailure: async errors => await HandleErrorsAsync(errors)
    ///     );
    /// </code>
    /// </example>
    public static async Task<Result<T>> ToResultAsync<T>(this Task<T> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            var value = await task.ConfigureAwait(false);
            return Result<T>.Success(value);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Converts a Task into a Task{Result}, wrapping successful task completion in a success result and exceptions in failure results.
    /// This overload handles tasks that don't return a value (void async operations).
    /// This is a utility method for integrating void async operations that don't use the Result pattern into Railway-Oriented Programming pipelines.
    /// Any exception thrown during task execution is caught and converted into a failure result with the exception details.
    /// </summary>
    /// <param name="task">The task to convert into a Result.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a success Result if the task completed successfully,
    /// or a failure Result with an error created from the exception if the task threw an exception.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Convert a void async method to Result-based approach
    /// Task saveTask = _repository.SaveChangesAsync();
    /// Result result = await saveTask.ToResultAsync();
    /// // If SaveChangesAsync throws an exception, it becomes a failure result
    ///
    /// // Use in pipelines with void operations
    /// var outcome = await SendEmailAsync(recipient, message)
    ///     .ToResultAsync()
    ///     .BindAsync(async () => await LogEmailSentAsync())
    ///     .MatchAsync(
    ///         onSuccess: async () => await UpdateNotificationStatusAsync(),
    ///         onFailure: async errors => await HandleEmailFailureAsync(errors)
    ///     );
    ///
    /// // Wrap external void async operations for consistent error handling
    /// Result cleanupResult = await externalService.CleanupResourcesAsync()
    ///     .ToResultAsync();
    /// if (cleanupResult.IsFailure)
    /// {
    ///     await LogErrorsAsync(cleanupResult.Errors);
    /// }
    /// </code>
    /// </example>
    public static async Task<Result> ToResultAsync(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await task.ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.FromException(ex));
        }
    }

    #endregion
}
