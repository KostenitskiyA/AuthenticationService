namespace Application.Options;

public record GoogleAuthenticationOptions
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}
