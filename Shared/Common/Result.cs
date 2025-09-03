using System.ComponentModel.DataAnnotations;

namespace Shared.Common;

/// <summary>
/// Result pattern implementation pentru handling errors f?r? exceptions
/// Conform planului de refactoring - CRITICAL PRIORITY
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public List<string> Errors { get; protected set; } = new();
    public string? SuccessMessage { get; protected set; }

    protected Result(bool isSuccess, IEnumerable<string>? errors = null, string? successMessage = null)
    {
        IsSuccess = isSuccess;
        Errors = errors?.ToList() ?? new List<string>();
        SuccessMessage = successMessage;
    }

    public static Result Success(string? message = null)
        => new(true, null, message);

    public static Result Failure(params string[] errors)
        => new(false, errors);

    public static Result Failure(IEnumerable<string> errors)
        => new(false, errors);

    public static Result Failure(string error)
        => new(false, new[] { error });

    // Implicit conversion pentru compatibilitate
    public static implicit operator bool(Result result) => result.IsSuccess;
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(bool isSuccess, T? value, IEnumerable<string>? errors = null, string? successMessage = null)
        : base(isSuccess, errors, successMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value, string? message = null)
        => new(true, value, null, message);

    public static Result<T> Failure(params string[] errors)
        => new(false, default(T), errors);

    public static Result<T> Failure(IEnumerable<string> errors)
        => new(false, default(T), errors);

    public static Result<T> Failure(string error)
        => new(false, default(T), new[] { error });

    // Conversion methods
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        if (!IsSuccess)
            return Result<TOut>.Failure(Errors);

        try
        {
            var mappedValue = mapper(Value!);
            return Result<TOut>.Success(mappedValue, SuccessMessage);
        }
        catch (Exception ex)
        {
            return Result<TOut>.Failure($"Mapping failed: {ex.Message}");
        }
    }

    public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> mapper)
    {
        if (!IsSuccess)
            return Result<TOut>.Failure(Errors);

        try
        {
            var mappedValue = await mapper(Value!);
            return Result<TOut>.Success(mappedValue, SuccessMessage);
        }
        catch (Exception ex)
        {
            return Result<TOut>.Failure($"Async mapping failed: {ex.Message}");
        }
    }

    // Validation helper
    public static Result<T> FromValidation<TModel>(TModel model, Func<TModel, T> factory)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model!);

        if (!Validator.TryValidateObject(model!, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(vr => vr.ErrorMessage ?? "Unknown validation error").ToList();
            return Result<T>.Failure(errors);
        }

        try
        {
            var value = factory(model);
            return Result<T>.Success(value);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Object creation failed: {ex.Message}");
        }
    }

    // Chain multiple operations
    public Result<TOut> Then<TOut>(Func<T, Result<TOut>> next)
    {
        if (!IsSuccess)
            return Result<TOut>.Failure(Errors);

        return next(Value!);
    }

    // Chain multiple async operations
    public async Task<Result<TOut>> ThenAsync<TOut>(Func<T, Task<Result<TOut>>> next)
    {
        if (!IsSuccess)
            return Result<TOut>.Failure(Errors);

        return await next(Value!);
    }

    // Convert to value or default
    public T ValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? Value! : defaultValue;
    }

    // Implicit conversion
    public static implicit operator bool(Result<T> result) => result.IsSuccess;
}

/// <summary>
/// Helper pentru combinarea mai multor rezultate
/// </summary>
public static class ResultHelpers
{
    public static Result Combine(params Result[] results)
    {
        var allErrors = results.Where(r => !r.IsSuccess)
                              .SelectMany(r => r.Errors)
                              .ToList();

        if (allErrors.Any())
            return Result.Failure(allErrors);

        var successMessages = results.Where(r => r.IsSuccess && !string.IsNullOrEmpty(r.SuccessMessage))
                                   .Select(r => r.SuccessMessage!)
                                   .ToList();

        var combinedMessage = successMessages.Any() ? string.Join("; ", successMessages) : null;
        return Result.Success(combinedMessage);
    }

    public static async Task<Result<T>> TryCatchAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            var result = await operation();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Operation failed: {ex.Message}");
        }
    }

    public static Result<T> TryCatch<T>(Func<T> operation)
    {
        try
        {
            var result = operation();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Operation failed: {ex.Message}");
        }
    }

    public static async Task<Result> TryCatchAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Operation failed: {ex.Message}");
        }
    }

    public static Result TryCatch(Action operation)
    {
        try
        {
            operation();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Operation failed: {ex.Message}");
        }
    }
}