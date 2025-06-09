namespace Authentication.API.Models.Options;

public record AuthenticationOptions
{
    public required string TokenName { get; init; }

    public required string Key { get; init; }

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required int ExpiresInMinutes { get; init; }
}