## ADDED Requirements

### Requirement: Supported languages
The Domain SHALL define a value object `Taal` with the values `Nederlands` and `Engels`. `Nederlands` MUST be the default everywhere a language is optional.

#### Scenario: Default language
- **WHEN** a command is created without a language
- **THEN** its Taal is Nederlands

### Requirement: Description per type and language
For every `AfvalType` and every `Taal` there SHALL be exactly one event description. The Dutch descriptions MUST equal the descriptions the provider adapter produces today. The English descriptions are: GRIJS "General waste is collected", GROEN "Organic waste (GFT) is collected", PAPIER "Paper is collected", VERPAKKINGEN "Plastic and drink cartons are collected", KERSTBOOM "Christmas tree is collected", ONBEKEND "Waste is collected".

#### Scenario: English grey waste
- **WHEN** the description of GRIJS is requested in Engels
- **THEN** the result is "General waste is collected"

#### Scenario: Dutch is unchanged
- **WHEN** the description of PAPIER is requested in Nederlands
- **THEN** the result is "Oud papier wordt opgehaald"

#### Scenario: Every type is covered
- **WHEN** all AfvalType values are combined with all Taal values
- **THEN** each combination yields a non-empty description

### Requirement: Alarm prefix per language
The reminder text SHALL start with "Herinnering:" for Nederlands and "Reminder:" for Engels.

#### Scenario: English reminder
- **WHEN** the language is Engels
- **THEN** the alarm description starts with "Reminder:"

### Requirement: Default from system language
Each UI SHALL preselect Engels when the system UI language is English and Nederlands otherwise. The user MUST be able to change the selection before processing.

#### Scenario: English system
- **WHEN** the UI starts on a system with UI language en-GB
- **THEN** Engels is preselected

#### Scenario: Other system language
- **WHEN** the UI starts on a system with UI language de-DE
- **THEN** Nederlands is preselected
