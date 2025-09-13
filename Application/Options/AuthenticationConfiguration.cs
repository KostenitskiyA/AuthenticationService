namespace Application.Options;

public record AuthenticationConfiguration
{
    public required AuthenticationOptions AuthenticationOptions { get; init; }

    public required GoogleAuthenticationOptions GoogleAuthenticationOptions { get; init; }
}