namespace Lanka.Modules.Analytics.Application.Abstractions.Audience;

public interface IAudienceSummaryProvider
{
    Task<AudienceSummary?> GetSummaryAsync(Guid instagramAccountId, CancellationToken cancellationToken = default);
}
