# afvalverwerkers Specification

## Purpose
Defines the `AfvalVerwerker` value object and the static list of supported waste processors. All processors use the same Ximmio endpoint and are distinguished only by their `CompanyCode`.

## Requirements

### Requirement: AfvalVerwerker value object
An `AfvalVerwerker` SHALL be an immutable record with `Id`, `Naam` and `CompanyCode` (a GUID string).

#### Scenario: Structural equality
- **WHEN** two AfvalVerwerker values have the same Id, Naam and CompanyCode
- **THEN** they are equal

### Requirement: Supported processors
`AfvalVerwerkers.Alle` SHALL list exactly 16 entries with Ids: twentemilieu, acv, almere, areareiniging, avalex, avri, blink, hellendoorn, meerlanden, oostzaan, rad, venlo, waardlanden, westland, woerden, ximmio. Twente Milieu SHALL be the first entry. The entries `meerlanden` and `ximmio` currently share the same CompanyCode.

#### Scenario: Listing
- **WHEN** a UI reads `AfvalVerwerkers.Alle`
- **THEN** it receives 16 processors with Twente Milieu first

### Requirement: Default CompanyCode
When no CompanyCode is supplied, the `VerwerkKalenderCommand` and the `IAfvalApi` methods SHALL default to the Twente Milieu CompanyCode.

#### Scenario: Default
- **WHEN** a command is created without a CompanyCode
- **THEN** it carries the Twente Milieu CompanyCode

### Requirement: Selection in the UIs
Every UI SHALL let the user choose an AfvalVerwerker and SHALL pass the CompanyCode of the selection in the command. The Desktop and Android view models MUST preselect the first entry (Twente Milieu); the console prompts for a choice before asking other input.

#### Scenario: Default selection
- **WHEN** the Desktop or Android view model is created
- **THEN** GeselecteerdeVerwerker is Twente Milieu
