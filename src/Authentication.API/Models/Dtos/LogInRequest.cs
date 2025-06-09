namespace Authentication.API.Models.Dtos;

public record LogInRequest
{
    public required string Email { get; init; }
    
    public required string Password { get; init; }
}