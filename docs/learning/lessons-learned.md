# Lessons Learned

<div align="center">

*Reflections on building Lanka — what worked, what was difficult, and what I would do differently*

</div>

---

## Overview

This document captures lessons from building Lanka as a diploma project. It's intended to be honest about both successes and challenges encountered while implementing modular monolith architecture with DDD patterns in .NET.

---

## What Worked Well

### 1. Starting with the Users Module

**Decision:** Build the Users module first as a reference implementation.

**Why it worked:** Having one complete module before starting others provided:
- A template for module structure that I could replicate
- Working examples of patterns (CQRS handlers, domain events, integration events)
- Confidence that the architecture could handle real requirements
- A benchmark for comparing implementation quality across modules

**Recommendation:** When learning new patterns, implement one vertical slice completely before spreading horizontally.

---

### 2. Architecture Decision Log (ADL)

**Decision:** Document every significant technical decision with context, alternatives considered, and rationale.

**Why it worked:**
- Forced me to think through decisions before implementing
- Created a reference for "why is it done this way" questions
- Valuable for diploma documentation — shows deliberate design, not accidental complexity
- Helped when revisiting code months later

**Recommendation:** Start the ADL from day one. Even simple decisions benefit from written reasoning.

---

### 3. Modular Monolith Over Microservices

**Decision:** Build a modular monolith instead of distributed microservices.

**Why it worked:**
- Single deployment simplified local development significantly
- Could focus on domain modeling and patterns without distributed systems complexity
- Still enforced module boundaries through integration events
- Easier debugging — everything runs in one process
- Appropriate for a learning project where operational overhead would distract from architectural goals

**Trade-off acknowledged:** Missing experience with service discovery, distributed tracing across services, and network failure handling.

---

### 4. Event-Driven Communication Between Modules

**Decision:** Modules communicate via integration events through RabbitMQ, not direct method calls.

**Why it worked:**
- Enforced loose coupling — modules genuinely don't know about each other's internals
- Made the Instagram linking saga possible without tight dependencies
- Easier to reason about module boundaries
- Closer to what microservices would require, so patterns transfer

**Key insight:** The discipline of "no cross-module repository access" revealed design problems early.

---

### 5. Catalog of Terms Documentation

**Decision:** Create a glossary explaining every pattern used in the codebase.

**Why it worked:**
- Clarified my own understanding by forcing precise definitions
- Useful reference during implementation — "what's the difference between domain event and integration event again?"
- Valuable for anyone reading the codebase
- Shows depth of understanding for academic review

---

## What Was Difficult

### 1. Saga Implementation Complexity

**Challenge:** The Instagram linking saga coordinates multiple modules, handles timeouts, and manages failure scenarios.

**What made it hard:**
- MassTransit state machines have a learning curve
- Testing sagas is non-trivial — need to simulate message flows
- Debugging distributed state across events requires careful logging
- Edge cases (timeout during processing, duplicate events) required thought

**How I addressed it:**
- Started with the simplest possible saga, added complexity incrementally
- Added extensive logging at state transitions
- Created the [Instagram Linking Walkthrough](walkthroughs/instagram-linking.md) to document the flow

**What I'd do differently:** Start with simpler workflows before attempting multi-module sagas.

---

### 2. MongoDB Integration for Analytics

**Challenge:** Mixing PostgreSQL (relational) with MongoDB (documents) in the same module.

**What made it hard:**
- Different transaction semantics — no distributed transactions
- Had to think carefully about what goes where
- Two different query patterns to maintain
- Eventual consistency between stores

**How I addressed it:**
- Clear separation: PostgreSQL for entities that participate in domain logic, MongoDB for read-optimized analytics data
- Accepted that some data would be eventually consistent

**Lesson:** Polyglot persistence adds complexity. Use it only when the benefits are clear.

---

### 3. External API Integration (Instagram)

**Challenge:** Building reliable integration with Instagram's API while developing locally.

**What made it hard:**
- Rate limits during development
- OAuth flow requires real redirects
- Test accounts have limited data
- API responses needed mocking for tests

**How I addressed it:**
- Built mock services that return realistic fake data
- Email-based resolution to switch between real and mock services
- Bogus library for generating fake Instagram profiles

**Key insight:** Mock services should be first-class code, not afterthoughts.

---

### 4. Keeping Modules Truly Independent

**Challenge:** Resisting the temptation to "just call that repository" across modules.

**What made it hard:**
- Sometimes the "right" data lives in another module
- Event-driven communication feels like overhead for simple queries
- Had to think about data duplication vs. querying

**How I addressed it:**
- Strict rule: no cross-module repository access
- When I needed data from another module, I either:
  - Included it in integration events
  - Duplicated it (with eventual consistency)
  - Reconsidered the module boundary

**Lesson:** Module boundaries should align with bounded contexts. If you constantly need data from another module, the boundary might be wrong.

---

### 5. Testing Event-Driven Flows

**Challenge:** Integration tests for flows that span multiple modules via events.

**What made it hard:**
- Events are asynchronous — tests need to wait for processing
- Multiple handlers might process the same event
- Test isolation with a real message broker is tricky

**How I addressed it:**
- Architecture tests verify module dependencies
- Integration tests use in-memory message broker for determinism
- Focused unit tests on individual handlers

**What I'd improve:** More comprehensive integration test infrastructure.

---

## What I Would Do Differently

### 1. Define Module Contracts Earlier

**Issue:** Some integration events evolved as I discovered what data was needed.

**Better approach:** Design integration event contracts upfront as part of module specification. Treat them as APIs between modules.

---

### 2. Invest More in Test Infrastructure Early

**Issue:** Test infrastructure was often an afterthought, making testing harder later.

**Better approach:** Build test helpers, factories, and fixtures as the first implementation step. Testing should be easy, not an obstacle.

---

### 3. Simpler Domain First

**Issue:** Some early domain models were over-complicated, trying to anticipate future requirements.

**Better approach:** Start with the simplest model that works. Refactor when actual requirements demand it. YAGNI applies to domain models too.

---

### 4. Document Flows Earlier

**Issue:** The Instagram linking flow documentation was written after implementation, requiring significant effort to trace through code.

**Better approach:** Document complex flows as sequence diagrams before implementing. Use them as implementation guides.

---

## Patterns That Proved Their Value

### Result Pattern

Using `Result<T>` instead of exceptions for business errors made error handling explicit and composable. Code became easier to read and test.

### Outbox Pattern

Reliable event publishing without distributed transactions. Essential for event-driven architecture.

### Value Objects

Encapsulating validation in value objects (Email, Money, etc.) eliminated entire categories of bugs.

### CQRS with Separate Read Models

Separating read and write concerns allowed optimizing each independently. Queries don't need to load full aggregates.

---

## Architectural Trade-offs Accepted

| Trade-off | What I Gained | What I Lost |
|-----------|---------------|-------------|
| Modular monolith over microservices | Simpler deployment, easier debugging | Independent scaling, technology heterogeneity |
| Event-driven over synchronous | Loose coupling, resilience | Immediate consistency, simpler debugging |
| PostgreSQL + MongoDB | Right tool for each job | Operational complexity, no cross-store transactions |
| Rich domain models | Business logic encapsulation | Learning curve, more code |
| Explicit event contracts | Clear module boundaries | Duplication of some data structures |

---

## Key Takeaways

1. **Architecture is about trade-offs.** Every pattern has costs. Document why you accepted them.

2. **Start simple, add complexity when needed.** The patterns should solve actual problems, not theoretical ones.

3. **Module boundaries are design decisions.** They should reflect business capabilities, not technical layers.

4. **Event-driven requires discipline.** It's easy to create event spaghetti if you're not careful about event design.

5. **Documentation is part of the system.** Especially for learning projects, writing things down clarifies thinking.

6. **Mocks and test infrastructure are production code.** They need the same care as the main application.

---

## For Future Diploma Students

If you're reading this for inspiration on your own project:

- **Choose a domain you find interesting.** You'll spend months with it.
- **Pick 2-3 patterns to learn deeply**, not 10 superficially.
- **Write the ADL as you go.** It's easier than reconstructing decisions later.
- **Build one complete vertical slice first.** Then replicate the pattern.
- **It's okay to refactor.** Learning means your understanding evolves.

---

<div align="center">

*The goal was learning, and on that measure, the project succeeded.*

</div>
