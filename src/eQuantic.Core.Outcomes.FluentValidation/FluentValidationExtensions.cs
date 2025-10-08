using eQuantic.Core.Outcomes.Errors;
using FluentValidation;
using FluentValidation.Results;

namespace eQuantic.Core.Outcomes.FluentValidation;

/// <summary>
/// Extension methods for integrating FluentValidation with Result types.
/// Provides seamless integration for Railway-Oriented Programming (ROP) patterns with automatic conversion
/// of FluentValidation results into the Result monad, enabling validation in functional pipelines.
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Converts a FluentValidation ValidationResult to a Result{T}, enabling Railway-Oriented Programming
    /// with validation. This method bridges FluentValidation and the Result pattern by automatically
    /// converting validation failures into structured ValidationError objects that can flow through
    /// your pipeline without exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the value to be returned in the success case.</typeparam>
    /// <param name="validationResult">The FluentValidation ValidationResult containing the validation outcome.</param>
    /// <param name="value">The value to encapsulate in the Result when validation succeeds.</param>
    /// <returns>
    /// A Result{T} containing the value if validation is successful (happy path),
    /// or a failed Result{T} with ValidationError objects if validation fails (error track).
    /// Each ValidationError includes the error code, message, property name, and attempted value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when validationResult is null.</exception>
    /// <example>
    /// <code>
    /// // Define a validator
    /// public class CreateUserValidator : AbstractValidator&lt;CreateUserRequest&gt;
    /// {
    ///     public CreateUserValidator()
    ///     {
    ///         RuleFor(x => x.Email).NotEmpty().EmailAddress();
    ///         RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
    ///     }
    /// }
    ///
    /// // Use ToResult to convert validation to Result pattern
    /// var request = new CreateUserRequest { Email = "invalid", Age = 15 };
    /// var validator = new CreateUserValidator();
    /// var validationResult = validator.Validate(request);
    ///
    /// var result = validationResult.ToResult(request);
    ///
    /// if (result.IsSuccess)
    /// {
    ///     // Happy path - validation passed
    ///     var user = result.Value;
    ///     Console.WriteLine($"Valid user: {user.Email}");
    /// }
    /// else
    /// {
    ///     // Error track - validation failed
    ///     foreach (var error in result.Errors.OfType&lt;ValidationError&gt;())
    ///     {
    ///         Console.WriteLine($"{error.PropertyName}: {error.Message}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static Result<T> ToResult<T>(this ValidationResult validationResult, T value)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        if (validationResult.IsValid)
        {
            return Result<T>.Success(value);
        }

        var errors = validationResult.Errors.Select(failure => new ValidationError(
            code: failure.ErrorCode,
            message: failure.ErrorMessage,
            propertyName: failure.PropertyName,
            attemptedValue: failure.AttemptedValue
        )).ToArray();

        return Result<T>.Failure(errors);
    }

    /// <summary>
    /// Converts a FluentValidation ValidationResult to a non-generic Result, useful for validation-only
    /// operations where you don't need to carry a value forward in the pipeline. This enables
    /// Railway-Oriented Programming for validation scenarios that focus on success/failure without
    /// a return value.
    /// </summary>
    /// <param name="validationResult">The FluentValidation ValidationResult containing the validation outcome.</param>
    /// <returns>
    /// A Result indicating success if validation passes (happy path),
    /// or a failed Result with ValidationError objects if validation fails (error track).
    /// Each ValidationError includes the error code, message, property name, and attempted value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when validationResult is null.</exception>
    /// <example>
    /// <code>
    /// // Define a validator for a command
    /// public class DeleteUserCommandValidator : AbstractValidator&lt;DeleteUserCommand&gt;
    /// {
    ///     public DeleteUserCommandValidator()
    ///     {
    ///         RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
    ///         RuleFor(x => x.UserId).Must(id => id > 0).WithMessage("User ID must be positive");
    ///     }
    /// }
    ///
    /// // Use ToResult without a value for validation-only scenarios
    /// var command = new DeleteUserCommand { UserId = -1 };
    /// var validator = new DeleteUserCommandValidator();
    /// var validationResult = validator.Validate(command);
    ///
    /// Result result = validationResult.ToResult();
    ///
    /// return result.IsSuccess
    ///     ? await DeleteUserAsync(command.UserId)
    ///     : result; // Return validation errors on the error track
    /// </code>
    /// </example>
    public static Result ToResult(this ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        if (validationResult.IsValid)
        {
            return Result.Success();
        }

        var errors = validationResult.Errors.Select(failure => new ValidationError(
            code: failure.ErrorCode,
            message: failure.ErrorMessage,
            propertyName: failure.PropertyName,
            attemptedValue: failure.AttemptedValue
        )).ToArray();

        return Result.Failure(errors);
    }

    /// <summary>
    /// Validates an object using FluentValidation and returns a Result{T}, combining validation and
    /// Result pattern in a single operation. This is ideal for starting a Railway-Oriented Programming
    /// pipeline where validation is the entry point. The method automatically converts FluentValidation
    /// failures into ValidationError objects on the error track.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="validator">The FluentValidation IValidator instance that defines validation rules.</param>
    /// <param name="instance">The object instance to validate.</param>
    /// <returns>
    /// A Result{T} containing the validated instance if all rules pass (happy path),
    /// or a failed Result{T} with ValidationError objects if any rule fails (error track).
    /// The returned value on success is the same instance that was validated.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when validator or instance is null.</exception>
    /// <example>
    /// <code>
    /// // Define your validator
    /// public class ProductValidator : AbstractValidator&lt;Product&gt;
    /// {
    ///     public ProductValidator()
    ///     {
    ///         RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    ///         RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be positive");
    ///         RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    ///     }
    /// }
    ///
    /// // Use Validate to start a validation pipeline
    /// var product = new Product { Name = "Widget", Price = 29.99m, Stock = 100 };
    /// var validator = new ProductValidator();
    ///
    /// Result&lt;Product&gt; result = validator.Validate(product);
    ///
    /// // Continue the pipeline with Railway-Oriented Programming
    /// var saveResult = result
    ///     .Map(p => SaveToDatabase(p))
    ///     .Map(p => NotifyInventorySystem(p));
    ///
    /// // If validation fails, the pipeline short-circuits and errors flow through
    /// if (saveResult.IsFailure)
    /// {
    ///     return BadRequest(saveResult.Errors);
    /// }
    /// </code>
    /// </example>
    public static Result<T> Validate<T>(this IValidator<T> validator, T instance)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(instance);

        var validationResult = validator.Validate(instance);
        return validationResult.ToResult(instance);
    }

    /// <summary>
    /// Asynchronously validates an object using FluentValidation and returns a Result{T}. This is the
    /// async equivalent of the synchronous Validate method, perfect for starting async Railway-Oriented
    /// Programming pipelines. Use this when your validators contain async rules (e.g., database lookups,
    /// API calls) or when integrating with async application flows.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="validator">The FluentValidation IValidator instance that defines validation rules.</param>
    /// <param name="instance">The object instance to validate.</param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the async validation operation.
    /// Defaults to CancellationToken.None if not provided.
    /// </param>
    /// <returns>
    /// A Task that resolves to a Result{T} containing the validated instance if all rules pass (happy path),
    /// or a failed Result{T} with ValidationError objects if any rule fails (error track).
    /// The returned value on success is the same instance that was validated.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when validator or instance is null.</exception>
    /// <example>
    /// <code>
    /// // Define a validator with async rules
    /// public class RegisterUserValidator : AbstractValidator&lt;RegisterUserRequest&gt;
    /// {
    ///     private readonly IUserRepository _userRepository;
    ///
    ///     public RegisterUserValidator(IUserRepository userRepository)
    ///     {
    ///         _userRepository = userRepository;
    ///
    ///         RuleFor(x => x.Email)
    ///             .NotEmpty()
    ///             .EmailAddress()
    ///             .MustAsync(async (email, ct) => !await _userRepository.EmailExistsAsync(email, ct))
    ///             .WithMessage("Email already registered");
    ///
    ///         RuleFor(x => x.Username)
    ///             .NotEmpty()
    ///             .MinimumLength(3)
    ///             .MustAsync(async (username, ct) => await _userRepository.IsUsernameAvailableAsync(username, ct))
    ///             .WithMessage("Username already taken");
    ///     }
    /// }
    ///
    /// // Use ValidateAsync in an async pipeline
    /// public async Task&lt;Result&lt;User&gt;&gt; RegisterUserAsync(
    ///     RegisterUserRequest request,
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     var validator = new RegisterUserValidator(_userRepository);
    ///
    ///     // Start the async validation pipeline
    ///     return await validator.ValidateAsync(request, cancellationToken)
    ///         .BindAsync(req => CreateUserAsync(req, cancellationToken))
    ///         .TapAsync(user => SendWelcomeEmailAsync(user, cancellationToken));
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T>> ValidateAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(instance);

        var validationResult = await validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);
        return validationResult.ToResult(instance);
    }

    /// <summary>
    /// Validates the value within a Result{T} using FluentValidation, enabling Railway-Oriented Programming
    /// by chaining validation into an existing pipeline. This method respects the railway pattern: if the
    /// Result is already on the error track (IsFailure), it short-circuits and returns the original failure
    /// without executing validation. Only successful Results proceed to validation.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the Result.</typeparam>
    /// <param name="result">The Result{T} whose value should be validated if the Result is successful.</param>
    /// <param name="validator">The FluentValidation IValidator instance to apply to the Result's value.</param>
    /// <returns>
    /// The original Result{T} if it was already failed (preserving the error track),
    /// the original Result{T} if validation passes (staying on the happy path),
    /// or a new failed Result{T} with ValidationError objects if validation fails (switching to error track).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <example>
    /// <code>
    /// // Railway-Oriented Programming pipeline with validation
    /// public class UpdateProductService
    /// {
    ///     private readonly IProductRepository _repository;
    ///     private readonly IValidator&lt;Product&gt; _validator;
    ///
    ///     public Result&lt;Product&gt; UpdateProduct(int productId, UpdateProductRequest request)
    ///     {
    ///         // Build a pipeline where each step can fail
    ///         return Result&lt;int&gt;.Success(productId)
    ///             .Bind(id => _repository.GetById(id)) // May fail if not found
    ///             .Map(product => ApplyUpdates(product, request)) // Transform the product
    ///             .Validate(_validator) // Validate the updated product
    ///             .Bind(product => _repository.Save(product)); // Save if validation passed
    ///
    ///         // If any step fails (not found, validation, save error),
    ///         // the pipeline short-circuits and returns the failure.
    ///         // No need for nested if statements or try-catch blocks!
    ///     }
    ///
    ///     private Product ApplyUpdates(Product product, UpdateProductRequest request)
    ///     {
    ///         product.Name = request.Name;
    ///         product.Price = request.Price;
    ///         return product;
    ///     }
    /// }
    /// </code>
    /// </example>
    public static Result<T> Validate<T>(this Result<T> result, IValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (result.IsFailure)
        {
            return result;
        }

        var validationResult = validator.Validate(result.Value);

        if (validationResult.IsValid)
        {
            return result;
        }

        var errors = validationResult.Errors.Select(failure => new ValidationError(
            code: failure.ErrorCode,
            message: failure.ErrorMessage,
            propertyName: failure.PropertyName,
            attemptedValue: failure.AttemptedValue
        )).ToArray();

        return Result<T>.Failure(errors);
    }

    /// <summary>
    /// Asynchronously validates the value within a Result{T} using FluentValidation, enabling async
    /// Railway-Oriented Programming pipelines. This method respects the railway pattern: if the Result
    /// is already on the error track (IsFailure), it short-circuits and returns the original failure
    /// without executing validation. Use this when your validators contain async rules or when integrating
    /// into async pipelines.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the Result.</typeparam>
    /// <param name="result">The Result{T} whose value should be validated if the Result is successful.</param>
    /// <param name="validator">The FluentValidation IValidator instance to apply to the Result's value.</param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the async validation operation.
    /// Defaults to CancellationToken.None if not provided.
    /// </param>
    /// <returns>
    /// A Task that resolves to the original Result{T} if it was already failed (preserving the error track),
    /// the original Result{T} if validation passes (staying on the happy path),
    /// or a new failed Result{T} with ValidationError objects if validation fails (switching to error track).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <example>
    /// <code>
    /// // Async Railway-Oriented Programming pipeline with validation
    /// public class OrderProcessingService
    /// {
    ///     private readonly IOrderRepository _orderRepository;
    ///     private readonly IInventoryService _inventoryService;
    ///     private readonly IValidator&lt;Order&gt; _orderValidator;
    ///
    ///     public async Task&lt;Result&lt;Order&gt;&gt; ProcessOrderAsync(
    ///         CreateOrderRequest request,
    ///         CancellationToken cancellationToken = default)
    ///     {
    ///         // Build an async pipeline with validation in the middle
    ///         return await CreateOrderFromRequest(request)
    ///             .ValidateAsync(_orderValidator, cancellationToken) // Async validation
    ///             .BindAsync(order => ReserveInventoryAsync(order, cancellationToken))
    ///             .BindAsync(order => _orderRepository.SaveAsync(order, cancellationToken))
    ///             .TapAsync(order => SendConfirmationEmailAsync(order, cancellationToken));
    ///
    ///         // The pipeline stops at the first failure (validation or any other step)
    ///         // and propagates the error through the chain without exceptions
    ///     }
    ///
    ///     private Result&lt;Order&gt; CreateOrderFromRequest(CreateOrderRequest request)
    ///     {
    ///         // Create order from request
    ///         return Result&lt;Order&gt;.Success(new Order { /* ... */ });
    ///     }
    ///
    ///     private async Task&lt;Result&lt;Order&gt;&gt; ReserveInventoryAsync(
    ///         Order order,
    ///         CancellationToken cancellationToken)
    ///     {
    ///         // Reserve inventory for order items
    ///         return await _inventoryService.ReserveAsync(order.Items, cancellationToken);
    ///     }
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Result<T> result,
        IValidator<T> validator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (result.IsFailure)
        {
            return result;
        }

        var validationResult = await validator.ValidateAsync(result.Value, cancellationToken).ConfigureAwait(false);

        if (validationResult.IsValid)
        {
            return result;
        }

        var errors = validationResult.Errors.Select(failure => new ValidationError(
            code: failure.ErrorCode,
            message: failure.ErrorMessage,
            propertyName: failure.PropertyName,
            attemptedValue: failure.AttemptedValue
        )).ToArray();

        return Result<T>.Failure(errors);
    }

    /// <summary>
    /// Asynchronously validates a Task{Result{T}} using FluentValidation, enabling fluent async
    /// Railway-Oriented Programming pipelines. This extension allows you to chain validation directly
    /// on async operations without explicit awaits, creating cleaner pipeline syntax. The method awaits
    /// the task, then validates the Result's value if successful, short-circuiting if already failed.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the Result.</typeparam>
    /// <param name="resultTask">
    /// The Task{Result{T}} to validate. The task is awaited first, then validation is applied
    /// to the Result's value if the Result is successful.
    /// </param>
    /// <param name="validator">The FluentValidation IValidator instance to apply to the Result's value.</param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the async validation operation.
    /// Defaults to CancellationToken.None if not provided.
    /// </param>
    /// <returns>
    /// A Task that resolves to the original Result{T} if it was already failed (preserving the error track),
    /// the original Result{T} if validation passes (staying on the happy path),
    /// or a new failed Result{T} with ValidationError objects if validation fails (switching to error track).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <example>
    /// <code>
    /// // Fluent async pipeline without explicit awaits
    /// public class UserService
    /// {
    ///     private readonly IUserRepository _repository;
    ///     private readonly IValidator&lt;User&gt; _validator;
    ///     private readonly IEmailService _emailService;
    ///
    ///     public Task&lt;Result&lt;User&gt;&gt; UpdateUserAsync(
    ///         int userId,
    ///         UpdateUserRequest request,
    ///         CancellationToken cancellationToken = default)
    ///     {
    ///         // Beautiful fluent pipeline - no intermediate awaits needed!
    ///         return _repository.GetByIdAsync(userId, cancellationToken)
    ///             .MapAsync(user => ApplyUpdates(user, request))
    ///             .ValidateAsync(_validator, cancellationToken) // Validate after async operation
    ///             .BindAsync(user => _repository.SaveAsync(user, cancellationToken))
    ///             .TapAsync(user => _emailService.SendUpdateNotificationAsync(user, cancellationToken));
    ///
    ///         // Each method returns Task&lt;Result&lt;T&gt;&gt;, and ValidateAsync handles the unwrapping
    ///         // This creates a clean, readable pipeline that handles both async and validation
    ///     }
    ///
    ///     private User ApplyUpdates(User user, UpdateUserRequest request)
    ///     {
    ///         user.Name = request.Name;
    ///         user.Email = request.Email;
    ///         return user;
    ///     }
    /// }
    ///
    /// // Compare with traditional approach (much more verbose):
    /// public async Task&lt;Result&lt;User&gt;&gt; UpdateUserTraditionalAsync(
    ///     int userId,
    ///     UpdateUserRequest request,
    ///     CancellationToken cancellationToken = default)
    /// {
    ///     var getUserResult = await _repository.GetByIdAsync(userId, cancellationToken);
    ///     if (getUserResult.IsFailure) return getUserResult;
    ///
    ///     var user = ApplyUpdates(getUserResult.Value, request);
    ///
    ///     var validationResult = await _validator.ValidateAsync(user, cancellationToken);
    ///     if (!validationResult.IsValid) return validationResult.ToResult(user);
    ///
    ///     var saveResult = await _repository.SaveAsync(user, cancellationToken);
    ///     if (saveResult.IsFailure) return saveResult;
    ///
    ///     await _emailService.SendUpdateNotificationAsync(user, cancellationToken);
    ///     return saveResult;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T>> ValidateAsync<T>(
        this Task<Result<T>> resultTask,
        IValidator<T> validator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var result = await resultTask.ConfigureAwait(false);
        return await result.ValidateAsync(validator, cancellationToken).ConfigureAwait(false);
    }
}
