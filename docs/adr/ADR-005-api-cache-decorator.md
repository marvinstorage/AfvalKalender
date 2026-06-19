# ADR-005 — 24-Hour File-Based API Cache with ForceerVernieuwen Bypass

**Status:** Accepted  
**Date:** 2025-12-01  
**Deciders:** Marvin Storage

---

## Context

The Ximmio API rate-limits repeated requests for the same address and year. Users who
run the application multiple times per day (e.g., debugging or testing) were hitting
`429 Too Many Requests` responses. The API data for a given year also changes
infrequently — at most a few times per season.

Redis, Memcached, and in-process `IMemoryCache` were evaluated but rejected: they add
infrastructure dependencies that are disproportionate to the problem scope. A
self-contained file-based approach was preferred.

---

## Decision

Introduce `CacherendeAfvalApi` in `AfvalKalender.Infrastructure/Cache/` as a
**Decorator** over `IAfvalApi`:

```
CacherendeAfvalApi (IAfvalApi)
    wraps → TwenteMilieuApi (IAfvalApi)
```

### Cache mechanics

- Cache directory: `apicache/` (configurable per platform — desktop uses current dir,
  Android uses `FileSystem.CacheDirectory`).
- Cache entry filename includes `companyCode`, `postcode`, `huisnummer` (and `jaar`
  for calendar entries) so entries are automatically separated per provider and address.
- Each file is a JSON-serialised `CacheEnvelop<T>` containing `OpgeslagenOp` (UTC
  timestamp) and `Inhoud` (the payload).
- A cached entry is valid for exactly **24 hours** from `OpgeslagenOp`.
- Cache writes are **best-effort**: any `IOException` is swallowed so a
  full filesystem never blocks the user.

### ForceerVernieuwen bypass

`IAfvalApi` exposes `bool forceerVernieuwen = false` on both methods.
When `true`, the decorator skips the cache read and always calls the inner API,
then overwrites the cache with the fresh response. This is exposed all the way up
through `VerwerkKalenderCommand.ForceerVernieuwen`.

---

## Consequences

### Positive
- Zero new runtime dependencies for caching.
- Works identically on Console, Desktop, and Android (platform-specific cache dir).
- `companyCode` in the filename means multi-provider caching works correctly
  without any additional key namespace management.
- Mockable clock (`Func<DateTime>` injection) enables deterministic unit testing
  of cache expiry without `Thread.Sleep`.

### Negative / Trade-offs
- Cache files are not encrypted; API response data (postcodes, address IDs) is
  stored as plain JSON on disk.
- No eviction strategy beyond TTL — old entries accumulate until the OS or user
  clears them.
- `ForceerVernieuwen` must be threaded through every layer (command → handler →
  API calls), adding a parameter that most users never need.
