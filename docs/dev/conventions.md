# Conventions

The rules the code follows. When in doubt, copy the style of the nearest existing file.

## Language

- **Domain language is Dutch**: entities, value objects, methods, events, properties (`AfvalOphaalMoment`, `SlaOpOfUpdateAsync`, `Omschrijving`, `AfvalOphaalMomentGewijzigd`). Events have past-tense names.
- User-facing text and calendar descriptions are Dutch. Developer docs, ADRs and CONTRIBUTING are English; README and CHANGELOG are Dutch.
- Technical framework terms stay English (`Handler`, `Decorator`, `Repository`).

## Architecture rules

- Dependency flow: Presentation -> Application -> Domain <- Infrastructure. The Domain has no NuGet packages ([Architecture](architecture.md)).
- Ports are interfaces in `AfvalKalender.Domain/Interfaces/`; adapters live in Infrastructure.
- Repositories never expose `IQueryable` through the port.
- Domain events are raised inside the entity and are never dispatched synchronously to infrastructure; the repository stores them in the outbox ([ADR-004](../adr/ADR-004-domain-events-outbox.md)).
- UIs are thin: they build a `VerwerkKalenderCommand` and resolve `ICommandHandler`. No business logic in views or view models.
- Use the hand-rolled `ICommandHandler<,>`; no MediatR ([ADR-001](../adr/ADR-001-light-cqrs-icommandhandler.md)).
- New cross-cutting behaviour for commands is a decorator, like `AfvalKalender.Application/Commands/ValidatingCommandHandlerDecorator.cs`.

## Behaviour rules

- **Postcode normalisation:** every UI strips spaces and upper-cases the postcode before it builds the command (`1234 AB` becomes `1234AB`).
- **Idempotent upserts:** re-running only updates records whose `Omschrijving` changed.
- **ICS UIDs** are `type_date_postcode`.
- **Validation** lives in `VerwerkKalenderCommandValidator`, not in the UIs.
- **Cache** is best-effort: a failing cache must never block the user.
- SSL validation is bypassed only on the specific `HttpClient` instances that need it.

## Code style

- C# 14 on .NET 10, nullable reference types on.
- MVVM in Desktop and Android uses `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm.
- Value objects are `record` types or have `init`-only properties.
- Android XAML bindings use `x:DataType` (compiled bindings).
- Prefer your own code, then the BCL, then an MIT or Apache 2.0 library. A new package needs a reason ([Third-Party Library Policy](../../CLAUDE.md#third-party-library-policy)).

## Privacy

- Never commit real postcodes or addresses, `.db` files or `apicache/` contents. Use made-up values like `1234AB` in tests, docs and issues.
- Passwords and tokens for sync are never logged or stored in the database.

## Docs and specs

- A new feature needs tests, a README update, updated diagrams and an ADR ([`.agents/AGENTS.md`](../../.agents/AGENTS.md)).
- Behaviour changes go through OpenSpec ([ADR-010](../adr/ADR-010-openspec-spec-driven-workflow.md)).
- Add a line under `## [Unreleased]` in `CHANGELOG.md`.
- Commit messages use `feat:`, `fix:`, `docs:`, `chore:`, `test:` or `refactor:`, an imperative first line and no AI attribution.
