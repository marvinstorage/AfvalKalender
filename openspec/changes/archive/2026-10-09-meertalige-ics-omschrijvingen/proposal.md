## Why

Issue #13: many expats in the Netherlands use these waste services but the calendar events are Dutch only ("Restafval wordt opgehaald"). The user wants to choose the language of the events that end up in their calendar.

## What Changes

- A new domain value object `Taal` (`Nederlands`, `Engels`) and a translation of every `AfvalType` into an event description per `Taal`.
- `VerwerkKalenderCommand` gets an optional `Taal` (default `Nederlands`, so existing callers keep today's behaviour).
- The ICS export and the WebDAV sync use the chosen `Taal` for the event summary and the alarm text. The stored `Omschrijving` in SQLite stays as delivered by the provider (Dutch), so change detection and the outbox events do not change.
- Console, Desktop and Android get a language choice. The default follows the system UI language (English for `en`, otherwise Dutch).
- Translating the UI labels themselves is **not** part of this change.
- **BREAKING (internal only)**: `IIcsExporter.ExporteerAsync` and `IAfvalKalenderSynchronisator.SynchroniseerAsync` get an extra `Taal` parameter.

## Capabilities

### New Capabilities
- `meertaligheid`: the supported languages, the translation of each AfvalType and how the language is chosen.

### Modified Capabilities
- `ics-export`: event summary and alarm text follow the chosen language.
- `afval-ophaalmomenten`: the command carries the language; stored descriptions stay Dutch.
- `calendar-sync`: WebDAV sync passes the language to the exported file.
- `ui-console`: language prompt.
- `ui-desktop`: language selector.
- `platform-android`: language selector.

## Impact

- Layers: Domain (`Taal`, translation), Application (command, handler), Infrastructure (`IcsExporter`, `WebDavSyncAdapter`), Presentation (all three UIs).
- No SQLite schema change and no migration: the database keeps the Dutch `Omschrijving`, so existing `afvalkalender.db` files stay usable.
- No new NuGet package. `IStringLocalizer` (Microsoft.Extensions.Localization) is not used: the Domain must stay dependency free and a fixed lookup of six types in two languages does not justify a resource pipeline (see design).
- Existing calendar events keep the same UID (`type_date_postcode`), so re-importing after a language change updates the text instead of duplicating events.
