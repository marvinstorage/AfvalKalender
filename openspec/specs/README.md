# AfvalKalender Specifications

These specs describe the behaviour implemented today. Platform decisions live in `docs/adr/`.

## Index

| Spec | Scope |
| --- | --- |
| [afval-ophaalmomenten](afval-ophaalmomenten/spec.md) | AfvalOphaalMoment, Adres, AfvalType, Update change detection, domain events |
| [afvalverwerkers](afvalverwerkers/spec.md) | 16 AfvalVerwerkers, CompanyCode, selection |
| [command-validation](command-validation/spec.md) | VerwerkKalenderCommand fields, validator, decorator |
| [ximmio-api](ximmio-api/spec.md) | Address id and calendar fetch, type mapping, errors |
| [api-cache](api-cache/spec.md) | 24h file cache, keys, ForceerVernieuwen |
| [persistence](persistence/spec.md) | SQLite, idempotent upserts, outbox, EnsureCreated |
| [ics-export](ics-export/spec.md) | Events, UIDs, reminder alarm, file names |
| [calendar-sync](calendar-sync/spec.md) | SyncProvider, KalenderSynchronisatieService, WebDAV, stubs |
| [ui-console](ui-console/spec.md) | Spectre.Console TUI |
| [ui-desktop](ui-desktop/spec.md) | Avalonia MVVM UI |
| [platform-android](platform-android/spec.md) | **Android-only** MAUI specifics (ApplicationId, SDK, share, cache dir) |
| [architecture-governance](architecture-governance/spec.md) | Layers, ADRs, docs and privacy rules |
| [security-transport](security-transport/spec.md) | Current TLS behaviour (known gap) and credential handling |

`platform-android` is the only platform-specific spec; all others are platform-neutral.
