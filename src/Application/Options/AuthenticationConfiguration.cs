namespace Application.Options;

public record AuthenticationConfiguration
{
    public required AuthenticationOptions AuthenticationOptions { get; set; }

    public required GoogleAuthenticationOptions GoogleAuthenticationOptions { get; set; }
}
