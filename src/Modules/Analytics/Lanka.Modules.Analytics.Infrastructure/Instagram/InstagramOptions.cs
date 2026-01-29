namespace Lanka.Modules.Analytics.Infrastructure.Instagram;

public sealed class InstagramOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string RenewRedirectUri { get; set; } = string.Empty;

    public string UserInfoUrl { get; set; } = string.Empty;

    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }

    public int RenewalThresholdInDays { get; init; }
}

/// <summary>
/// Development-only options for controlling Instagram API behavior.
/// Only available in Development environment.
/// </summary>
public sealed class InstagramDevelopmentOptions
{
    /// <summary>
    /// List of User IDs allowed to use real Instagram API in development mode.
    /// All other users will use mock services.
    /// </summary>
    public List<Guid> AllowedUserIds { get; set; } = [];
}
