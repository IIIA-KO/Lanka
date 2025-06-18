# ADL 013 - Adoption of Saga Pattern for Cross-Module Orchestration

**Date:** 2025-05-15  
**Status:** Accepted

## Context

As Lanka evolves into a modular monolith with strictly bounded modules such as Users, Analytics, and Campaigns, we encountered a problem where certain workflows span multiple modules and require coordinated side effects. One such example is the Instagram account linking process:

- A user initiates Instagram account linking via the Users module.
- The Analytics module is responsible for communicating with Instagram's APIs to fetch token and profile data.
- Keycloak requires a known username and provider ID to finalize external identity linking.

This led to a handshake problem: the Users module needed to perform an operation (Keycloak link) that depended on data only available via Analytics. Direct calls across modules would violate the modular architecture.

## Decision

We introduced the Saga orchestration pattern, specifically using MassTransit state machines, to coordinate long-running workflows across module boundaries.

- A saga instance was created per Instagram linking request, tracked by a LinkInstagramSaga.
- The saga receives InstagramAccountLinkedIntegrationEvent (from Users module).
- It then publishes a InstagramAccountLinkingStartedIntegrationEvent.
- The Analytics module reacts, fetches token + metadata, stores it, and emits InstagramAccountDataFetchedIntegrationEvent.
- The Users module listens for that and finalizes the Keycloak link.
- When all required messages are handled, the saga finalizes and emits InstagramAccountLinkingCompletedIntegrationEvent.

## Consequences

- Clear boundaries: Modules no longer directly invoke one another.
- Extensible pattern: The same saga orchestration approach can be reused for:
  - Campaign lifecycle (pend → confirm → complete → review)
  - Support chat lifecycle
  - Refund workflows
- Resilience: Saga can track progress, retries, and timeouts.
- Transparency: With state machine events logged, developers can trace progress end-to-end.