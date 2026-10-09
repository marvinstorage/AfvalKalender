# command-validation Specification

## Purpose
Defines the `VerwerkKalenderCommand` input contract and the validation decorator that guards the handler (ADR-001, ADR-003).

## Requirements

### Requirement: Command fields
`VerwerkKalenderCommand` SHALL be an immutable record with Postcode, Huisnummer, Jaar, HerinneringUur, OutputPad, CompanyCode (default Twente Milieu), ForceerVernieuwen (default false), SyncProvider (default Geen), SyncDoelUrlOfToken, SyncGebruiker and SyncWachtwoord (all default null).

#### Scenario: Defaults
- **WHEN** a command is created with only the five required values
- **THEN** ForceerVernieuwen is false, SyncProvider is Geen and the sync fields are null

### Requirement: Validation rules
The `VerwerkKalenderCommandValidator` MUST reject a command with an ArgumentException (ArgumentNullException for a null command) when: Postcode is blank or does not match `^[1-9][0-9]{3}[A-Z]{2}$`; Huisnummer is blank; Jaar is outside 2000-2100; HerinneringUur is outside 0-23; OutputPad is blank; CompanyCode is not a parseable GUID. Messages are in Dutch.

#### Scenario: Valid command
- **WHEN** postcode 1234AB, huisnummer 10, jaar 2026, uur 13, a path and a valid GUID are validated
- **THEN** no exception is thrown

#### Scenario: Postcode with space or lowercase
- **WHEN** the postcode is "1234 AB" or "1234ab"
- **THEN** an ArgumentException is thrown

#### Scenario: Out-of-range values
- **WHEN** Jaar is 1999 or 2101, or HerinneringUur is -1 or 24
- **THEN** an ArgumentException is thrown

#### Scenario: Invalid CompanyCode
- **WHEN** CompanyCode is not a GUID
- **THEN** an ArgumentException is thrown

### Requirement: Validation before handling
`ValidatingCommandHandlerDecorator` SHALL call the validator before the inner handler and MUST NOT call the inner handler when validation throws. The validator does not check the sync fields.

#### Scenario: Invalid command
- **WHEN** an invalid command is handled through the decorator
- **THEN** the exception propagates and no API, repository, export or sync call is made

### Requirement: Postcode normalisation by the UI
Because the validator is strict, every UI SHALL strip spaces and upper-case the postcode before building the command.

#### Scenario: Spaced input
- **WHEN** the user types "1234 ab"
- **THEN** the command carries "1234AB"
