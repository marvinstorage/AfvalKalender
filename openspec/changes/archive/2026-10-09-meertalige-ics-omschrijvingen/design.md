## Context

`TwenteMilieuApi.MapOmschrijving` turns the provider type into a Dutch sentence that is stored in SQLite as `Omschrijving` and used as ICS summary. `Update()` detects provider changes by comparing `Omschrijving`; the outbox events carry it. Existing ADRs in force: ADR-001 (light CQRS), ADR-002 (sync through the port), ADR-004 (outbox), ADR-005 (cache), ADR-007 (TUI). None covers localisation.

## Goals / Non-Goals

**Goals:**
- Choose the language of ICS summary and alarm text per run, for the ICS file and the WebDAV sync.
- Keep stored data, UIDs and change detection untouched.
- Make a third language a one-place change.

**Non-Goals:**
- Translating UI labels, validation messages or documentation.
- Storing the language preference between runs.
- Translating provider specific free text (only the six `AfvalType` values).

## Decisions

1. **Translate at output time, not at storage time.** The database keeps the Dutch `Omschrijving`. Rejected: storing the translated text, because switching language would then look like a provider change (`AfvalOphaalMomentGewijzigd`, new outbox rows) and break idempotent upserts. No schema change, so existing `afvalkalender.db` files stay valid under `EnsureCreated`.
2. **`Taal` value object and a static lookup in the Domain** (`Taal`, `AfvalTypeVertaling`). Pure C# without dependencies, in line with the zero-dependency Domain. The Dutch entries replace the switch in `TwenteMilieuApi` so there is one source of Dutch text. Rejected: `IStringLocalizer`/resx (adds Microsoft.Extensions.Localization, culture plumbing and resource files for six strings; against the library policy order custom, BCL, open source), and an `ITranslator` port (no external system involved, so a port is ceremony).
3. **Nederlands exports the stored `Omschrijving`; other languages translate by `AfvalType`.** Keeps today's output byte for byte and keeps provider specific text for Dutch when other providers (issue #5) deliver richer descriptions.
4. **Language travels as an explicit parameter.** `VerwerkKalenderCommand.Taal` (default `Nederlands`, appended last so positional callers still compile), then `IIcsExporter.ExporteerAsync(..., Taal)` and `IAfvalKalenderSynchronisator.SynchroniseerAsync(..., Taal)`. Rejected: an ambient culture (hidden state, hard to test) and putting `Taal` inside `SyncConfiguratie` (it is not a sync setting).
5. **UI default from the system language.** One Domain helper `Taal.VanCultuur(CultureInfo)` maps the two-letter UI language `en` to `Engels` and everything else to `Nederlands`, so all three UIs agree.
6. **UID unchanged** (`type_date_postcode`), so a language switch updates events in the calendar instead of duplicating them.

Dependency flow stays Presentation -> Application -> Domain <- Infrastructure; `Taal` and the lookup live in the Domain.

## Risks / Trade-offs

- [Dutch text in two places] -> `TwenteMilieuApi` uses the Domain lookup for Nederlands; a unit test asserts the old strings.
- [Existing calendar entries keep the old language until re-import] -> the UID is stable, so re-import replaces them; documented in the README.
- [Interface change breaks mocks in tests] -> update the Moq setups in `AfvalServiceTests` and `WebDavSyncAdapterTests`.
- [More languages need translators] -> the lookup is a single table; English only for now.

## Open Questions

- Which language should come next (German is common among expats)? Not needed for this change.
