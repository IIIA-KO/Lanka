namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.UpdateAccount;

internal sealed record TokenResponse(
    Guid Id,
    Guid UserId,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc
)
{
    public TokenResponse()
        : this(Guid.Empty, Guid.Empty, string.Empty, DateTimeOffset.MinValue)
    {
    }
}
