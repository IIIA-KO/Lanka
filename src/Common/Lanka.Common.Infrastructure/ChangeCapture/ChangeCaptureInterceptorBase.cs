using Lanka.Common.Domain;
using Lanka.Common.Infrastructure.Outbox;
using Lanka.Common.Infrastructure.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace Lanka.Common.Infrastructure.ChangeCapture;

public abstract class ChangeCaptureInterceptorBase : SaveChangesInterceptor
{
    private const int OperationCreate = 1;
    private const int OperationUpdate = 2;
    private const int OperationDelete = 3;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            this.InsertChangeCaptureMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void InsertChangeCaptureMessages(DbContext context)
    {
        List<OutboxMessage> messages = [];

        foreach (EntityEntry<IChangeCaptured> entry in context.ChangeTracker.Entries<IChangeCaptured>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            if (entry.Entity is not IEntity)
            {
                continue;
            }

            int operation = entry.State switch
            {
                EntityState.Added => OperationCreate,
                EntityState.Modified => OperationUpdate,
                EntityState.Deleted => OperationDelete,
                _ => throw new InvalidOperationException($"Unexpected entity state: {entry.State}")
            };

            Guid entityId = GetEntityId(entry);

            EntityChangeCapturedDomainEvent domainEvent;

            if (operation == OperationDelete)
            {
                string? itemType = this.GetItemType(entry.Entity);

                if (itemType is null)
                {
                    continue;
                }

                domainEvent = new EntityChangeCapturedDomainEvent(entityId, itemType, operation);
            }
            else
            {
                CapturedChangeData? data = this.ExtractCapturedData(entry.Entity);

                if (data is null)
                {
                    continue;
                }

                domainEvent = new EntityChangeCapturedDomainEvent(
                    entityId,
                    data.ItemType,
                    operation,
                    data.Title,
                    data.Content,
                    data.Tags.ToArray(),
                    data.Metadata);
            }

            var outboxMessage = new OutboxMessage(
                domainEvent.Id,
                domainEvent.GetType().Name,
                JsonConvert.SerializeObject(domainEvent, SerializerSettings.Instance),
                domainEvent.OccurredOnUtc);

            messages.Add(outboxMessage);
        }

        if (messages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(messages);
        }
    }

    private static Guid GetEntityId(EntityEntry entry)
    {
        object? idProperty = entry.Property("Id").CurrentValue;

        return idProperty switch
        {
            TypedEntityId typedId => typedId.Value,
            Guid guid => guid,
            _ => throw new InvalidOperationException(
                $"Cannot extract entity ID from {entry.Entity.GetType().Name}. " +
#pragma warning disable CA1508
                $"Expected TypedEntityId or Guid, got {idProperty?.GetType().Name ?? "null"}.")
#pragma warning restore CA1508
        };
    }

    /// <summary>
    /// Extract search data from the entity. Return null to skip this entity.
    /// Called for Create and Update operations.
    /// </summary>
    protected abstract CapturedChangeData? ExtractCapturedData(IChangeCaptured entity);

    /// <summary>
    /// Get the item type string for delete operations (when ExtractCapturedData is not called).
    /// Return null to skip this entity.
    /// </summary>
    protected abstract string? GetItemType(IChangeCaptured entity);
}
