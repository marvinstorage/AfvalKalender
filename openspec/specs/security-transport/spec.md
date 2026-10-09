# security-transport Specification

## Purpose
Documents the current transport-security behaviour honestly. KNOWN GAP (no requirement is claimed): in all three UIs the typed `HttpClient` for the Ximmio API is configured with `DangerousAcceptAnyServerCertificateValidator`, and the WebDAV client with a callback that accepts every certificate, so TLS certificate validation is disabled for these clients. CLAUDE.md justifies it for Ximmio with a self-signed certificate; for WebDAV there is no such justification, and credentials are sent with Basic Authentication over that connection. Android additionally allows cleartext traffic. The Google and Microsoft Graph clients use default validation. This spec only lists what is true today.

## Requirements

### Requirement: Secrets are not persisted
The WebDAV username and password SHALL only be passed in the in-memory command and SyncConfiguratie; the application MUST NOT write them to SQLite, the API cache or the ICS file.

#### Scenario: After a run
- **WHEN** a run with WebDAV credentials has finished
- **THEN** the database and cache contain no credentials

### Requirement: Masked password input
The console UI SHALL mask the WebDAV password and the Android UI SHALL mark the field as a password entry.

#### Scenario: Console prompt
- **WHEN** the password is typed
- **THEN** it is not echoed

### Requirement: Basic authentication only with a username
The WebDAV adapter SHALL send an Authorization header only when a username is supplied.

#### Scenario: No username
- **WHEN** the username is empty
- **THEN** the PUT carries no Authorization header
