# persistence Specification

## Purpose
Defines SQLite persistence through EF Core (`AfvalDbContext`, `EfAfvalRepository`) and the transactional outbox for domain events (ADR-004).

## Requirements

### Requirement: Idempotent upsert
`SlaOpOfUpdateAsync` SHALL identify an existing record by Postcode, Huisnummer, Datum (date part) and Type. A missing record MUST be inserted; an existing record MUST only have `Update(omschrijving)` called, so unchanged records are not modified.

#### Scenario: Re-run with same data
- **WHEN** the same moments are saved twice
- **THEN** no duplicates are created and no new events are raised

#### Scenario: Changed description
- **WHEN** a known moment arrives with a different Omschrijving
- **THEN** the stored record is updated and LaatstGewijzigd changes

### Requirement: Query
`HaalOpVoorAdresEnJaarAsync` SHALL return the stored moments for the exact postcode, huisnummer and year of Datum. `IQueryable` MUST NOT be exposed.

#### Scenario: Other year
- **WHEN** moments of another year exist
- **THEN** they are not returned

### Requirement: Transactional outbox
In the same `SaveChanges` call that stores the moments, every domain event of tracked AfvalOphaalMomenten SHALL be written as an `OutboxMessage` (EventType full type name, Content JSON, OccurredOn, ProcessedOn null) and the events cleared on the entity. The system currently has no processor for outbox messages; ProcessedOn stays null.

#### Scenario: New moment
- **WHEN** a new moment is saved
- **THEN** an OutboxMessage for AfvalOphaalMomentToegevoegd exists

#### Scenario: Changed moment
- **WHEN** a moment's description changes
- **THEN** an OutboxMessage for AfvalOphaalMomentGewijzigd exists

### Requirement: Schema creation
The three UIs SHALL call `Database.EnsureCreated()` at startup and use a SQLite file `afvalkalender.db`. The repository contains one EF migration (`AddOutboxMessagesTable`) and a model snapshot, but startup does not apply migrations. Locations: Console in the project root (derived from the base directory), Desktop in the working directory, Android in the app data directory.

#### Scenario: First start
- **WHEN** the app starts without a database file
- **THEN** the file is created with both tables

### Requirement: Required columns
Type, Datum, Postcode and Huisnummer of AfvalOphaalMoment and EventType, Content and OccurredOn of OutboxMessage SHALL be required.

#### Scenario: Model
- **WHEN** the EF model is inspected
- **THEN** those properties are not nullable
