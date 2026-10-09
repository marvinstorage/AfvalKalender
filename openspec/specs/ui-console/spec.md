# ui-console Specification

## Purpose
Defines the interactive terminal UI in `AfvalKalender.ConsoleUI`, built with Spectre.Console (ADR-007). It is a thin driving adapter over `ICommandHandler`.

## Requirements

### Requirement: Interactive input flow
The app SHALL show a header and then ask in order: AfvalVerwerker (selection list of all 16, page size 10), postcode, huisnummer, year (default current year), reminder hours (default 13), and whether to configure WebDAV/CalDAV (default no; when yes URL, username and masked password).

#### Scenario: Defaults
- **WHEN** the user presses Enter on the year and hour prompts
- **THEN** the current year and 13 are used

### Requirement: Postcode normalisation
The postcode SHALL have spaces removed and be upper-cased before it is used.

#### Scenario: Spaced input
- **WHEN** the user enters "1234 ab"
- **THEN** the command contains 1234AB

### Requirement: Command construction
The app SHALL build a `VerwerkKalenderCommand` with the CompanyCode of the selection, ForceerVernieuwen false, output file `AfvalKalender_<postcode>_<huisnummer>_<jaar>.ics` in the working directory, and SyncProvider WebDav only if WebDAV was configured. A spinner status is shown while it runs.

#### Scenario: Run
- **WHEN** the user completes the prompts
- **THEN** the handler is called once with those values

### Requirement: Result display
On success the app SHALL print the number of moments and the absolute path of the ICS file, and show a table (max 10 rows, ordered by date, starting from yesterday) of Datum and Afvaltype with colour per type.

#### Scenario: Upcoming moments
- **WHEN** more than 10 moments are on or after yesterday
- **THEN** only the first 10 are shown

### Requirement: Error display
Any exception from the handler SHALL be shown as a red message with the exception text; the app then exits normally.

#### Scenario: Validation failure
- **WHEN** the handler throws
- **THEN** "Er is een fout opgetreden" with the message is printed

### Requirement: Composition
`Program.cs` SHALL register infrastructure, the validating decorator and ConsoleApp, call `EnsureCreated()` and run the app. The ForceerVernieuwen option is not exposed in this UI.

#### Scenario: Start
- **WHEN** the program starts
- **THEN** the database exists before the prompts appear
