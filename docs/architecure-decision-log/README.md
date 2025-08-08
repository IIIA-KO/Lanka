# 🎯 Architecture Decision Log (ADL)

<div align="center">

*Documenting the why behind every architectural choice in Lanka*

![ADL Banner](../images/adl-banner.png)

**"Good architecture decisions are about trade-offs, and great architecture is about documenting those trade-offs."**

</div>

---

## 📚 **What is an Architecture Decision Log?**

An Architecture Decision Log (ADL) is a collection of records documenting important architectural decisions made during the development of Lanka. Each record (ADR - Architecture Decision Record) captures:

- 🎯 **The context** that led to the decision
- 🤔 **The decision** itself and alternatives considered  
- ✅ **The consequences** and trade-offs of the choice
- 📅 **When** the decision was made and its current status

---

## 🗂️ **Decision Categories**

<table>
<tr>
<td width="50%">

### **🏗️ Foundational Decisions**
- System architecture patterns
- Technology stack choices
- Core design principles
- Development methodologies

### **🔧 Technical Decisions**
- Database choices and strategies
- Communication patterns
- Security implementations
- Performance optimizations

</td>
<td width="50%">

### **🎯 Domain Decisions**  
- Business logic modeling
- Module boundaries
- Data flow patterns
- Integration strategies

### **🚀 Operational Decisions**
- Deployment strategies
- Monitoring approaches
- Development workflows
- Quality assurance methods

</td>
</tr>
</table>

---

## 📋 **Decision Index**

### **🏛️ Foundation & Architecture** *(001-010)*

| ADR | Title | Status | Impact |
|-----|-------|--------|--------|
| [001](001-adoption-of-adl.md) | 📚 Adoption of Architecture Decision Log | ✅ Accepted | 🟢 Foundation |
| [002](002-technology-stack.md) | 🛠️ Technology Stack Selection | ✅ Accepted | 🔴 High |
| [003](003-modular-monolith-architecture.md) | 🧩 Modular Monolith Architecture | ✅ Accepted | 🔴 High |
| [004](004-adoption-of-ddd.md) | 💎 Domain-Driven Design Adoption | ✅ Accepted | 🟡 Medium |
| [005](005-cqrs-implementation.md) | 🎪 CQRS Implementation Strategy | ✅ Accepted | 🟡 Medium |
| [006](006-mediatr-pipelines.md) | 🔄 MediatR Pipeline Adoption | ✅ Accepted | 🟢 Low |

### **🎭 Application Patterns** *(007-012)*

| ADR | Title | Status | Impact |
|-----|-------|--------|--------|
| [007](007-modules-overview.md) | 🧩 Module Structure Definition | ✅ Accepted | 🔴 High |
| [008](008-event-driven-architecture.md) | 🔄 Event-Driven Architecture | ✅ Accepted | 🟡 Medium |
| [009](009-configuration-management.md) | ⚙️ Configuration Management | ✅ Accepted | 🟢 Low |
| [010](010-contracts-projects.md) | 📋 Contract Projects Structure | ✅ Accepted | 🟢 Low |
| [011](011-messagins-with-outbox&inbox.md) | 📮 Outbox & Inbox Messaging Pattern | ✅ Accepted | 🟡 Medium |
| [012](012-reverse-proxy.md) | 🔀 Reverse Proxy Implementation | ✅ Accepted | 🟢 Low |

### **🎯 Specialized Solutions** *(013-020)*

| ADR | Title | Status | Impact |
|-----|-------|--------|--------|
| [013](013-adoption-of-saga-orchestration.md) | 🎭 Saga Orchestration Pattern | ✅ Accepted | 🟡 Medium |
| [014](014-mongodb-adoption-analytics.md) | 📊 MongoDB for Analytics Data | ✅ Accepted | 🟡 Medium |

---

## 📊 **Decision Status Legend**

| Status | Meaning | Description |
|--------|---------|-------------|
| 🎯 **Proposed** | Under consideration | Decision is being evaluated |
| ✅ **Accepted** | Active & implemented | Decision is in effect |
| 🔄 **Superseded** | Replaced by newer decision | Decision has been updated |
| ❌ **Rejected** | Not adopted | Decision was considered but rejected |
| ⚠️ **Deprecated** | Being phased out | Decision is no longer recommended |

## 🎨 **Impact Classification**

| Level | Description | Examples |
|-------|-------------|----------|
| 🔴 **High** | Fundamental architectural choices | Technology stack, architecture pattern |
| 🟡 **Medium** | Significant implementation decisions | Database choices, communication patterns |
| 🟢 **Low** | Tactical implementation choices | Library selections, configuration approaches |

---

## 🔍 **Quick Navigation**

### **By Topic**
- 🏗️ **Architecture**: [003](003-modular-monolith-architecture.md), [007](007-modules-overview.md), [008](008-event-driven-architecture.md)
- 🗃️ **Data & Persistence**: [014](014-mongodb-adoption-analytics.md), [011](011-messagins-with-outbox&inbox.md)
- 🔄 **Communication**: [008](008-event-driven-architecture.md), [011](011-messagins-with-outbox&inbox.md), [012](012-reverse-proxy.md)
- 💻 **Development**: [002](002-technology-stack.md), [005](005-cqrs-implementation.md), [006](006-mediatr-pipelines.md)

### **By Recency**
1. [014](014-mongodb-adoption-analytics.md) - MongoDB for Analytics Data
2. [013](013-adoption-of-saga-orchestration.md) - Saga Orchestration Pattern  
3. [012](012-reverse-proxy.md) - Reverse Proxy Implementation
4. [011](011-messagins-with-outbox&inbox.md) - Outbox & Inbox Messaging
5. [010](010-contracts-projects.md) - Contract Projects Structure

---

## 📝 **ADR Template**

When creating new Architecture Decision Records, use this template:

```markdown
# ADR XXX - [Short Descriptive Title]

**Date:** YYYY-MM-DD  
**Status:** [Proposed | Accepted | Superseded | Rejected | Deprecated]
**Supersedes:** [ADR number if applicable]
**Superseded by:** [ADR number if applicable]

## Context

Describe the context and problem statement that led to this decision.
Include any relevant background information.

## Decision

State the decision clearly and concisely.

## Alternatives Considered

List the alternative options that were considered and why they were not chosen.

## Consequences

### Positive
- List the positive outcomes and benefits

### Negative  
- List the negative consequences and trade-offs

### Neutral
- List neutral consequences that are neither positive nor negative

## Implementation Notes

Any specific implementation details or considerations.

## References

- Links to relevant documentation
- External resources that influenced the decision
```

---

## 🎯 **Decision Process**

### **1. 🤔 Identify the Decision**
- Recognize when an architectural decision needs to be made
- Define the scope and impact of the decision
- Gather stakeholders and context

### **2. 📊 Analyze Options** 
- Research and document alternatives
- Evaluate trade-offs and consequences
- Consider long-term implications

### **3. 📝 Document the Decision**
- Create ADR using the template
- Include rationale and alternatives
- Specify implementation guidance

### **4. ✅ Review & Approve**
- Get team review and feedback
- Ensure alignment with overall architecture
- Update status when decision is final

### **5. 🔄 Maintain & Evolve**
- Monitor implementation and outcomes
- Update status if decision changes
- Create superseding ADRs when needed

---

## 🎨 **Best Practices**

### **✅ Do**
- **Be specific** - Include concrete details about the decision
- **Explain the why** - Context is more important than the what
- **Consider alternatives** - Show you've thought through options
- **Be honest about trade-offs** - Include negative consequences
- **Update status** - Keep records current as decisions evolve

### **❌ Don't**
- **Make decisions in isolation** - Get input from relevant stakeholders
- **Skip documentation** - Even "obvious" decisions should be recorded
- **Ignore implementation feedback** - Update ADRs based on real experience
- **Be afraid to supersede** - It's okay to change decisions with new information

---

## 🚀 **Contributing to ADL**

### **Proposing New ADRs**
1. Create a new branch: `adr/xxx-your-decision-title`
2. Copy the ADR template and fill it out
3. Submit a pull request for team review
4. Update status based on team decision

### **Updating Existing ADRs**
1. Only update status and implementation notes
2. For significant changes, create a superseding ADR
3. Maintain historical context and reasoning

---

## 📊 **ADL Statistics**

<div align="center">

| Metric | Count |
|--------|-------|
| 📝 **Total ADRs** | 14 |
| ✅ **Accepted** | 14 |
| 🎯 **Proposed** | 0 |
| 🔄 **Superseded** | 0 |
| 🔴 **High Impact** | 3 |
| 🟡 **Medium Impact** | 6 |
| 🟢 **Low Impact** | 5 |

*Last updated: Aug 2025*

</div>

---

<div align="center">

*"The best way to make good decisions is to learn from bad ones, and the best way to learn from decisions is to document them."*

**Happy Decision Making! 🎯**

</div>