using System.Text.Json.Serialization;

namespace Shared.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? SuccessMessage { get; set; }

    // Constructor pentru serializare JSON
    [JsonConstructor]
    public Result()
    {
        Errors = new List<string>();
    }

    protected Result(bool isSuccess, T? value, List<string> errors, string? successMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? new List<string>();
        SuccessMessage = successMessage;
    }

    public static Result<T> Success(T value, string? message = null)
        => new(true, value, new List<string>(), message);

    public static Result<T> Failure(params string[] errors)
        => new(false, default, errors.ToList(), null);

    public static Result<T> Failure(List<string> errors)
        => new(false, default, errors, null);
}

public class Result : Result<object>
{
    // Constructor pentru serializare JSON
    [JsonConstructor]
    public Result() : base()
    {
    }

    protected Result(bool isSuccess, List<string> errors, string? successMessage)
        : base(isSuccess, null, errors, successMessage)
    {
    }

    public static Result Success(string? message = null)
        => new(true, new List<string>(), message);

    public static new Result Failure(params string[] errors)
        => new(false, errors.ToList(), null);

    public static new Result Failure(List<string> errors)
        => new(false, errors, null);
}