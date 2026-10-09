# calendar-sync Specification

## Purpose
Defines the optional push of the calendar to a remote target: `SyncProvider`, `SyncConfiguratie`, the Domain service `KalenderSynchronisatieService` and the adapters (ADR-002, ADR-006). Only WebDAV is functional; the Google and Microsoft adapters are stubs.

## Requirements

### Requirement: Provider selection
`SyncProvider` SHALL have the values Geen, WebDav, GoogleCalendar and MicrosoftGraph. `SyncConfiguratie` is a record of Provider, DoelUrlOfToken, Gebruikersnaam and Wachtwoord. The handler builds it from the command, using empty strings for null values.

#### Scenario: No sync
- **WHEN** the command has SyncProvider Geen
- **THEN** no synchronisator is called

### Requirement: KalenderSynchronisatieService dispatch
The service SHALL return immediately for Geen, otherwise call the first `IAfvalKalenderSynchronisator` whose `Ondersteunt(provider)` is true, and MUST throw NotSupportedException when none matches.

#### Scenario: Matching adapter
- **WHEN** the provider is WebDav
- **THEN** the WebDAV adapter's SynchroniseerAsync is called

#### Scenario: No adapter
- **WHEN** no registered adapter supports the provider
- **THEN** a NotSupportedException is thrown

### Requirement: Sync after export
The handler SHALL run synchronisation after the ICS export and with the moments read back from the repository.

#### Scenario: Order
- **WHEN** a command is handled
- **THEN** the order is API id, API calendar, save, query, ICS export, sync

### Requirement: WebDAV adapter
The WebDAV adapter SHALL reject a blank URL with ArgumentException, render the ICS to a temporary file, HTTP PUT the content as `text/calendar` to the URL, add Basic Authentication only when a username is given, call EnsureSuccessStatusCode, and always delete the temporary file.

#### Scenario: With credentials
- **WHEN** URL, user and password are given
- **THEN** a PUT is sent with a Basic Authorization header

#### Scenario: Server error
- **WHEN** the server answers 4xx or 5xx
- **THEN** an HttpRequestException is thrown

### Requirement: Google and Microsoft Graph adapters are stubs
The `GoogleCalendarSyncAdapter` and `MicrosoftGraphSyncAdapter` SHALL support their provider and set a Bearer header from DoelUrlOfToken but currently send no events (the mapping to the cloud APIs is not implemented; ADR-006 is Proposed). No UI offers these providers.

#### Scenario: Stub call
- **WHEN** the Google adapter is called
- **THEN** it completes without sending a request

### Requirement: UI mapping
Console and Desktop SHALL select WebDav when a WebDAV URL is given (Console asks via a confirmation, Desktop uses a non-blank URL) and Geen otherwise.

#### Scenario: Desktop without URL
- **WHEN** the WebDAV URL field is blank
- **THEN** SyncProvider is Geen

### Requirement: Sync uses the chosen language
The handler SHALL pass the command's Taal to the synchronisator, and the WebDAV adapter MUST render the uploaded ICS in that language.

#### Scenario: English WebDAV upload
- **WHEN** a WebDAV sync runs with Taal Engels
- **THEN** the ICS exporter is called with Engels for the temporary file
