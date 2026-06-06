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

Hexagonal / Clean Architecture with Dutch-language domain (Twente municipality waste collection calendar app).

**Dependency flow:** `ConsoleUI` / `DesktopUI` → `Application` → `Domain` ← `Infrastructure`

- **Domain** — Zero NuGet dependencies. Pure C# types: entities, value objects, enums, and the three outbound port interfaces (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`).
- **Application** — `AfvalService` orchestrates the full fetch/persist/export workflow. Houses inbound and outbound port interfaces.
- **Infrastructure** — Driven adapters: `TwenteMilieuApi` (HTTP), `EfAfvalRepository` (SQLite/EF Core), `IcsExporter` (Ical.Net).
- **Presentation** — `ConsoleUI` (ANSI terminal, `Microsoft.Extensions.Hosting`) and `DesktopUI` (Avalonia, `CommunityToolkit.Mvvm` source generators). Both layers own DI wiring.

```
┌─────────────────────────────────────────────────────────┐
│  Presentation  (Driving Adapter)                        │
│  ConsoleUI (ANSI) · DesktopUI (Avalonia)                │
├─────────────────────────────────────────────────────────┤
│  Application  (Core)                                    │
│  AfvalService · Inbound ports · Outbound ports          │
├─────────────────────────────────────────────────────────┤
│  Domain  (Core — ZERO external dependencies)            │
│  Adres · AfvalOphaalMoment · AfvalType                  │
├─────────────────────────────────────────────────────────┤
│  Infrastructure  (Driven Adapters)                      │
│  TwenteMilieuApi · EfAfvalRepository · IcsExporter      │
└─────────────────────────────────────────────────────────┘
```

### Core workflow (`AfvalService.VerwerkKalenderAsync`)

1. `TwenteMilieuApi.HaalUniekAdresIdOpAsync` → POST `/api/FetchAdress` → returns `UniqueId`
2. `TwenteMilieuApi.HaalKalenderOpAsync` → POST `/api/GetCalendar` → returns `List<AfvalOphaalMoment>`
3. `EfAfvalRepository.SlaOpOfUpdateAsync` → upsert into `afvalkalender.db` (local SQLite file); tracks `LaatstGewijzigd` on change
4. Re-fetch from DB for consistency
5. `IcsExporter.ExporteerAsync` → writes `AfvalKalender_{postcode}_{huisnummer}_{year}.ics`

### Key design decisions

- **No EF migrations** — schema created via `EnsureCreated()` on first run; `afvalkalender.db` lives next to the executable.
- **SSL validation disabled** for the Twente Milieu API `HttpClient` (API uses a self-signed cert).
- **Idempotent upserts** — re-running with the same address updates only records whose `Omschrijving` changed.
- **ICS UIDs** are scoped per `type + date + postcode` to prevent duplicate calendar entries on reimport.
- **MVVM source generators** — `[ObservableProperty]` and `[RelayCommand]` on `MainWindowViewModel` generate boilerplate at compile time; do not write manual `INotifyPropertyChanged` code.
- **Dutch throughout** — all domain identifiers, entity names, and method names are in Dutch to match the problem domain.
- **MediatR** — not the default; prefer a hand-rolled `ICommandHandler<TCommand>` interface + DI registration. Existing MediatR usage does not need to be refactored.

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
- **Repository tests** use EF Core InMemory; **ViewModel tests** use `Avalonia.Headless` with mocked `IAfvalService`
