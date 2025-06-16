using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Instagram.RefreshToken;
using Lanka.Modules.Users.IntegrationEvents.RenewInstagramAccess;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Users;

internal sealed class InstagramAccessRenewalStartedIntegrationEventHandler
    : IntegrationEventHandler<InstagramAccessRenewalStartedIntegrationEvent>
{
    private readonly ISender _sender;

    public InstagramAccessRenewalStartedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        InstagramAccessRenewalStartedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new RefreshInstagramTokenCommand(
                integrationEvent.UserId,
                integrationEvent.Code
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(RefreshInstagramTokenCommand), result.Error);
        }
    }
}
