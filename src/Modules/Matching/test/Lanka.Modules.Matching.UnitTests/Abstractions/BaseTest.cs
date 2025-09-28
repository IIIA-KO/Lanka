using Bogus;
using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.UnitTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest // Consider making public types internal
{
    public static readonly Faker Faker = new();

    protected static T AssertDomainEventWasPublished<T>(IEntity entity)
        where T : IDomainEvent
    {
        T? domainEvent = entity.DomainEvents.OfType<T>().SingleOrDefault() ??
                         throw new Exception($"{typeof(T).Name} was not published");

        return domainEvent;
    }
}
