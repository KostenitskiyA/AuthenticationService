namespace Application.Options;

public record AuthenticationOptions
{
    public required string Key { get; set; }

    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    public required int TokenExpiresInMinutes { get; set; }

    public required int RefreshTokenExpiresInMinutes { get; set; }
}
