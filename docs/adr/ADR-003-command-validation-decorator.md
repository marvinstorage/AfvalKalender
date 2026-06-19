# ADR-003 — Command Validation via Decorator Pipeline

**Status:** Accepted  
**Date:** 2026-06-01  
**Deciders:** Marvin Storage

---

## Context

`VerwerkKalenderCommandHandler` contains pure orchestration logic. Adding input
validation directly to the handler would violate the Single Responsibility Principle
and make it harder to test validation rules independently of the orchestration flow.

FluentValidation was evaluated but rejected: it adds a significant transitive
dependency for a small number of simple, domain-driven validation rules.

---

## Decision

Introduce two new types in `AfvalKalender.Application/Commands/`:

- `ICommandValidator<TCommand>` — a single-method interface (`void Validate(TCommand)`)
- `ValidatingCommandHandlerDecorator<TCommand, TResult>` — wraps any
  `ICommandHandler<TCommand, TResult>`, calls the validator first, then delegates

`VerwerkKalenderCommandValidator` implements `ICommandValidator<VerwerkKalenderCommand>`:

| Field | Rule |
|---|---|
| `Postcode` | Required; must match `^[1-9][0-9]{3}[A-Z]{2}$` |
| `Huisnummer` | Required (non-whitespace) |
| `Jaar` | Between 2000 and 2100 inclusive |
| `HerinneringUur` | Between 0 and 23 inclusive |
| `OutputPad` | Required (non-whitespace) |
| `CompanyCode` | Must be a parseable `Guid` |

All three UIs register the chain identically:

```csharp
services.AddScoped<VerwerkKalenderCommandHandler>();
services.AddScoped<ICommandValidator<VerwerkKalenderCommand>, VerwerkKalenderCommandValidator>();
services.AddScoped<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>(sp =>
    new ValidatingCommandHandlerDecorator<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>(
        sp.GetRequiredService<VerwerkKalenderCommandHandler>(),
        sp.GetRequiredService<ICommandValidator<VerwerkKalenderCommand>>()));
```

---

## Consequences

### Positive
- Validation rules are tested completely independently of handler logic.
- The decorator can be removed or swapped per DI registration without changing any
  handler or UI code.
- Additional decorators (logging, timing) can be stacked on top using the same pattern.
- No FluentValidation dependency.

### Negative / Trade-offs
- Validation errors surface as `ArgumentException`; callers must catch and display them.
- The DI registration is slightly verbose compared to using a framework with automatic
  pipeline registration.
