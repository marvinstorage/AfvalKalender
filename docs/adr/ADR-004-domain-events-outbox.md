# ADR-004 — Domain Events and Transactional Outbox Pattern

**Status:** Accepted  
**Date:** 2026-06-10  
**Deciders:** Marvin Storage

---

## Context

The `AfvalOphaalMoment` entity encapsulates change-detection logic (the `Update` method).
Side-effects of those changes — such as notifying an external system, publishing to a
message bus, or auditing — must not be executed inside the domain entity itself (that
would couple the domain to infrastructure) nor outside the transaction (that risks
inconsistency if the DB write succeeds but the side-effect fails).

---

## Decision

### Domain Events (Domain Layer)

`AfvalOphaalMoment` inherits the following pattern:

```csharp
private readonly List<IDomainEvent> _domainEvents = new();
public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
public void ClearDomainEvents() => _domainEvents.Clear();
protected void AddDomainEvent(IDomainEvent e) => _domainEvents.Add(e);
```

Two concrete events are raised:
- `AfvalOphaalMomentToegevoegd` — raised from the constructor when a new moment is created.
- `AfvalOphaalMomentGewijzigd` — raised from `Update()` when the `Omschrijving` changes.

Both are past-tense Dutch names, consistent with the Ubiquitous Language.

### Outbox Pattern (Infrastructure Layer)

`EfAfvalRepository.SlaOpOfUpdateAsync` harvests domain events **inside the same
`SaveChangesAsync` call** as the entity changes, writing them to an `OutboxMessages`
table in SQLite:

```csharp
// Serialize domain events into OutboxMessage rows
_context.OutboxMessages.Add(new OutboxMessage
{
    EventType = domainEvent.GetType().FullName,
    Content   = JsonConvert.SerializeObject(domainEvent),
    OccurredOn = domainEvent.OccurredOn
});
entity.ClearDomainEvents();

await _context.SaveChangesAsync(); // atomic write
```

An `OutboxMessage` has `ProcessedOn` (nullable) to track when a downstream consumer
has processed the event. No background processor is implemented yet — the Outbox
table is ready for a future relay/processor.

---

## Consequences

### Positive
- Domain entity raises events expressing what happened in business terms — no
  coupling to infrastructure.
- Events and entity state are always consistent: same SQLite transaction.
- The Outbox table is ready to feed a future message relay (e.g., notify a webhook,
  publish to Azure Service Bus) without changing the domain or application layer.

### Negative / Trade-offs
- `OutboxMessage` currently has no background relay processor — events accumulate
  in the table indefinitely until a processor is added.
- `Newtonsoft.Json` is used to serialize events (already a transitive dependency
  via the Ximmio API adapter); System.Text.Json could be used instead in future.
- `ProcessedOn` column is not currently used by anything — adds schema complexity
  without immediate value.
