# ics-export Specification

## Purpose
Defines `IcsExporter` (port `IIcsExporter`), which writes an RFC 5545 calendar file using Ical.Net.

## Requirements

### Requirement: One event per moment
The exporter SHALL write one VEVENT per AfvalOphaalMoment to the given path (overwriting an existing file), starting at 08:00 and ending at 09:00 on Datum, with Summary equal to the description of the moment's AfvalType in the requested Taal. For Nederlands this equals the stored Omschrijving.

#### Scenario: Two moments
- **WHEN** two moments are exported in Nederlands
- **THEN** the file contains two events from 08:00 to 09:00 on their dates, with the stored descriptions as summary

#### Scenario: English export
- **WHEN** a GRIJS moment is exported in Engels
- **THEN** the event summary is "General waste is collected"

### Requirement: Stable UID
The UID of each event SHALL be `<Type>_<yyyyMMdd>_<Postcode>`, so re-importing does not create duplicates.

#### Scenario: Same moment twice
- **WHEN** the same moment is exported in two runs
- **THEN** both files hold the same UID

### Requirement: Reminder alarm
Each event SHALL have one display alarm with description `<prefix> <summary>`, where the prefix follows the requested Taal (`Herinnering:` or `Reminder:`), and trigger `-PT<herinneringUurVooraf>H`, i.e. that many hours before the event start.

#### Scenario: Reminder 13 hours
- **WHEN** herinneringUurVooraf is 13
- **THEN** the trigger is -PT13H

#### Scenario: English reminder text
- **WHEN** a GROEN moment is exported in Engels
- **THEN** the alarm description is "Reminder: Organic waste (GFT) is collected"

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

### Requirement: UID is language independent
The UID of an event SHALL NOT depend on the Taal, so re-importing a calendar after a language change updates the existing events.

#### Scenario: Language switch
- **WHEN** the same moment is exported in Nederlands and in Engels
- **THEN** both files hold the same UID
