using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.IntegrationEvents.Bloggers;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Application.Users.Update;
using MediatR;

namespace Lanka.Modules.Users.Presentation;

internal sealed class BloggerUpdatedIntegrationEventHandler
    : IntegrationEventHandler<BloggerUpdatedIntegrationEvent>
{
    private readonly ISender _sender;

    public BloggerUpdatedIntegrationEventHandler(ISender sender)
    {
        this._sender = sender;
    }

    public override async Task Handle(
        BloggerUpdatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<UserResponse> result = await this._sender.Send(
            new UpdateUserCommand(
                integrationEvent.BloggerId,
                integrationEvent.FirstName,
                integrationEvent.LastName,
                integrationEvent.BirthDate
            ),
            cancellationToken
        );
        
        if (result.IsFailure)
        {
            throw new LankaException(nameof(UpdateUserCommand), result.Error);
        }
    }
}
