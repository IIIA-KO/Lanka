using Lanka.Common.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Lanka.Common.Infrastructure.Interceptors
{
    public sealed class PublishDomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PublishDomainEventsInterceptor(IServiceScopeFactory serviceScopeFactory)
        {
            this._serviceScopeFactory = serviceScopeFactory;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(eventData);

            if (eventData.Context is not null)
            {
                await this.PublishDomainEventsAsync(eventData.Context);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private async Task PublishDomainEventsAsync(DbContext context)
        {
            var domainEvents = context
                .ChangeTracker.Entries<IEntity>()
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    IReadOnlyCollection<IDomainEvent> domainEvents = entity.GetDomainEvents();

                    entity.ClearDomainEvents();

                    return domainEvents;
                })
                .ToList();

            using IServiceScope scope = this._serviceScopeFactory.CreateScope();

            IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            foreach (IDomainEvent domainEvent in domainEvents)
            {
                await publisher.Publish(domainEvent);
            }
        }
    }
}
