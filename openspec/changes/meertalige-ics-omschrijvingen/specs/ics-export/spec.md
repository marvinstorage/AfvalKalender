## MODIFIED Requirements

### Requirement: One event per moment
The exporter SHALL write one VEVENT per AfvalOphaalMoment to the given path (overwriting an existing file), starting at 08:00 and ending at 09:00 on Datum, with Summary equal to the description of the moment's AfvalType in the requested Taal. For Nederlands this equals the stored Omschrijving.

#### Scenario: Two moments
- **WHEN** two moments are exported in Nederlands
- **THEN** the file contains two events from 08:00 to 09:00 on their dates, with the stored descriptions as summary

#### Scenario: English export
- **WHEN** a GRIJS moment is exported in Engels
- **THEN** the event summary is "General waste is collected"

### Requirement: Reminder alarm
Each event SHALL have one display alarm with description `<prefix> <summary>`, where the prefix follows the requested Taal (`Herinnering:` or `Reminder:`), and trigger `-PT<herinneringUurVooraf>H`, i.e. that many hours before the event start.

#### Scenario: Reminder 13 hours
- **WHEN** herinneringUurVooraf is 13
- **THEN** the trigger is -PT13H

#### Scenario: English reminder text
- **WHEN** a GROEN moment is exported in Engels
- **THEN** the alarm description is "Reminder: Organic waste (GFT) is collected"

## ADDED Requirements

### Requirement: UID is language independent
The UID of an event SHALL NOT depend on the Taal, so re-importing a calendar after a language change updates the existing events.

#### Scenario: Language switch
- **WHEN** the same moment is exported in Nederlands and in Engels
- **THEN** both files hold the same UID
