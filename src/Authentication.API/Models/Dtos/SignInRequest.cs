namespace Authentication.API.Models.Dtos;

public record SignInRequest
{
    public required string Name { get; init; }

    public required string Email { get; init; }

    public string? Password { get; init; }
}