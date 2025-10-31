namespace API.Models;

public sealed record Result(string TraceId, bool Success, object? Data, Error? Error);
