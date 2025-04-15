using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Bloggers.UpdateBlogger;
using Lanka.Modules.Users.IntegrationEvents;
using MediatR;

namespace Lanka.Modules.Campaigns.Presentation.Bloggers;

internal sealed class UserUpdatedIntegrationEventHandler
    : IntegrationEventHandler<UserUpdatedIntegrationEvent>
{
    private readonly ISender _sender;

    public UserUpdatedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        UserUpdatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new UpdateBloggerCommand(
                integrationEvent.UserId,
                integrationEvent.FirstName,
                integrationEvent.LastName,
                integrationEvent.BirthDate
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(UpdateBloggerCommand), result.Error);
        }
    }
}
