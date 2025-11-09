namespace Application.Models;

public class Result<T>
{
    private Result(T? value, bool isSuccess, string? errorMessage)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public T? Value { get; }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? ErrorMessage { get; }

    public static Result<T> Success(T value) => new(value, true, null);

    public static Result<T> Failure(string errorMessage) => new(default, false, errorMessage);

    public static implicit operator Result<T>(T value) => Success(value);
}

public class Result
{
    private Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(string errorMessage) => new(false, errorMessage);
}

