# Outbox Pattern

**Category:** Messaging

## Definition
A reliability pattern that stores messages (typically domain events) in a database table within the same transaction as the business operation. These messages are later processed by a background job that publishes them to the messaging system.

## Purpose
- Ensures messages are not lost when the primary database transaction succeeds
- Provides atomic operations between data changes and message publishing
- Enables retrying failed message publications

## Implementation in Lanka
The Outbox pattern is implemented with these components:
- `OutboxMessage` entity for storing domain events
- `InsertOutboxMessagesInterceptor` for capturing domain events during EF Core saves
- `ProcessOutboxJob` for publishing stored events asynchronously
- `IdempotentDomainEventHandler` for ensuring events are processed exactly once