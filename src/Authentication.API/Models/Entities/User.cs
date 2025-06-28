namespace Authentication.API.Models.Entities;

public record User
{
    public Guid Id { get; set; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }
}