## ADDED Requirements

### Requirement: Language selector
The Desktop view model SHALL expose the available languages and a selected language, initialised from the system UI language, and put the selection in the command.

#### Scenario: Initial language
- **WHEN** the view model is created on an English system
- **THEN** the selected language is Engels

#### Scenario: Command contains language
- **WHEN** the user processes with Engels selected
- **THEN** the command has Taal Engels
