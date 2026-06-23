# ADR-002 — WebDAV/CalDAV sync via IAfvalKalenderSynchronisator

**Status:** Implemented (Accepted on 2026-06-19, UI integration completed)  
**Date:** 2026-06-19  
**Deciders:** Marvin Storage

---

## Context

Users want to push their waste collection calendar directly to a remote CalDAV server
(e.g., Nextcloud, Baikal, Radicale) rather than importing a static `.ics` file manually
each time. The sync operation is a secondary, optional output channel that must sit
alongside the existing file export — not replace it.

---

## Decision

Introduce a new **outbound port** `IAfvalKalenderSynchronisator` in the Domain layer
with a single method:

```csharp
Task SynchroniseerAsync(
    IEnumerable<AfvalOphaalMoment> momenten,
    string webDavUrl,
    string gebruikersnaam,
    string wachtwoord,
    int herinneringUur);
```

Implement it as `WebDavSyncAdapter` in the Infrastructure layer
(`AfvalKalender.Infrastructure/Sync/`). The adapter:

1. Delegates ICS generation to the existing `IIcsExporter` (written to a temp file via `Path.GetTempPath()`).
2. Reads the generated ICS content into memory.
3. Issues an HTTP PUT to the supplied WebDAV URL with `Content-Type: text/calendar`.
4. Optionally attaches a `Basic` `Authorization` header when credentials are provided.
5. Always deletes the temp file in a `finally` block — guaranteed cleanup on both success and failure.

All three UIs register the adapter via:

```csharp
services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
```

This reuses the same SSL-bypass handler already used for the Ximmio API.

---

## Consequences

### Positive
- Any WebDAV/CalDAV server (Nextcloud, Baikal, Radicale, iCloud DAV, etc.) is
  supported without any new third-party dependencies.
- The sync port is independently injectable and testable: `IIcsExporter` and
  `HttpMessageHandler` are mocked separately via `Moq.Protected`.
- Credentials are passed at call time — no persistent credential store is needed.
- The Decorator pattern can be applied later (HTTPS-only enforcement, OAuth token
  exchange) without changing the adapter itself.

### Negative / Trade-offs
- Credentials transmitted as Basic Auth (Base64 of username:password) — callers are
  responsible for using HTTPS to prevent credential exposure.
- No retry/back-off strategy; a transient network failure will surface as an
  `HttpRequestException` to the caller.
- Temporary ICS files are written to `Path.GetTempPath()` — on Android this is the
  platform cache directory and requires no special permission, but it means a second
  ICS write pass (one for local export, one for WebDAV sync).
