## ADDED Requirements

### Requirement: Sync uses the chosen language
The handler SHALL pass the command's Taal to the synchronisator, and the WebDAV adapter MUST render the uploaded ICS in that language.

#### Scenario: English WebDAV upload
- **WHEN** a WebDAV sync runs with Taal Engels
- **THEN** the ICS exporter is called with Engels for the temporary file
