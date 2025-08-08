namespace Lanka.Gateway.Cors;

public sealed class CorsOptions
{
    public const string PolicyName = "LankaCorsPolicy";

    public const string SectionName = "Cors";

    public required string[] AllowedOrigins { get; init; }
}
