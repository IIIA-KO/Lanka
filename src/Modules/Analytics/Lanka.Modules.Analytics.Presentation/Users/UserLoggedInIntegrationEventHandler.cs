using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.BloggerActivity.TrackUserLogin;
using Lanka.Modules.Users.IntegrationEvents;
using MediatR;

namespace Lanka.Modules.Analytics.Presentation.Users;

internal sealed class UserLoggedInIntegrationEventHandler
    : IntegrationEventHandler<UserLoggedInIntegrationEvent>
{
    private readonly ISender _sender;

    public UserLoggedInIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        UserLoggedInIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new TrackUserLoginCommand(
                integrationEvent.UserId,
                integrationEvent.LastLoggedInAtUtc
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(TrackUserLoginCommand), result.Error);
        }
    }
}
