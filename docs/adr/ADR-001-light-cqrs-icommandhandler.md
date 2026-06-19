# ADR-001 — Light CQRS via hand-rolled ICommandHandler

**Status:** Accepted  
**Date:** 2025-06-01  
**Deciders:** Marvin Storage

---

## Context

Both `ConsoleUI` and `DesktopUI` needed to trigger the same orchestration logic
(fetch → persist → export). Both were directly coupled to the concrete `AfvalService`
class, making it impossible to swap, decorate, or test the workflow in isolation
without touching both entry points.

The application also needed a way to add cross-cutting concerns (validation, logging,
timing) in a single place without polluting either the UI layer or the core handler.

---

## Decision

Introduce a minimal, hand-rolled CQRS pattern instead of adopting MediatR:

- `ICommandHandler<TCommand, TResult>` — the generic inbound port contract
  (defined in `AfvalKalender.Application/Commands/`)
- `VerwerkKalenderCommand` — an immutable `record` carrying user input with no behaviour
- `VerwerkKalenderCommandHandler` — stateless orchestrator; replaces `AfvalService`

All three UIs (`ConsoleUI`, `DesktopUI`, `AndroidUI`) depend only on the interface,
resolved through the DI container. No UI file has a direct reference to any handler
or application internals.

Cross-cutting concerns attach as **decorator** implementations of the same interface:

```
ValidatingCommandHandlerDecorator<TCommand, TResult>
    wraps → VerwerkKalenderCommandHandler
```

---

## Consequences

### Positive
- Adding a new use case requires only: a new `record` + a new `Handler` + one DI line — no other files change.
- Handler is tested in isolation by mocking the three outbound ports (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`) and calling `HandleAsync` directly.
- Validation, logging, and timing can be stacked as decorators with zero impact on the handler or any UI.
- No MediatR dependency; the entire pattern fits in ~30 lines of framework code.

### Negative / Trade-offs
- A new hand-rolled abstraction that contributors must learn (though it is simpler than MediatR).
- No built-in pipeline ordering; decorator stacking order must be managed manually in DI.
