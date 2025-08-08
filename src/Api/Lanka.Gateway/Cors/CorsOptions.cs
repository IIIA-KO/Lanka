namespace Lanka.Gateway.Cors;

internal sealed class CorsOptions
{
    public const string PolicyName = "LankaCorsPolicy";

    public const string SectionName = "Cors";

    public required string[] AllowedOrigins { get; init; }
}
