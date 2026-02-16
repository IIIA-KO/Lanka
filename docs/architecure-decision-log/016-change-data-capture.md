# ADL 016 - Change Data Capture for Elasticsearch Sync

**Date:** 2026-02-16
**Status:** Accepted

## Context

Lanka's Matching module provides full-text search via Elasticsearch. To keep the index in sync with PostgreSQL, the project used **manually raised domain events** in entity methods. Each Create/Update/Delete method had to call `RaiseDomainEvent(new *SearchSyncDomainEvent(...))`. Per-entity domain event handlers then queried the database for full entity data, constructed integration events, and published them to RabbitMQ. The Matching module consumed these events with per-entity handlers.

This approach had several problems:

1. **Fragile**: If a developer forgot to raise the event in a new entity method, the search index would silently go out of sync. There was no compile-time or runtime safety net.
2. **Verbose**: 20 search sync domain event handlers, 6 per-entity integration events, and 6 per-entity integration event handlers — all with nearly identical boilerplate.
3. **Redundant DB queries**: Each domain event handler re-queried the database for entity data that was already available at the point of the change.
4. **Inconsistency risk**: Domain events carried minimal data (just IDs), so the handler's DB query could return data that had already been modified by a subsequent operation within the same request.

## Decision

Replace manual domain events with **automatic Change Data Capture (CDC) via EF Core's `SaveChangesInterceptor`**. Entities opt in with an empty marker interface `IChangeCaptured`. Per-module interceptors in the Infrastructure layer define how to extract search-relevant data from each entity type.

The approach:

1. **`IChangeCaptured`** — Empty marker interface in Common.Domain. Entities implement it to opt in to automatic change detection.
2. **`ChangeCaptureInterceptorBase`** — Abstract `SaveChangesInterceptor` in Common.Infrastructure. Scans the EF ChangeTracker for `IChangeCaptured` entities in Added/Modified/Deleted state.
3. **Per-module interceptors** — Concrete implementations (e.g., `CampaignsChangeCaptureInterceptor`) that define entity → search data mapping via pattern matching.
4. **Single domain event handler per module** — Converts `EntityChangeCapturedDomainEvent` to `SearchSyncIntegrationEvent` and publishes to RabbitMQ.
5. **Single integration event handler in Matching** — Replaces 6 per-entity handlers.

Search data is extracted directly from the entity at `SaveChanges` time — no extra DB queries needed.

## Alternatives Considered

### Debezium / PostgreSQL Logical Replication
- **Pros**: True CDC from the Write-Ahead Log — captures all changes including raw SQL, migrations, Dapper writes
- **Cons**: Requires Kafka + Debezium connector. Heavy infrastructure overhead for a learning project. Complex to set up and debug.
- **Why rejected**: Overkill. Dapper is used only for read queries and infrastructure tables (outbox/inbox), so all entity changes go through EF Core.

### PostgreSQL Triggers + `pg_notify`
- **Pros**: Database-level — catches all changes regardless of application layer
- **Cons**: Business logic (what to index, how to format) moves to SQL. Harder to maintain and test. Tighter coupling to PostgreSQL.
- **Why rejected**: Moves logic to a less maintainable layer. Entity-to-search-data mapping is complex enough to warrant C# code.

### Keep Manual Domain Events (Status Quo)
- **Pros**: No changes needed. Developers explicitly control when sync happens.
- **Cons**: The root problem remains — forgetting to raise an event silently breaks search. The 20+ handler files are pure boilerplate.
- **Why rejected**: The reliability problem was the motivation for this decision.

## Consequences

### Positive

- **Impossible to forget**: Any `SaveChanges` on an `IChangeCaptured` entity automatically generates a sync event. No manual step required.
- **Massive boilerplate reduction**: ~30 files deleted (20 domain event handlers, 6 integration events, 6 integration event handlers), replaced by ~6 new files.
- **No extra DB queries**: Entity data is extracted directly from the ChangeTracker at `SaveChanges` time, eliminating the re-query pattern.
- **Domain stays clean**: `IChangeCaptured` is an empty marker — the entity has zero knowledge of search. All mapping logic lives in Infrastructure.
- **Consistent data**: Search data reflects the exact state being persisted, not a potentially stale re-query.

### Negative

- **Only catches EF changes**: Raw SQL or Dapper writes won't trigger CDC. Acceptable because entity writes always go through EF in this project.
- **Modified = always synced**: Any EF-tracked modification to an `IChangeCaptured` entity triggers a sync, even if search-irrelevant fields changed. Elasticsearch's upsert handles this idempotently, so the cost is a redundant no-op update.

### Neutral

- **Existing outbox/inbox infrastructure reused**: The interceptor writes directly to the outbox table. No schema changes or new tables.
- **Integration event base class kept**: `SearchSyncIntegrationEvent` remains unchanged. Only the concrete per-entity subclasses are removed.

## Implementation Notes

### Key Files

| File | Purpose |
|------|---------|
| `Common.Domain/IChangeCaptured.cs` | Empty marker interface |
| `Common.Domain/EntityChangeCapturedDomainEvent.cs` | Generic domain event with entity ID, item type, operation, and search data |
| `Common.Infrastructure/ChangeCapture/ChangeCaptureInterceptorBase.cs` | Abstract interceptor: ChangeTracker scanning + OutboxMessage creation |
| `Common.Infrastructure/ChangeCapture/CapturedChangeData.cs` | Data record for extracted search data |
| `Campaigns.Infrastructure/ChangeCapture/CampaignsChangeCaptureInterceptor.cs` | Blogger/Campaign/Offer/Pact/Review mapping |
| `Analytics.Infrastructure/ChangeCapture/AnalyticsChangeCaptureInterceptor.cs` | InstagramAccount mapping |

### Interceptor Design

The interceptor writes `OutboxMessage` entries directly to the DbContext (same pattern as `InsertOutboxMessagesInterceptor`) rather than using `Entity.RaiseDomainEvent()`, which is `protected`. This means interceptor registration order is irrelevant.

### Adding a New Searchable Entity

1. Add `IChangeCaptured` to the entity class declaration
2. Add a pattern match case in the module's change capture interceptor

Two changes, both compile-time verifiable (missing pattern match = warning).

## Related Decisions

- [008 - Event-Driven Architecture](008-event-driven-architecture.md): CDC builds on the existing domain event → integration event pattern
- [011 - Outbox & Inbox Pattern](011-messagins-with-outbox&inbox.md): CDC reuses the outbox for reliable delivery

## References

- [Change Data Capture (Martin Fowler)](https://www.martinfowler.com/articles/patterns-legacy-displacement/event-interception.html#ChangeDataCapturecdc)
- [EF Core Interceptors](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
