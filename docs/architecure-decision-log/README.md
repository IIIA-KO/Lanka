# Architecture Decision Log (ADL)

<div align="center">

*Why I made the choices I made*

![ADL Banner](../images/adl-banner.png)

</div>

---

## What's an ADL?

An Architecture Decision Log is a collection of records documenting significant architectural decisions. Each record (ADR) captures:

- **Context** — The situation that led to the decision
- **Decision** — What was decided and alternatives considered
- **Consequences** — Trade-offs and implications
- **Status** — Whether it's still active or has been superseded

For a diploma project, this serves two purposes: it helps me remember *why* I made certain choices, and it demonstrates to reviewers that decisions were made deliberately, not arbitrarily.

---

## Decision Index

### Foundation & Architecture (001-006)

| ADR | Title | Status | Impact |
|-----|-------|--------|--------|
| [001](001-adoption-of-adl.md) | Adoption of Architecture Decision Log | Accepted | Foundation |
| [002](002-technology-stack.md) | Technology Stack Selection | Accepted | High |
| [003](003-modular-monolith-architecture.md) | Modular Monolith Architecture | Accepted | High |
| [004](004-adoption-of-ddd.md) | Domain-Driven Design Adoption | Accepted | Medium |
| [005](005-cqrs-implementation.md) | CQRS Implementation Strategy | Accepted | Medium |
| [006](006-mediatr-pipelines.md) | MediatR Pipeline Adoption | Accepted | Low |

### Application Patterns (007-012)

| ADR | Title | Status | Impact |
|-----|-------|--------|--------|
| [007](007-modules-overview.md) | Module Structure Definition | Accepted | High |
| [008](008-event-driven-architecture.md) | Event-Driven Architecture | Accepted | Medium |
| [009](009-configuration-management.md) | Configuration Management | Accepted | Low |
| [010](010-contracts-projects.md) | Contract Projects Structure | Accepted | Low |
| [011](011-messagins-with-outbox&inbox.md) | Outbox & Inbox Pattern | Accepted | Medium |
| [012](012-reverse-proxy.md) | Reverse Proxy Implementation | Accepted | Low |

### Specialized Solutions (013-015)

| ADR | Title | Status | Impact |
|-----|-------|--------|--------|
| [013](013-adoption-of-saga-orchestration.md) | Saga Orchestration Pattern | Accepted | Medium |
| [014](014-mongodb-adoption-analytics.md) | MongoDB for Analytics | Accepted | Medium |
| [015](015-aspire-adoption.md) | .NET Aspire Adoption | Accepted | Medium |

---

## Quick Navigation

### By Topic

- **Architecture**: [003](003-modular-monolith-architecture.md), [007](007-modules-overview.md), [008](008-event-driven-architecture.md)
- **Data & Persistence**: [014](014-mongodb-adoption-analytics.md), [011](011-messagins-with-outbox&inbox.md)
- **Communication**: [008](008-event-driven-architecture.md), [011](011-messagins-with-outbox&inbox.md), [012](012-reverse-proxy.md)
- **Orchestration**: [015](015-aspire-adoption.md)
- **Development Patterns**: [002](002-technology-stack.md), [005](005-cqrs-implementation.md), [006](006-mediatr-pipelines.md)

### Most Recent

1. [015](015-aspire-adoption.md) — .NET Aspire Adoption
2. [014](014-mongodb-adoption-analytics.md) — MongoDB for Analytics
3. [013](013-adoption-of-saga-orchestration.md) — Saga Orchestration Pattern

---

## Status Legend

| Status | Meaning |
|--------|---------|
| **Proposed** | Under consideration |
| **Accepted** | Active and implemented |
| **Superseded** | Replaced by newer decision |
| **Rejected** | Considered but not adopted |
| **Deprecated** | Being phased out |

## Impact Classification

| Level | Description |
|-------|-------------|
| **High** | Fundamental choices (architecture, tech stack) |
| **Medium** | Significant decisions (database, patterns) |
| **Low** | Tactical choices (libraries, config) |

---

## ADR Template

When documenting a new decision:

```markdown
# ADR XXX - Title

**Date:** YYYY-MM-DD
**Status:** Proposed | Accepted | Superseded | Rejected

## Context

What's the situation? What problem are we solving?

## Decision

What did we decide? Be specific.

## Alternatives Considered

What else was considered and why wasn't it chosen?

## Consequences

### Positive
- Benefits of this decision

### Negative
- Trade-offs and downsides

### Neutral
- Side effects that aren't clearly positive or negative

## Implementation Notes

Any specific guidance for implementing this decision.

## References

- Links to relevant resources
```

---

## Statistics

| Metric | Count |
|--------|-------|
| **Total ADRs** | 15 |
| **Accepted** | 15 |
| **High Impact** | 3 |
| **Medium Impact** | 7 |
| **Low Impact** | 5 |

---

## Why Document Decisions?

1. **Memory** — In six months, I won't remember why I chose PostgreSQL over MySQL
2. **Communication** — Reviewers can understand the reasoning without asking
3. **Learning** — Writing it down forces me to think it through
4. **Accountability** — If a decision was wrong, I can see what I was thinking

---

<div align="center">

*"The best time to document a decision is when you make it. The second best time is now."*

</div>
