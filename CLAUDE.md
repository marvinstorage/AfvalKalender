# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build
dotnet build --configuration Release

# Run
dotnet run --project AfvalKalender.ConsoleUI
dotnet run --project AfvalKalender.DesktopUI

# Test
dotnet test                                            # all projects
dotnet test AfvalKalender.UnitTests                    # domain + application tests only
dotnet test AfvalKalender.Infrastructure.Tests         # EF Core repository tests
dotnet test AfvalKalender.DesktopUI.Tests              # Avalonia headless UI tests

# Run a single test by name
dotnet test AfvalKalender.UnitTests --filter "FullyQualifiedName~AdresTests.Constructor_MetGeldigeData"

# Publish self-contained executables
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
```

## Architecture & Layer Rules

Hexagonal / Clean Architecture with Dutch-language domain (multi-provider Dutch waste collection calendar app via the Ximmio API).

**Dependency flow:** `Presentation` → `Application` → `Domain` ← `Infrastructure`

- **Domain** — Zero NuGet dependencies. Pure C# types: entities, value objects, enums, and the three outbound port interfaces (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`).
- **Application** — Light CQRS. `ICommandHandler<TCommand, TResult>` is the inbound port contract. `VerwerkKalenderCommandHandler` orchestrates the full fetch/persist/export workflow.
- **Infrastructure** — Driven adapters: `TwenteMilieuApi` (HTTP), `EfAfvalRepository` (SQLite/EF Core), `IcsExporter` (Ical.Net), `CacherendeAfvalApi` (24 h file cache decorator).
- **Presentation** — `ConsoleUI` (ANSI/CLI), `DesktopUI` (Avalonia/GUI), and `AndroidUI` (MAUI). All layers depend on `ICommandHandler` only — no direct coupling to Application internals.

```
┌─────────────────────────────────────────────────────────────┐
│  Presentation (Driving Adapters)                            │
│  ConsoleUI (ANSI) · DesktopUI (Avalonia) · Android (MAUI)   │
└───────────────┬─────────────────────────────────────────────┘
                │ depends on (ICommandHandler)
                ▼
┌─────────────────────────────────────────────────────────────┐
│  Application (Inbound Ports / Orchestration)                │
│  VerwerkKalenderCommand · VerwerkKalenderCommandHandler     │
└───────────────┬──────────────────────┬──────────────────────┘
                │ calls                │ calls
                ▼                      ▼
┌──────────────────────────────────────┬──────────────────────┐
│  Domain (Core Business Logic)        │  Infrastructure      │
│  Entities: Adres, AfvalOphaalMoment  │  (Driven Adapters)   │
│  ValueObjects: AfvalType, Verwerker  │  TwenteMilieuApi     │
│  Interfaces (Outbound Ports):        │  EfAfvalRepository   │
│    IAfvalApi, IAfvalRepository,      │  IcsExporter         │
│    IIcsExporter                      │  CacherendeAfvalApi  │
└──────────────────────────────────────┴──────────────────────┘
```

### Commands pattern

```
ICommandHandler<TCommand, TResult>          ← inbound port (Application/Commands/)
VerwerkKalenderCommand                      ← immutable record; input only, no behaviour
VerwerkKalenderCommandHandler               ← orchestrator; calls outbound ports
```

All UIs resolve `ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>` from DI.
Adding a new use case = new `record` + new `Handler` class + one DI line. No other files change.
Cross-cutting concerns (logging, timing, validation) attach as handler decorators wrapping the interface.

### Core workflow (`VerwerkKalenderCommandHandler.HandleAsync`)

1. Receives `VerwerkKalenderCommand` with postcode, huisnummer, jaar, herinneringUur, outputPad, companyCode
2. `IAfvalApi.HaalUniekAdresIdOpAsync` → POST `/api/FetchAdress` → returns `UniqueId`
3. `IAfvalApi.HaalKalenderOpAsync` → POST `/api/GetCalendar` → returns `List<AfvalOphaalMoment>`
4. `IAfvalRepository.SlaOpOfUpdateAsync` → upsert into `afvalkalender.db` (local SQLite file)
5. `IIcsExporter.ExporteerAsync` → writes `AfvalKalender_{postcode}_{huisnummer}_{year}.ics`

### Key design decisions

- **No EF migrations** — schema created via `EnsureCreated()` on first run.
- **SSL validation disabled** for the Ximmio `HttpClient` (API uses a self-signed cert).
- **Multi-provider via companyCode** — The UI populates a picker from `AfvalVerwerkers.Alle`; the selected `CompanyCode` travels through the command to every `IAfvalApi` call. 
- **Postcode normalisation** — All UIs strip spaces before building the command (`"1234 AB"` → `"1234AB"`).
- **MAUI 10 / Android 16 Readiness** — `AndroidUI` uses the `CreateWindow` override and compiled bindings (`x:DataType`) for performance.
- **Idempotent upserts** — Re-running updates only records whose `Omschrijving` changed.
- **ICS UIDs** are scoped per `type + date + postcode` to prevent duplicate calendar entries on reimport.
- **MVVM source generators** — `[ObservableProperty]` and `[RelayCommand]` on `MainWindowViewModel` generate boilerplate at compile time.
- **Dutch throughout** — All domain identifiers, entity names, and method names are in Dutch.
- **MediatR** — Not used; prefer the hand-rolled `ICommandHandler<TCommand>` pattern.

## Multi-Disciplinaire Teamlens

When answering questions or generating code, reason through the combined perspective of our cross-functional Scrum team:

- **Senior Software Architect & DDD Expert** — strict bounded context boundaries, core domain purity, invariant enforcement, structural consistency, no framework bleed.
- **Senior Software Engineer** — robust, performance-optimised, readable, type-safe C# / .NET 10. Minimal but deliberate use of third-party libraries. Clean separation of concerns.
- **Product Owner & Business Analyst** — maximising user value; zero-friction UX (reuse native phone/desktop calendars instead of forcing app installations).
- **Quality Specialist / Test Engineer** — automated testability via xUnit, FluentAssertions, and Moq.

## Core Engineering Philosophies

### Domain-Driven Design (DDD) Tactical Patterns

| Pattern | Rule |
|---------|------|
| **Aggregate Root** | Enforces all internal invariants before state change. References other aggregates by `Id` only. |
| **Entity** | Stable, unique identity that persists over time. |
| **Value Object** | Immutable; equality based on structural value. Use C# `record` or `init`-only properties. |
| **Domain Event** | Past-tense Dutch name. Captures what has already happened; emitted by aggregates. |
| **Domain Service** | Stateless. Encapsulates calculations spanning multiple aggregates. |
| **Repository** | Interface in Application layer; implementation in Infrastructure. |
| **Read Model** | Separate, query-optimised projection. Never mutate write-side aggregates via a read model. |

### Third-Party Library Policy

Evaluate every new NuGet package against: *"Can we implement this ourselves with manageable code, or does this library add enough value to justify the dependency?"*

Order of preference:
1. **Custom implementation** — for simple patterns that fit the architecture (Commands/Handlers, Result types).
2. **BCL / .NET Runtime** — always preferred over third-party.
3. **Open Source (MIT or Apache 2.0)** — widely adopted, actively maintained.
4. **Commercial / restrictively licensed** — only after explicit approval.

## Testing Conventions

- **Frameworks**: xUnit · FluentAssertions · Moq · EF Core InMemory (repo tests) · Avalonia.Headless.XUnit (VM tests)
- **Test name format**: `Method_Scenario_ExpectedResult` in Dutch (e.g., `Constructor_MetGeldigeData_ZouAdresMoetenAanmaken`)
- **Mocking**: mock outbound ports (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`) via Moq; never mock domain entities
- **Repository tests** use EF Core InMemory; **ViewModel tests** use `Avalonia.Headless` with a mocked `ICommandHandler<,>`
- **Handler tests** mock the three outbound ports (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`) and test the handler directly

## Architecture Decision Records

### ADR-001 — Light CQRS via hand-rolled ICommandHandler

**Status:** Accepted

**Context:** Both `ConsoleUI` and `DesktopUI` needed to trigger the same orchestration logic (fetch → persist → export). They were both coupled to the concrete `AfvalService` class, making it impossible to swap or decorate the workflow without touching both UIs.

**Decision:** Introduce a minimal, hand-rolled CQRS pattern:
- `ICommandHandler<TCommand, TResult>` as the inbound port contract (no MediatR)
- `VerwerkKalenderCommand` record as the input value object
- `VerwerkKalenderCommandHandler` as the single orchestrator, replacing `AfvalService`

Both UIs depend only on the interface. The handler is registered in DI.

**Consequences:**
- Adding a new use case requires only a new `record` + new `Handler` + one DI registration
- Cross-cutting concerns (logging, caching, validation) can be added as handler decorators without touching either UI or the handler itself
- Handler is straightforward to test in isolation — mock the three outbound ports, call `HandleAsync`
- No MediatR dependency; the pattern fits in ~30 lines of framework code
