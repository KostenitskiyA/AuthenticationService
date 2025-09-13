namespace Application.Options;

public record GoogleAuthenticationOptions
{
    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }
}