# Inbox Pattern

**Category:** Messaging

## Definition
A reliability pattern that stores incoming messages in a database before processing them. Combined with idempotency mechanisms, it ensures exactly-once processing semantics in distributed systems.

## Purpose
- Prevents message loss when received from external sources
- Enables retrying failed message processing
- Provides idempotency to avoid duplicate processing
- Creates an audit trail of received messages

## Implementation in Lanka
The Inbox pattern is implemented with these components:
- `InboxMessage` entity for storing integration events
- `IntegrationEventConsumer` for capturing incoming events from the message broker
- `ProcessInboxJob` for processing stored events asynchronously
- `IdempotentIntegrationEventHandler` for ensuring events are processed exactly once