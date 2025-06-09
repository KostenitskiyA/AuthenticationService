namespace Authentication.API.Models.Options;

public record GoogleAuthenticationOptions
{
    public required string TokenName { get; init; }
    
    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }
}