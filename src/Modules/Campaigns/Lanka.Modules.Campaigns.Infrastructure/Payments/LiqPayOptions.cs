namespace Lanka.Modules.Campaigns.Infrastructure.Payments;

public sealed class LiqPayOptions
{
    public string PublicKey { get; init; } = string.Empty;

    public string PrivateKey { get; init; } = string.Empty;

    public string ServerUrl { get; init; } = string.Empty;

    public string ResultUrl { get; init; } = string.Empty;
}
