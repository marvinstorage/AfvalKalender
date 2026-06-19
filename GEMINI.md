# GEMINI.md

This file provides guidance to Gemini (Antigravity CLI) when working with code in this repository.

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

# Build Android APK (requires Android SDK + JDK installed locally)
# AndroidSdkDirectory = path to your Android SDK root
# JavaSdkDirectory    = path to your JDK 21 installation
dotnet build-server shutdown && dotnet publish AfvalKalender.AndroidUI \
  -c Release \
  -f net10.0-android \
  -p:RuntimeIdentifier=android-arm64 \
  -p:AndroidPackageFormat=apk \
  -p:AndroidKeyStore=false \
  -p:SkipUsingBuiltInWorkloads=true \
  -p:NoWarn=NU1605 \
  -p:AndroidSdkDirectory=$HOME/.android/sks \
  -p:JavaSdkDirectory=/usr/lib/jvm/java-21-openjdk-amd64
```

## Architecture & Layer Rules

The project follows **Hexagonal / Clean Architecture** principles, ensuring that the core business logic (Domain & Application) is isolated from external concerns like UIs, databases, and APIs.

**Dependency flow:** `Presentation` → `Application` → `Domain` ← `Infrastructure`

- **Domain** (Core) — Contains business entities (`Adres`, `AfvalOphaalMoment`), value objects (`AfvalType`, `AfvalVerwerker`), and the **outbound port interfaces** (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`, `IAfvalKalenderSynchronisator`). Zero NuGet dependencies.
- **Application** (Core) — Implements use-case orchestration. Defines the **inbound port** (`ICommandHandler<TCommand, TResult>`) and its implementation (`VerwerkKalenderCommandHandler`). Orchestrates the workflow by calling outbound ports.
- **Infrastructure** (Driven Adapters) — Concrete implementations of the outbound ports. Includes `TwenteMilieuApi` (HTTP), `EfAfvalRepository` (SQLite/EF Core), `IcsExporter` (Ical.Net), `CacherendeAfvalApi` (24h file-based cache decorator), and `WebDavSyncAdapter` (HTTP PUT to a WebDAV/CalDAV endpoint).
- **Presentation** (Driving Adapters) — Entry points: `ConsoleUI` (ANSI/CLI), `DesktopUI` (Avalonia/GUI), and `AndroidUI` (MAUI). All UIs are decoupled from application logic, interacting only through the `ICommandHandler` interface.

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
│    IIcsExporter,                     │  CacherendeAfvalApi  │
│    IAfvalKalenderSynchronisator      │  WebDavSyncAdapter   │
└──────────────────────────────────────┴──────────────────────┘
```

### Commands pattern (Light CQRS)

We use a hand-rolled command pattern to avoid a hard dependency on MediatR while gaining its benefits:
- `ICommandHandler<TCommand, TResult>`: Generic interface for all use cases.
- `VerwerkKalenderCommand`: Immutable `record` carrying user input.
- `VerwerkKalenderCommandHandler`: Stateless orchestrator.

Both UIs resolve the handler via Dependency Injection, allowing for easy testing and the addition of cross-cutting concerns (logging, validation) via decorators.

### Core workflow (`VerwerkKalenderCommandHandler.HandleAsync`)

1. Receives `VerwerkKalenderCommand` (postcode, huisnummer, jaar, herinneringUur, outputPad, companyCode).
2. `IAfvalApi.HaalUniekAdresIdOpAsync` → Fetches the internal Ximmio `UniqueId`.
3. `IAfvalApi.HaalKalenderOpAsync` → Retrieves collection dates for the year.
4. `IAfvalRepository.SlaOpOfUpdateAsync` → Persists to SQLite, tracking `LaatstGewijzigd` for changes.
5. `IIcsExporter.ExporteerAsync` → Generates the `.ics` file with relative reminder triggers.

### Key design decisions

- **Dutch Ubiquitous Language**: Domain entities and methods use Dutch names to match the problem domain.
- **SQLite with EnsureCreated**: No EF migrations; the database is created dynamically in the application's local data folder.
- **SSL Bypass**: The Ximmio API often uses self-signed certificates; `HttpClient` is configured to accept them safely for this specific use case.
- **Idempotent Updates**: Scheduling changes are detected by comparing descriptions; only changed records trigger a `LaatstGewijzigd` update.
- **MAUI 10 / Android 16 Readiness**: `AndroidUI` uses modern `CreateWindow` patterns and compiled bindings (`x:DataType`) for performance.
- **API Caching**: `CacherendeAfvalApi` decorates the API port to provide 24-hour file-based caching, respecting the `companyCode` for multi-provider support.
- **WebDAV Sync**: `WebDavSyncAdapter` implements `IAfvalKalenderSynchronisator`. It generates a temporary ICS file via `IIcsExporter`, then uploads it to any WebDAV/CalDAV server using an HTTP PUT request with optional Basic Authentication. Temporary files are cleaned up in a `finally` block to guarantee no leaks.

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
- **Mocking**: mock outbound ports (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`, `IAfvalKalenderSynchronisator`) via Moq; never mock domain entities
- **Repository tests** use EF Core InMemory; **ViewModel tests** use `Avalonia.Headless` with a mocked `ICommandHandler<,>`
- **Handler tests** mock the three outbound ports (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`) and test the handler directly
- **Sync adapter tests** use `Moq.Protected` to intercept `HttpMessageHandler.SendAsync`, verifying PUT method, URI, Basic Auth header, and response error propagation

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

### ADR-002 — WebDAV/CalDAV sync via IAfvalKalenderSynchronisator

**Status:** Accepted

**Context:** Users want to push their waste collection calendar directly to a remote CalDAV server (e.g., Nextcloud, Baikal, Radicale) rather than importing a static `.ics` file each time. The sync operation is a secondary, optional output channel that sits alongside the existing file export.

**Decision:** Introduce a new outbound port `IAfvalKalenderSynchronisator` in the Domain layer, with a single method `SynchroniseerAsync`. Implement it as `WebDavSyncAdapter` in the Infrastructure layer. The adapter:
1. Delegates ICS generation to the existing `IIcsExporter` (writing to a temp file).
2. Reads the generated ICS content and sends it as a `text/calendar` HTTP PUT to the supplied WebDAV URL.
3. Optionally attaches a Basic Auth header when credentials are provided.
4. Always deletes the temp file in a `finally` block.

All three UIs register the adapter via `AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()` with the same SSL bypass handler used for the Ximmio API.

**Consequences:**
- Any WebDAV/CalDAV server (Nextcloud, Baikal, Radicale, iCloud, etc.) is supported without additional dependencies.
- The sync port is independently injectable and testable — `IIcsExporter` and `HttpMessageHandler` are mocked separately.
- Credentials are passed at call time; no persistent credential store is introduced.
- Future HTTPS-only enforcement or OAuth can be added as a decorator without changing the adapter.
