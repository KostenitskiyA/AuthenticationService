namespace API.Models;

public sealed record ValidationError(string Field, string Message);