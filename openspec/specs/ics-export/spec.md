# ics-export Specification

## Purpose
Defines `IcsExporter` (port `IIcsExporter`), which writes an RFC 5545 calendar file using Ical.Net.

## Requirements

### Requirement: One event per moment
The exporter SHALL write one VEVENT per AfvalOphaalMoment to the given path (overwriting an existing file), starting at 08:00 and ending at 09:00 on Datum, with Summary equal to Omschrijving.

#### Scenario: Two moments
- **WHEN** two moments are exported
- **THEN** the file contains two events from 08:00 to 09:00 on their dates

### Requirement: Stable UID
The UID of each event SHALL be `<Type>_<yyyyMMdd>_<Postcode>`, so re-importing does not create duplicates.

#### Scenario: Same moment twice
- **WHEN** the same moment is exported in two runs
- **THEN** both files hold the same UID

### Requirement: Reminder alarm
Each event SHALL have one display alarm with description `Herinnering: <Omschrijving>` and trigger `-PT<herinneringUurVooraf>H`, i.e. that many hours before the event start.

#### Scenario: Reminder 13 hours
- **WHEN** herinneringUurVooraf is 13
- **THEN** the trigger is -PT13H

### Requirement: Empty list
An empty list SHALL produce a valid calendar without events.

#### Scenario: No moments
- **WHEN** no moments are exported
- **THEN** the file is written without VEVENT entries

### Requirement: Output location
The Console and Desktop UIs SHALL name the file `AfvalKalender_<postcode>_<huisnummer>_<jaar>.ics` in the working directory; Android writes the same name in the app cache directory.

#### Scenario: File name
- **WHEN** postcode 1234AB, huisnummer 10 and year 2026 are processed
- **THEN** the name is AfvalKalender_1234AB_10_2026.ics
