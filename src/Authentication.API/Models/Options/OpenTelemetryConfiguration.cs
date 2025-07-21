namespace Authentication.API.Models.Options;

public record OpenTelemetryConfiguration
{
    public required string ServiceName { get; init; }

    public required string LokiUrl { get; init; }

    public required string OtelUrl { get; init; }
}