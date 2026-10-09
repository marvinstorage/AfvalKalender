# api-cache Specification

## Purpose
Defines `CacherendeAfvalApi`, a decorator around `IAfvalApi` that caches responses in JSON files for 24 hours (ADR-005).

## Requirements

### Requirement: Cache lifetime
Cached results SHALL be reused for 24 hours after `OpgeslagenOp`. The clock is injectable (`Func<DateTime>`, default UTC now). An older entry MUST be treated as missing and the inner API called again.

#### Scenario: Within TTL
- **WHEN** the same address id is requested twice within 24 hours
- **THEN** the inner API is called once

#### Scenario: Expired
- **WHEN** the entry is older than 24 hours
- **THEN** the inner API is called again and the file is rewritten

### Requirement: Cache keys
Files SHALL be named `adresid_<companyCode>_<postcode>_<huisnummer>.json` and `kalender_<companyCode>_<postcode>_<huisnummer>_<jaar>.json` in the cache directory, which is created on construction. Different companyCode, address or year MUST use separate files.

#### Scenario: Different addresses
- **WHEN** two different addresses are requested
- **THEN** two cache files exist and each address gets its own data

### Requirement: ForceerVernieuwen
When `forceerVernieuwen` is true the decorator SHALL skip reading the cache, call the inner API and overwrite the cache entry.

#### Scenario: Forced refresh
- **WHEN** forceerVernieuwen is true within the TTL
- **THEN** the inner API is called again

### Requirement: Calendar round trip
Cached calendars SHALL store Type (as name), Datum, Omschrijving, Postcode and Huisnummer per moment and restore them as AfvalOphaalMomenten. An unparseable type name MUST become ONBEKEND.

#### Scenario: All types
- **WHEN** a cached calendar contains all AfvalType values
- **THEN** all are restored correctly

### Requirement: Best-effort cache
Unreadable or corrupt cache files SHALL be treated as a miss, and write failures MUST NOT surface to the caller.

#### Scenario: Corrupt file
- **WHEN** the cache file contains invalid JSON
- **THEN** the inner API is called and no error reaches the caller

### Requirement: Cache location per UI
Console and Desktop SHALL use a relative `apicache` directory; Android SHALL use `apicache` inside the app cache directory. The cache directory MUST NOT be committed (see architecture-governance).

#### Scenario: Android
- **WHEN** the Android app builds its cache
- **THEN** the path is under the platform cache directory
