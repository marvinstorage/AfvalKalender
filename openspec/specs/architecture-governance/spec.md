# architecture-governance Specification

## Purpose
Records the architectural and documentation rules of AfvalKalender, taken from CLAUDE.md, `.agents/AGENTS.md` and `openspec/config.yaml`.

## Requirements

### Requirement: Layering and dependency flow
The solution SHALL keep the flow Presentation to Application to Domain, with Infrastructure depending on Domain. Domain has no NuGet dependencies and defines the outbound ports (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`, `IAfvalKalenderSynchronisator`); Infrastructure implements them; the UIs resolve `ICommandHandler` from DI.

#### Scenario: New outbound capability
- **WHEN** a new external system is added
- **THEN** a port is defined in Domain first and an adapter in Infrastructure

### Requirement: Dutch ubiquitous language
Domain names, methods and events SHALL be in Dutch, and tests SHALL be named `Method_Scenario_ExpectedResult` in Dutch.

#### Scenario: New event
- **WHEN** a domain event is added
- **THEN** it has a past-tense Dutch name

### Requirement: Third-party library policy
New packages SHALL be justified in order: custom code, BCL, MIT or Apache 2.0, commercial only after approval. No MediatR.

#### Scenario: New package
- **WHEN** a NuGet package is proposed
- **THEN** it is justified against this order

### Requirement: ADRs
Long-term architectural decisions SHALL be recorded in `docs/adr/ADR-NNN-Title.md` (3-digit number); currently ADR-001 to ADR-007 exist and the ADR table in CLAUDE.md MUST be updated.

#### Scenario: New decision
- **WHEN** an ADR is added
- **THEN** CLAUDE.md lists it

### Requirement: Feature documentation rules
Implementing a new feature SHALL include unit tests, README.md updates, architecture documentation or diagram updates and an ADR where applicable. Specs under `openspec/specs/` are updated with behaviour changes.

#### Scenario: New feature
- **WHEN** a feature is implemented
- **THEN** tests, README, architecture docs and specs are updated

### Requirement: Privacy of repository content
The repository MUST NOT contain real Dutch postcodes or addresses (in tests, API responses, ICS files or specs), `.db` files or `apicache/` files. `.gitignore` SHALL ignore `*.db` and `apicache/`. Commits carry no AI attribution trailer per openspec rules.

#### Scenario: Test data
- **WHEN** a test needs an address
- **THEN** it uses a fake value such as 1234AB

### Requirement: Test conventions
Tests SHALL use xUnit, FluentAssertions and Moq, never mock domain entities, mock outbound ports, use EF InMemory for repositories and a fake clock for cache tests. Test projects are run one by one (UnitTests, Infrastructure.Tests, DesktopUI.Tests).

#### Scenario: Cache test
- **WHEN** a cache TTL is tested
- **THEN** an injected clock controls time
