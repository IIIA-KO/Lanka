# Resources That Shaped This Project

<div align="center">

*Books, articles, and projects that influenced Lanka's architecture*

</div>

---

## Why This List Exists

Every architectural decision builds on ideas from others. This page documents the resources that most influenced how Lanka is structured. If you want to understand *why* things are done a certain way, these sources provide the theoretical foundation.

---

## Books

### Domain-Driven Design

| Book | Author | What I Took From It |
|------|--------|---------------------|
| *Domain-Driven Design: Tackling Complexity in the Heart of Software* | Eric Evans | The foundational concepts: aggregates, entities, value objects, bounded contexts. The "blue book" that started it all. |
| *Implementing Domain-Driven Design* | Vaughn Vernon | Practical application of DDD patterns. More concrete than Evans, with code examples. The aggregate design rules were particularly useful. |


### Architecture

| Book | Author | What I Took From It |
|------|--------|---------------------|
| *Clean Architecture* | Robert C. Martin | The dependency rule and layer organization. Why domain should not depend on infrastructure. |
| *Building Microservices* | Sam Newman | Even though Lanka is a monolith, this book's discussion of service boundaries and communication patterns applies. The modular monolith approach borrows many ideas. |
| *Patterns of Enterprise Application Architecture* | Martin Fowler | Unit of Work, Repository pattern, and other foundational patterns. Still relevant decades later. |

### Event-Driven Systems

| Book | Author | What I Took From It |
|------|--------|---------------------|
| *Enterprise Integration Patterns* | Gregor Hohpe, Bobby Woolf | Message patterns, saga orchestration concepts, reliable messaging. The outbox pattern is discussed here. |
| *Designing Data-Intensive Applications* | Martin Kleppmann | Understanding distributed systems trade-offs, eventual consistency, and why reliable messaging is hard. |

---

## Online Resources

### Blogs and Articles

| Resource | Author/Source | Topics |
|----------|---------------|--------|
| [Milan Jovanović's Blog](https://www.milanjovanovic.tech/) | Milan Jovanović | .NET-specific CQRS, DDD, and Clean Architecture. Weekly newsletter with practical examples. Many Lanka patterns came from here. |
| [Anton Martyniuk's Blog](https://antondevtips.com/) | Anton Martyniuk | Practical .NET tutorials, Entity Framework, ASP.NET Core patterns. Newsletter covers real-world implementation details. |
| [UA .NET Community](https://dotnet.in.ua/uk) | Ukrainian .NET Community | Ukrainian .NET developer community. Events, meetups, and Telegram chat for knowledge sharing among Ukrainian developers. |
| [Event Sourcing You Are Doing It Wrong](https://www.youtube.com/watch?v=GzrZworHpIk) | Greg Young | Understanding event sourcing trade-offs. Why I chose event-driven without full event sourcing. |
| [Modular Monolith Primer](https://www.kamilgrzybek.com/blog/modular-monolith-primer) | Kamil Grzybek | The definitive guide to modular monolith architecture. His sample project was a major reference. |

### Sample Projects

| Project | Why It's Useful |
|---------|-----------------|
| [modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) | Kamil Grzybek's reference implementation. Similar architecture to Lanka. I studied this extensively. |
| [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers) | Microsoft's microservices reference. Even for a monolith, the patterns for handling distributed concerns apply. |
| [Ardalis Clean Architecture Template](https://github.com/ardalis/CleanArchitecture) | Steve Smith's practical template. Good starting point for understanding layer organization. |

### Documentation

| Resource | Topics |
|----------|--------|
| [MassTransit Documentation](https://masstransit.io/documentation) | State machines (sagas), message contracts, RabbitMQ integration. Essential for the event-driven parts. |
| [MediatR Wiki](https://github.com/jbogard/MediatR/wiki) | Pipeline behaviors, request handling patterns. |
| [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/) | Migrations, configuration, performance optimization. |

---

## Courses and Videos

| Resource | Platform | Topics |
|----------|----------|--------|
| *Domain-Driven Design Fundamentals* | Pluralsight | Steve Smith and Julie Lerman. Good video introduction to DDD concepts. |
| *Clean Architecture* | YouTube / Various | Multiple talks by Robert C. Martin explaining the principles. |
| *CQRS and Event Sourcing* | YouTube | Greg Young's talks on the topic. Even without full event sourcing, the CQRS concepts apply. |

---

## Specific Pattern References

### Outbox Pattern

- [Transactional Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html) — microservices.io
- [Implementing the Outbox Pattern](https://www.milanjovanovic.tech/blog/outbox-pattern-for-reliable-microservices-messaging) — Milan Jovanović

### Saga Orchestration

- [MassTransit Saga Documentation](https://masstransit.io/documentation/patterns/saga)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html) — microservices.io

### Result Pattern

- [Functional Error Handling](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern) — Milan Jovanović
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) — F# for Fun and Profit (the concept translates to C#)

### CQRS

- [CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs) — Microsoft Azure Architecture Center
- [CQRS Documents](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf) — Greg Young

---

## Tools Documentation

| Tool | Documentation | Used For |
|------|---------------|----------|
| PostgreSQL | [postgresql.org/docs](https://www.postgresql.org/docs/) | Primary database |
| MongoDB | [mongodb.com/docs](https://www.mongodb.com/docs/) | Analytics storage |
| RabbitMQ | [rabbitmq.com/documentation](https://www.rabbitmq.com/documentation.html) | Message broker |
| Keycloak | [keycloak.org/documentation](https://www.keycloak.org/documentation) | Identity provider |
| Elasticsearch | [elastic.co/guide](https://www.elastic.co/guide/) | Search functionality |

---

## How I Use These Resources

1. **Concept learning:** Books for foundational understanding, especially Evans and Vernon for DDD.

2. **Practical implementation:** Milan Jovanović's blog and Kamil Grzybek's sample project for .NET-specific patterns.

3. **Problem-solving:** Official documentation when implementing specific features (MassTransit for sagas, EF Core for data access).

4. **Architecture decisions:** Clean Architecture and Building Microservices for high-level design principles.

---

## Recommendations for Learning

If you're new to these concepts, I suggest this order:

1. **Start with:** *Clean Architecture* — Understand why layers matter
2. **Then:** *Domain-Driven Design Distilled* — Quick DDD introduction
3. **Explore:** Milan Jovanović's blog — Practical .NET examples
4. **Deep dive:** *Implementing Domain-Driven Design* — Detailed patterns
5. **Reference:** Kamil Grzybek's modular monolith project — See it all together

---

<div align="center">

*Standing on the shoulders of giants.*

</div>
