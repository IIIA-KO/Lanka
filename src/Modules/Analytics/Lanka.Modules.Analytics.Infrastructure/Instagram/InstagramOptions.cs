namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

public sealed class InstagramOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string RenewRedirectUri { get; set; } = string.Empty;

    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }

    public int RenewalThresholdInDays { get; init; }
}

public sealed class InstagramDevelopmentOptions
{
    public IReadOnlyList<string> AllowedUserEmails { get; init; } = [];
}
