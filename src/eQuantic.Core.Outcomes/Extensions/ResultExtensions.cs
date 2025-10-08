using eQuantic.Core.Outcomes.Errors;

namespace eQuantic.Core.Outcomes.Extensions;

/// <summary>
/// Extension methods for Result types implementing Railway-Oriented Programming.
/// </summary>
public static class ResultExtensions
{
    #region Map (Functor)

    /// <summary>
    /// Transforms the value inside a successful result using a mapping function (functor pattern).
    /// If the result is failed, short-circuits and returns a new failed result preserving the original errors.
    /// This operation stays on the "happy path" of the railway, transforming the payload without unwrapping.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input result.</typeparam>
    /// <typeparam name="TOut">The type of the value that will be contained in the output result.</typeparam>
    /// <param name="result">The source result to transform.</param>
    /// <param name="mapper">A pure function that transforms the success value from TIn to TOut.</param>
    /// <returns>
    /// A new Result containing the transformed value if the input was successful,
    /// or a failed Result with the original errors if the input was failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// // Transform a user ID into a user object
    /// Result&lt;int&gt; userIdResult = Result&lt;int&gt;.Success(42);
    ///
    /// Result&lt;string&gt; userNameResult = userIdResult
    ///     .Map(id => $"User_{id}"); // Transform int to string
    ///
    /// // userNameResult contains "User_42"
    ///
    /// // Failed results propagate errors automatically
    /// Result&lt;int&gt; failedResult = Result&lt;int&gt;.Failure(ValidationError.Create("Invalid ID"));
    /// Result&lt;string&gt; stillFailed = failedResult
    ///     .Map(id => $"User_{id}"); // Mapper is not executed
    ///
    /// // stillFailed contains the same ValidationError
    /// </code>
    /// </example>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value))
            : Result<TOut>.Failure(result.Errors);
    }

    /// <summary>
    /// Transforms a non-generic (void) result into a generic result by providing a value through a factory function.
    /// If the result is failed, short-circuits and returns a failed result preserving the original errors.
    /// Useful for converting operation results into value-bearing results.
    /// </summary>
    /// <typeparam name="TOut">The type of the value that will be contained in the output result.</typeparam>
    /// <param name="result">The source non-generic result to transform.</param>
    /// <param name="mapper">A factory function that produces the value for the successful result.</param>
    /// <returns>
    /// A new Result containing the produced value if the input was successful,
    /// or a failed Result with the original errors if the input was failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// // Convert a void operation result into a value result
    /// Result operationResult = SaveToDatabase();
    ///
    /// Result&lt;DateTime&gt; timestampedResult = operationResult
    ///     .Map(() => DateTime.UtcNow); // Add timestamp to successful operation
    ///
    /// // Chain with other operations
    /// Result&lt;string&gt; confirmation = operationResult
    ///     .Map(() => "Operation completed successfully")
    ///     .Map(msg => $"{msg} at {DateTime.UtcNow}");
    /// </code>
    /// </example>
    public static Result<TOut> Map<TOut>(
        this Result result,
        Func<TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return result.IsSuccess
            ? Result<TOut>.Success(mapper())
            : Result<TOut>.Failure(result.Errors);
    }

    #endregion

    #region Bind (Monad)

    /// <summary>
    /// Chains a result-producing operation using the monadic bind pattern (also known as FlatMap or SelectMany).
    /// Unwraps the value from a successful result, applies a function that returns a new Result, and flattens the output.
    /// If the input result is failed, short-circuits and propagates the errors without executing the binder.
    /// This is the fundamental operation for composing Railway-Oriented Programming pipelines.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input result.</typeparam>
    /// <typeparam name="TOut">The type of the value that will be contained in the output result.</typeparam>
    /// <param name="result">The source result to bind.</param>
    /// <param name="binder">A function that takes the unwrapped success value and returns a new Result. This function may succeed or fail.</param>
    /// <returns>
    /// The Result returned by the binder function if the input was successful,
    /// or a failed Result with the original errors if the input was failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="binder"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// // Chain multiple operations that can fail
    /// Result&lt;string&gt; GetUser(int id) =>
    ///     id > 0
    ///         ? Result&lt;string&gt;.Success($"User{id}")
    ///         : Result&lt;string&gt;.Failure(ValidationError.Create("Invalid user ID"));
    ///
    /// Result&lt;Email&gt; GetEmail(string username) =>
    ///     !string.IsNullOrEmpty(username)
    ///         ? Result&lt;Email&gt;.Success(new Email($"{username}@example.com"))
    ///         : Result&lt;Email&gt;.Failure(ValidationError.Create("Username is required"));
    ///
    /// // Use Bind to chain operations on the railway
    /// Result&lt;int&gt; userIdResult = Result&lt;int&gt;.Success(42);
    ///
    /// Result&lt;Email&gt; emailResult = userIdResult
    ///     .Bind(id => GetUser(id))        // Result&lt;int&gt; -> Result&lt;string&gt;
    ///     .Bind(username => GetEmail(username)); // Result&lt;string&gt; -> Result&lt;Email&gt;
    ///
    /// // If any step fails, the error propagates through the chain
    /// Result&lt;int&gt; invalidId = Result&lt;int&gt;.Success(-1);
    /// Result&lt;Email&gt; failed = invalidId
    ///     .Bind(id => GetUser(id))        // Fails here
    ///     .Bind(username => GetEmail(username)); // Not executed
    /// </code>
    /// </example>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return result.IsSuccess
            ? binder(result.Value)
            : Result<TOut>.Failure(result.Errors);
    }

    /// <summary>
    /// Chains a result-producing operation from a non-generic (void) result to a generic result.
    /// If the input result is successful, executes the binder function to produce a value-bearing result.
    /// If the input result is failed, short-circuits and propagates the errors without executing the binder.
    /// Useful for sequencing operations where the first is void and the second produces a value.
    /// </summary>
    /// <typeparam name="TOut">The type of the value that will be contained in the output result.</typeparam>
    /// <param name="result">The source non-generic result to bind.</param>
    /// <param name="binder">A function that returns a Result with a value. This function may succeed or fail.</param>
    /// <returns>
    /// The Result returned by the binder function if the input was successful,
    /// or a failed Result with the original errors if the input was failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="binder"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// // Chain a void operation with a value-producing operation
    /// Result ValidateAccess() =>
    ///     userHasPermission
    ///         ? Result.Success()
    ///         : Result.Failure(AuthorizationError.Create("Access denied"));
    ///
    /// Result&lt;Document&gt; LoadDocument() =>
    ///     documentExists
    ///         ? Result&lt;Document&gt;.Success(new Document())
    ///         : Result&lt;Document&gt;.Failure(NotFoundError.Create("Document not found"));
    ///
    /// // First validate, then load document
    /// Result&lt;Document&gt; documentResult = ValidateAccess()
    ///     .Bind(() => LoadDocument()); // Only loads if validation succeeds
    /// </code>
    /// </example>
    public static Result<TOut> Bind<TOut>(
        this Result result,
        Func<Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return result.IsSuccess
            ? binder()
            : Result<TOut>.Failure(result.Errors);
    }

    /// <summary>
    /// Chains a result-producing operation from a generic result to a non-generic (void) result.
    /// Unwraps the value from a successful result, applies a function that returns a void Result, and returns it.
    /// If the input result is failed, short-circuits and propagates the errors without executing the binder.
    /// Useful for sequencing operations where the final operation doesn't return a value.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input result.</typeparam>
    /// <param name="result">The source result to bind.</param>
    /// <param name="binder">A function that takes the unwrapped success value and returns a non-generic Result. This function may succeed or fail.</param>
    /// <returns>
    /// The Result returned by the binder function if the input was successful,
    /// or a failed Result with the original errors if the input was failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="binder"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// // Chain a value operation with a void operation
    /// Result&lt;User&gt; GetUser(int id) =>
    ///     userExists
    ///         ? Result&lt;User&gt;.Success(new User { Id = id })
    ///         : Result&lt;User&gt;.Failure(NotFoundError.Create("User not found"));
    ///
    /// Result SendWelcomeEmail(User user) =>
    ///     emailSent
    ///         ? Result.Success()
    ///         : Result.Failure(EmailError.Create("Failed to send email"));
    ///
    /// // Get user and send email
    /// Result emailResult = GetUser(42)
    ///     .Bind(user => SendWelcomeEmail(user)); // Only sends if user is found
    ///
    /// // Can be simplified with method group
    /// Result result = GetUser(42).Bind(SendWelcomeEmail);
    /// </code>
    /// </example>
    public static Result Bind<TIn>(
        this Result<TIn> result,
        Func<TIn, Result> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return result.IsSuccess
            ? binder(result.Value)
            : Result.Failure(result.Errors);
    }

    #endregion

    #region Match (Pattern Matching)

    /// <summary>
    /// Applies pattern matching to a result by providing separate functions for success and failure cases.
    /// This is the terminal operation that exits the railway, transforming both tracks into a single return type.
    /// Both branches must return the same type, ensuring exhaustive handling of all cases.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input result.</typeparam>
    /// <typeparam name="TOut">The common return type produced by both success and failure handlers.</typeparam>
    /// <param name="result">The result to pattern match against.</param>
    /// <param name="onSuccess">A function to execute if the result is successful, receiving the unwrapped value.</param>
    /// <param name="onFailure">A function to execute if the result is failed, receiving the collection of errors.</param>
    /// <returns>The value produced by either the success or failure handler, depending on the result state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result&lt;User&gt; userResult = GetUser(42);
    ///
    /// // Convert result to HTTP response (exiting the railway)
    /// HttpResponse response = userResult.Match(
    ///     onSuccess: user => new HttpResponse
    ///     {
    ///         StatusCode = 200,
    ///         Body = JsonSerializer.Serialize(user)
    ///     },
    ///     onFailure: errors => new HttpResponse
    ///     {
    ///         StatusCode = 400,
    ///         Body = JsonSerializer.Serialize(errors)
    ///     }
    /// );
    ///
    /// // Convert to string representation
    /// string message = userResult.Match(
    ///     onSuccess: user => $"Welcome, {user.Name}!",
    ///     onFailure: errors => $"Error: {string.Join(", ", errors.Select(e => e.Message))}"
    /// );
    /// </code>
    /// </example>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<IReadOnlyList<IError>, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Errors);
    }

    /// <summary>
    /// Applies pattern matching to a non-generic (void) result by providing separate functions for success and failure cases.
    /// This is the terminal operation that exits the railway, transforming both tracks into a single return type.
    /// Both branches must return the same type, ensuring exhaustive handling of all cases.
    /// </summary>
    /// <typeparam name="TOut">The common return type produced by both success and failure handlers.</typeparam>
    /// <param name="result">The non-generic result to pattern match against.</param>
    /// <param name="onSuccess">A function to execute if the result is successful.</param>
    /// <param name="onFailure">A function to execute if the result is failed, receiving the collection of errors.</param>
    /// <returns>The value produced by either the success or failure handler, depending on the result state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result operationResult = SaveChanges();
    ///
    /// // Convert void result to status code
    /// int statusCode = operationResult.Match(
    ///     onSuccess: () => 200,
    ///     onFailure: errors => errors.Any(e => e is NotFoundError) ? 404 : 400
    /// );
    ///
    /// // Convert to user-friendly message
    /// string message = operationResult.Match(
    ///     onSuccess: () => "Operation completed successfully",
    ///     onFailure: errors => $"Operation failed: {errors.First().Message}"
    /// );
    /// </code>
    /// </example>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<IReadOnlyList<IError>, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Errors);
    }

    /// <summary>
    /// Applies pattern matching to a result by providing separate actions for success and failure cases.
    /// This is a void-returning variant useful for performing side effects at the end of a railway pipeline.
    /// Ensures both success and failure cases are explicitly handled with appropriate actions.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the input result.</typeparam>
    /// <param name="result">The result to pattern match against.</param>
    /// <param name="onSuccess">An action to execute if the result is successful, receiving the unwrapped value.</param>
    /// <param name="onFailure">An action to execute if the result is failed, receiving the collection of errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result&lt;Order&gt; orderResult = ProcessOrder(orderId);
    ///
    /// // Perform side effects based on the result
    /// orderResult.Match(
    ///     onSuccess: order =>
    ///     {
    ///         logger.LogInformation("Order {OrderId} processed successfully", order.Id);
    ///         emailService.SendConfirmation(order);
    ///     },
    ///     onFailure: errors =>
    ///     {
    ///         logger.LogError("Order processing failed: {Errors}",
    ///             string.Join(", ", errors.Select(e => e.Message)));
    ///         notificationService.AlertSupport(errors);
    ///     }
    /// );
    ///
    /// // Simple console output
    /// userResult.Match(
    ///     onSuccess: user => Console.WriteLine($"User found: {user.Name}"),
    ///     onFailure: errors => Console.WriteLine($"Errors: {errors.Count}")
    /// );
    /// </code>
    /// </example>
    public static void Match<T>(
        this Result<T> result,
        Action<T> onSuccess,
        Action<IReadOnlyList<IError>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (result.IsSuccess)
        {
            onSuccess(result.Value);
        }
        else
        {
            onFailure(result.Errors);
        }
    }

    #endregion

    #region Tap (Side Effects)

    /// <summary>
    /// Performs a side effect on the success value without modifying the result, then returns the original result unchanged.
    /// This allows you to "tap into" the railway to perform logging, caching, or other side effects while staying on the happy path.
    /// If the result is failed, the action is not executed and the result passes through unchanged.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result to tap into.</param>
    /// <param name="action">An action to perform on the success value. Should have no side effects on the result itself.</param>
    /// <returns>The original result, completely unmodified.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result&lt;User&gt; userResult = GetUser(42)
    ///     .Tap(user => logger.LogInformation("Retrieved user: {UserId}", user.Id))
    ///     .Tap(user => cache.Set($"user:{user.Id}", user))
    ///     .Map(user => user.Email);
    ///
    /// // The Tap operations don't affect the flow - they just observe
    /// // Useful for debugging pipelines
    /// Result&lt;Order&gt; order = ValidateOrder(request)
    ///     .Tap(o => Console.WriteLine($"Validation passed for order {o.Id}"))
    ///     .Bind(ApplyDiscounts)
    ///     .Tap(o => Console.WriteLine($"Discounts applied: {o.TotalDiscount}"))
    ///     .Bind(SaveOrder)
    ///     .Tap(o => Console.WriteLine($"Order saved with ID: {o.Id}"));
    ///
    /// // Failed results skip all Tap operations
    /// Result&lt;int&gt; failed = Result&lt;int&gt;.Failure(ValidationError.Create("Invalid"));
    /// var result = failed
    ///     .Tap(x => Console.WriteLine("This won't execute")); // Skipped
    /// </code>
    /// </example>
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Performs a side effect on a successful non-generic (void) result without modifying it, then returns the original result unchanged.
    /// This allows you to "tap into" the railway to perform logging or other side effects on successful operations.
    /// If the result is failed, the action is not executed and the result passes through unchanged.
    /// </summary>
    /// <param name="result">The non-generic result to tap into.</param>
    /// <param name="action">An action to perform when the result is successful.</param>
    /// <returns>The original result, completely unmodified.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result operationResult = SaveChanges()
    ///     .Tap(() => logger.LogInformation("Changes saved successfully"))
    ///     .Tap(() => cache.Invalidate())
    ///     .Tap(() => eventBus.Publish(new ChangesSavedEvent()));
    ///
    /// // Chain void operations with side effects
    /// Result result = ValidateInput()
    ///     .Tap(() => Console.WriteLine("Validation passed"))
    ///     .Bind(() => ProcessData())
    ///     .Tap(() => Console.WriteLine("Processing complete"));
    /// </code>
    /// </example>
    public static Result Tap(
        this Result result,
        Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action();
        }

        return result;
    }

    /// <summary>
    /// Performs a side effect on the error collection if the result is failed, then returns the original result unchanged.
    /// This allows you to "tap into" the error track of the railway to perform logging, monitoring, or other side effects.
    /// If the result is successful, the action is not executed and the result passes through unchanged.
    /// The complementary operation to Tap, which operates on the success track.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result to tap into.</param>
    /// <param name="action">An action to perform on the error collection when the result is failed.</param>
    /// <returns>The original result, completely unmodified.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result&lt;User&gt; userResult = GetUser(invalidId)
    ///     .TapError(errors => logger.LogError("Failed to get user: {Errors}",
    ///         string.Join(", ", errors.Select(e => e.Message))))
    ///     .TapError(errors => metrics.IncrementCounter("user_fetch_failures"))
    ///     .TapError(errors => alertService.NotifyIfCritical(errors));
    ///
    /// // Combine with success taps for comprehensive observability
    /// Result&lt;Order&gt; orderResult = ProcessOrder(orderId)
    ///     .Tap(order => logger.LogInformation("Order processed: {OrderId}", order.Id))
    ///     .TapError(errors => logger.LogError("Order processing failed: {Errors}", errors))
    ///     .Tap(order => metrics.IncrementCounter("orders_processed"))
    ///     .TapError(errors => metrics.IncrementCounter("orders_failed"));
    ///
    /// // Successful results skip TapError
    /// Result&lt;int&gt; success = Result&lt;int&gt;.Success(42);
    /// var result = success
    ///     .TapError(errors => Console.WriteLine("This won't execute")); // Skipped
    /// </code>
    /// </example>
    public static Result<T> TapError<T>(
        this Result<T> result,
        Action<IReadOnlyList<IError>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsFailure)
        {
            action(result.Errors);
        }

        return result;
    }

    #endregion

    #region Ensure (Inline Validation)

    /// <summary>
    /// Applies inline validation to a successful result using a predicate. If validation fails, switches to the error track.
    /// This is a guard operation that adds validation points within a railway pipeline, allowing you to enforce business rules.
    /// If the input result is already failed, short-circuits and returns the failed result without executing the predicate.
    /// If the result is successful but the predicate returns false, returns a failed result with the provided error.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">A validation function that returns true if the value is valid, false otherwise.</param>
    /// <param name="error">The error to include in the failed result if validation fails.</param>
    /// <returns>
    /// The original result if it was already failed or if validation passes,
    /// or a new failed result with the specified error if validation fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> or <paramref name="error"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result&lt;User&gt; userResult = GetUser(userId)
    ///     .Ensure(
    ///         user => user.Age >= 18,
    ///         ValidationError.Create("User must be 18 or older"))
    ///     .Ensure(
    ///         user => user.IsActive,
    ///         ValidationError.Create("User account is inactive"))
    ///     .Ensure(
    ///         user => !string.IsNullOrEmpty(user.Email),
    ///         ValidationError.Create("User must have an email address"));
    ///
    /// // Chain multiple validations in a fluent pipeline
    /// Result&lt;Order&gt; orderResult = CreateOrder(request)
    ///     .Ensure(order => order.Items.Count > 0,
    ///         ValidationError.Create("Order must contain at least one item"))
    ///     .Ensure(order => order.Total > 0,
    ///         ValidationError.Create("Order total must be greater than zero"))
    ///     .Ensure(order => order.Total <= 10000,
    ///         ValidationError.Create("Order total exceeds maximum allowed"))
    ///     .Bind(SaveOrder);
    /// </code>
    /// </example>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        IError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value)
            ? result
            : Result<T>.Failure(error);
    }

    /// <summary>
    /// Applies inline validation to a successful result using a predicate, with a context-aware error factory.
    /// If validation fails, switches to the error track using an error created from the invalid value.
    /// This overload is useful when you need to include information from the value itself in the error message.
    /// If the input result is already failed, short-circuits and returns the failed result without executing the predicate.
    /// If the result is successful but the predicate returns false, creates an error using the factory and returns a failed result.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <param name="predicate">A validation function that returns true if the value is valid, false otherwise.</param>
    /// <param name="errorFactory">A function that creates an error from the invalid value. Only called if validation fails.</param>
    /// <returns>
    /// The original result if it was already failed or if validation passes,
    /// or a new failed result with an error created by the factory if validation fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> or <paramref name="errorFactory"/> is null.</exception>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// Result&lt;Product&gt; productResult = GetProduct(productId)
    ///     .Ensure(
    ///         product => product.Stock > 0,
    ///         product => ValidationError.Create(
    ///             $"Product '{product.Name}' is out of stock. Current stock: {product.Stock}"))
    ///     .Ensure(
    ///         product => product.Price <= maxPrice,
    ///         product => ValidationError.Create(
    ///             $"Product price ${product.Price} exceeds maximum allowed ${maxPrice}"));
    ///
    /// // Use value context for detailed error messages
    /// Result&lt;BankAccount&gt; transferResult = GetAccount(accountId)
    ///     .Ensure(
    ///         account => account.Balance >= amount,
    ///         account => ValidationError.Create(
    ///             $"Insufficient funds. Balance: ${account.Balance}, Required: ${amount}"))
    ///     .Ensure(
    ///         account => !account.IsFrozen,
    ///         account => ValidationError.Create(
    ///             $"Account {account.AccountNumber} is frozen. Reason: {account.FreezeReason}"));
    /// </code>
    /// </example>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, IError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);

        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value)
            ? result
            : Result<T>.Failure(errorFactory(result.Value));
    }

    #endregion

    #region Conversions

    /// <summary>
    /// Converts a generic Result to a non-generic (void) Result, discarding the success value.
    /// This is useful when you need to continue a railway pipeline but no longer need the value,
    /// or when an API expects a void Result. Errors are preserved during conversion.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source result, which will be discarded.</typeparam>
    /// <param name="result">The generic result to convert to a non-generic result.</param>
    /// <returns>
    /// A non-generic Result that is successful if the input was successful (without any value),
    /// or failed with the same errors if the input was failed.
    /// </returns>
    /// <example>
    /// Railway-Oriented Programming example:
    /// <code>
    /// // Discard the value when only success/failure matters
    /// Result saveResult = GetUser(userId)
    ///     .Map(user => user with { LastLoginDate = DateTime.UtcNow })
    ///     .Bind(SaveUser)
    ///     .ToResult(); // Convert Result&lt;User&gt; to Result
    ///
    /// // Use when an API expects void Result
    /// public Result ProcessOrder(int orderId)
    /// {
    ///     return GetOrder(orderId)  // Returns Result&lt;Order&gt;
    ///         .Bind(ValidateOrder)  // Returns Result&lt;Order&gt;
    ///         .Bind(ApplyDiscounts) // Returns Result&lt;Order&gt;
    ///         .Bind(SaveOrder)      // Returns Result&lt;Order&gt;
    ///         .ToResult();          // Convert to Result for API
    /// }
    ///
    /// // Chain with void operations
    /// Result finalResult = CalculateTotal(items)  // Result&lt;decimal&gt;
    ///     .ToResult()                             // Result
    ///     .Bind(() => SendNotification());        // Result
    /// </code>
    /// </example>
    public static Result ToResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Errors);
    }

    #endregion
}
