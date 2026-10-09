## ADDED Requirements

### Requirement: Language does not change stored data
The language chosen in `VerwerkKalenderCommand` SHALL only affect generated output (ICS file and sync). The stored `Omschrijving`, `LaatstGewijzigd` and the domain events MUST NOT depend on it.

#### Scenario: English run
- **WHEN** a command with Taal Engels is handled for a moment already stored
- **THEN** the stored Omschrijving is unchanged and no AfvalOphaalMomentGewijzigd event is raised

#### Scenario: Handler passes the language
- **WHEN** a command with Taal Engels is handled
- **THEN** the ICS exporter is called with Engels
