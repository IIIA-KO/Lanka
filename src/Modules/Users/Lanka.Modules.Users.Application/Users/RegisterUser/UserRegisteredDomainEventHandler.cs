using Lanka.Common.Application.EventBus;
using Lanka.Common.Application.Exceptions;
using Lanka.Common.Application.Messaging;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Users.GetUser;
using Lanka.Modules.Users.Domain.Users.DomainEvents;
using Lanka.Modules.Users.IntegrationEvents;
using MediatR;

namespace Lanka.Modules.Users.Application.Users.RegisterUser;

internal sealed class UserRegisteredDomainEventHandler
    : DomainEventHandler<UserCreatedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IEventBus _eventBus;

    public UserRegisteredDomainEventHandler(ISender sender, IEventBus eventBus)
    {
        this._sender = sender;
        this._eventBus = eventBus;
    }

    public override async Task Handle(
        UserCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<UserResponse> result =
            await this._sender.Send(new GetUserQuery(domainEvent.UserId.Value), cancellationToken);

        if (result.IsFailure)
        {
            throw new LankaException(nameof(GetUserQuery), result.Error);
        }

        await this._eventBus.PublishAsync(
            new UserRegisteredIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                result.Value.Id,
                result.Value.Email,
                result.Value.FirstName,
                result.Value.LastName,
                result.Value.BirthDay
            ),
            cancellationToken
        );
    }
}
