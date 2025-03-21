using MediatR;

namespace Lanka.Common.Domain;

public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTime OcurredOnUtc { get; }
}
