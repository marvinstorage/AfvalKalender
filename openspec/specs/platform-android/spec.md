# platform-android Specification

## Purpose
Android-only (.NET MAUI) decisions and behaviour. This is the only platform-specific spec; the others are platform-neutral.
## Requirements

### Requirement: Packaging
The app SHALL target `net10.0-android` with ApplicationId `com.companyname.afvalkalender`, title AfvalKalender, version 1.0.1 (code 2) and minimum Android API 21. Releases are sideloaded APKs.

#### Scenario: Project settings
- **WHEN** the csproj is read
- **THEN** it contains these values

### Requirement: Permissions
The manifest SHALL request INTERNET and legacy storage permissions (READ up to API 32, WRITE up to API 29) and allow cleartext traffic.

#### Scenario: Manifest
- **WHEN** the manifest is inspected
- **THEN** these permissions are present

### Requirement: Storage locations
The database SHALL be `afvalkalender.db` in `FileSystem.AppDataDirectory`, the API cache `apicache` in `FileSystem.CacheDirectory`, and the ICS file in the cache directory (falling back to the system temp path when unavailable, e.g. in unit tests).

#### Scenario: ICS path
- **WHEN** the user processes an address
- **THEN** the file is written in the cache directory

### Requirement: View model behaviour
`MainPageViewModel` SHALL offer the same fields and defaults as the Desktop view model (first AfvalVerwerker, current year, 13 hours), reject blank postcode or huisnummer with a Dutch error, normalise the postcode, and report success or "Fout: <message>".

#### Scenario: Blank input
- **WHEN** huisnummer is blank
- **THEN** StatusBericht starts with "Fout" and the handler is not called

### Requirement: Share
After success the UI SHALL show an "ICS Bestand Delen" button that opens the Android share sheet with the ICS file titled "Deel Afvalkalender".

#### Scenario: Share
- **WHEN** the user taps the button with a result available
- **THEN** a ShareFileRequest for the file is made
