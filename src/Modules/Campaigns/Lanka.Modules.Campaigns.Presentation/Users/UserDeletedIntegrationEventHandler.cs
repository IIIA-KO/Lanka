using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Bloggers.Delete;
using Lanka.Modules.Users.IntegrationEvents;
using MediatR;

namespace Lanka.Modules.Campaigns.Presentation.Users;

internal sealed class UserDeletedIntegrationEventHandler
    : IntegrationEventHandler<UserDeletedIntegrationEvent>
{
    private readonly ISender _sender;

    public UserDeletedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        UserDeletedIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._sender.Send(new DeleteBloggerCommand(integrationEvent.UserId), cancellationToken);
        
        if (result.IsFailure)
        {
            throw new LankaException(nameof(DeleteBloggerCommand), result.Error);
        }
    }
}
