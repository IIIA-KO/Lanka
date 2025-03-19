# Introduction of the Contracts Project

**Date:** 2025-02-19  
**Status:** Accepted

## Context

In the Lanka project, different modules need to share immutable value objects, DTOs, and events. However, direct dependencies between modules introduce tight coupling, which conflicts with the goal of maintaining a modular and independent architecture.

To address this, we need a Contracts project to define shared structures that multiple modules can rely on without referencing each other directly.

## Decision

A new Contracts project will be introduced to:

- Contain only immutable value objects, DTOs, and integration events.
- Ensure that modules communicate only via contracts instead of referencing internal domain models.
- Allow modules to evolve independently while maintaining a stable contract for communication.
- Be used in public APIs of each module, ensuring that only necessary data is exposed.

## Consequences

### Positive

- Modules interact through well-defined contracts rather than direct dependencies.
- Common data structures (e.g., Money, Currency) are centralized and consistent across the system.
- Contracts provide a stable API surface, making it easier to manage updates.
- Reduces unnecessary dependencies between modules.

### Negative

- Requires discipline to keep contracts updated and versioned properly.
- Some data may be duplicated across modules to avoid direct references.
- Business rules must remain in domain models, which could lead to confusion if not clearly separated.
