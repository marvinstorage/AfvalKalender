# afval-ophaalmomenten Specification

## Purpose
Defines the Domain entities `AfvalOphaalMoment` and `Adres`, the `AfvalType` value object, change detection through `Update()` and the domain events raised by the entity. Domain code has no framework dependencies.

## Requirements

### Requirement: AfvalOphaalMoment creation
An `AfvalOphaalMoment` SHALL be constructed from a `Type`, `Datum`, `Omschrijving`, `Postcode` and `Huisnummer`. On construction it MUST set `LaatstGewijzigd` to the current local time and raise exactly one `AfvalOphaalMomentToegevoegd` event carrying the same values.

#### Scenario: New moment
- **WHEN** an AfvalOphaalMoment is created for type GRIJS, a date, postcode 1234AB and huisnummer 10
- **THEN** its properties equal the given values
- **AND** `DomainEvents` contains one `AfvalOphaalMomentToegevoegd`

### Requirement: Update with change detection
`Update(omschrijving)` SHALL change the entity only when the new Omschrijving differs from the current one. When it differs, the entity MUST store the new Omschrijving, set `LaatstGewijzigd` to the current local time and raise one `AfvalOphaalMomentGewijzigd` event with the old and new Omschrijving. When it is identical, nothing SHALL change and no event SHALL be raised.

#### Scenario: Changed description
- **WHEN** Update is called with a different Omschrijving
- **THEN** Omschrijving and LaatstGewijzigd are updated
- **AND** an `AfvalOphaalMomentGewijzigd` event is added

#### Scenario: Same description
- **WHEN** Update is called with the current Omschrijving
- **THEN** LaatstGewijzigd is unchanged and no event is added

### Requirement: Domain events
`AfvalOphaalMomentToegevoegd` and `AfvalOphaalMomentGewijzigd` SHALL be immutable records implementing `IDomainEvent`, whose `OccurredOn` is set to the UTC time of creation. The entity MUST expose its events as a read-only collection and offer `ClearDomainEvents()`. Events SHALL NOT be dispatched by the Domain itself (see persistence for the outbox).

#### Scenario: Clearing events
- **WHEN** `ClearDomainEvents()` is called
- **THEN** `DomainEvents` is empty

### Requirement: AfvalType values
`AfvalType` SHALL be an enum with the values GRIJS, GROEN, PAPIER, VERPAKKINGEN, KERSTBOOM and ONBEKEND.

#### Scenario: Unknown type
- **WHEN** a pickup type cannot be mapped
- **THEN** the value ONBEKEND is used

### Requirement: Adres
An `Adres` SHALL require a non-blank Postcode and a non-blank Huisnummer, and MAY carry an optional `UniekId` that can be set afterwards via `SetUniekId`.

#### Scenario: Valid address
- **WHEN** an Adres is created with postcode 1234AB and huisnummer 10
- **THEN** it is created with UniekId null

#### Scenario: Missing postcode or huisnummer
- **WHEN** an Adres is created with an empty or whitespace Postcode or Huisnummer
- **THEN** an ArgumentException is thrown

### Requirement: Language does not change stored data
The language chosen in `VerwerkKalenderCommand` SHALL only affect generated output (ICS file and sync). The stored `Omschrijving`, `LaatstGewijzigd` and the domain events MUST NOT depend on it.

#### Scenario: English run
- **WHEN** a command with Taal Engels is handled for a moment already stored
- **THEN** the stored Omschrijving is unchanged and no AfvalOphaalMomentGewijzigd event is raised

#### Scenario: Handler passes the language
- **WHEN** a command with Taal Engels is handled
- **THEN** the ICS exporter is called with Engels
