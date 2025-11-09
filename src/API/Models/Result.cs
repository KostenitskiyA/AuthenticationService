namespace API.Models;

public sealed record Result
{
    private Result(string traceId, bool isSuccess, Error? error)
    {
        TraceId = traceId;
        IsSuccess = isSuccess;
        Error = error;
    }

    public string TraceId { get; init; }

    public bool IsSuccess { get; init; }

    public Error? Error { get; init; }

    public static Result Success(string traceId) => new(traceId, true, null);

    public static Result Failure(string traceId, Error error) => new(traceId, false, error);
}

public sealed record Result<T>
{
    private Result(string traceId, bool isSuccess, T? data, Error? error)
    {
        TraceId = traceId;
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public string TraceId { get; init; }

    public bool IsSuccess { get; init; }

    public T? Data { get; init; }

    public Error? Error { get; init; }

    public static Result<T> Success(string traceId, T data) => new(traceId, true, data, null);

    public static Result<T> Failure(string traceId, Error error) => new(traceId, false, default, error);
}
