# pdf-export Specification

## Purpose
Defines the optional printable A4 year schedule (PDF) that is written next to the ICS export from the same persisted collection moments, in the chosen language.

## Requirements

### Requirement: Year schedule on A4
The `PdfExporter` (port `IPdfExporter`) SHALL write a valid single-page A4 PDF to the given path (overwriting an existing file) that shows the twelve months of the requested year, each with a weekday header and the day numbers, and marks every collection day with the colour of its AfvalType.

#### Scenario: Valid file
- **WHEN** moments for 2026 are exported
- **THEN** the file starts with `%PDF-`, ends with `%%EOF`, has an A4 MediaBox and contains one page

#### Scenario: Colour per type
- **WHEN** a GRIJS and a GROEN moment are exported
- **THEN** the page uses a different fill colour for each

### Requirement: Language
Title, month names, weekday initials, legend and waste type names SHALL follow the requested Taal.

#### Scenario: English
- **WHEN** a GRIJS moment is exported in Engels
- **THEN** the legend contains "General waste" and the months are English

#### Scenario: Dutch
- **WHEN** a GRIJS moment is exported in Nederlands
- **THEN** the legend contains "Restafval" and the months are Dutch

### Requirement: Legend
The page SHALL contain a legend with one entry for each AfvalType that occurs in the exported moments.

#### Scenario: Two types
- **WHEN** only GRIJS and PAPIER moments are exported
- **THEN** the legend has exactly those two entries

### Requirement: Empty list
An empty list SHALL still produce a valid PDF with the twelve months and no marked days.

#### Scenario: No moments
- **WHEN** an empty list is exported for 2026
- **THEN** a valid PDF is written and the legend has no entries

### Requirement: Text safety
Text written into the PDF MUST have `(`, `)` and `\` escaped so that values such as a house number suffix cannot break the file.

#### Scenario: Special characters
- **WHEN** a house number contains `(` or `\`
- **THEN** the file is still structurally valid
