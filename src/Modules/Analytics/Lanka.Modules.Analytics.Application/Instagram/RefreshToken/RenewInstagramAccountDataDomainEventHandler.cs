using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Messaging;
using Lanka.Modules.Analytics.Application.Abstractions.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.IntegrationEvents;

namespace Lanka.Modules.Analytics.Application.Instagram.RefreshToken;

internal sealed class RenewInstagramAccountDataDomainEventHandler
    : DomainEventHandler<InstagramAccountDataRenewedDomainEvent>
{
    private readonly IEventBus _eventBus;
    private readonly IAudienceSummaryProvider _audienceSummaryProvider;

    public RenewInstagramAccountDataDomainEventHandler(
        IEventBus eventBus,
        IAudienceSummaryProvider audienceSummaryProvider)
    {
        this._eventBus = eventBus;
        this._audienceSummaryProvider = audienceSummaryProvider;
    }

    public override async Task Handle(
        InstagramAccountDataRenewedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        AudienceSummary? summary = await this._audienceSummaryProvider.GetSummaryAsync(
            domainEvent.InstagramAccountId.Value, cancellationToken);

        await this._eventBus.PublishAsync(
            new InstagramAccountDataRenewedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId.Value,
                domainEvent.Username,
                domainEvent.FollowersCount,
                domainEvent.MediaCount,
                domainEvent.ProviderId,
                summary?.EngagementRate,
                summary?.TopAgeGroup,
                summary?.TopGender,
                summary?.TopGenderPercentage,
                summary?.TopCountry,
                summary?.TopCountryPercentage
            ),
            cancellationToken
        );
    }
}
