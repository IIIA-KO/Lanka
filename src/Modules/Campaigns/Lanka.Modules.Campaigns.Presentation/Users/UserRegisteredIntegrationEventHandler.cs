using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Bloggers.Create;
using Lanka.Modules.Users.IntegrationEvents;
using MediatR;

namespace Lanka.Modules.Campaigns.Presentation.Users;

internal sealed class UserRegisteredIntegrationEventHandler
    : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly ISender _sender;

    public UserRegisteredIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(
            new CreateBloggerCommand(
                integrationEvent.UserId,
                integrationEvent.Email,
                integrationEvent.FirstName,
                integrationEvent.LastName,
                integrationEvent.BirthDate
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new LankaException(nameof(CreateBloggerCommand), result.Error);
        }
    }
}
