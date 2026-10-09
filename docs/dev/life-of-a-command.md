# Life of a command

This page follows one `VerwerkKalenderCommand` through every layer. Use a made-up address: postcode `1234AB`, house number `10`, year 2026, reminder 12 hours ahead, no sync.

## 1. The UI builds the command

The user chooses a provider and fills in the address.

- Console: `AfvalKalender.ConsoleUI/ConsoleApp.cs` prompts with Spectre.Console, builds the output file name `AfvalKalender_<postcode>_<huisnummer>_<jaar>.ics` and runs the handler behind a status spinner.
- Desktop: `AfvalKalender.DesktopUI/ViewModels/MainWindowViewModel.cs` normalises the postcode (`ToUpper().Replace(" ", "")`) and runs the command.
- Android: `AfvalKalender.AndroidUI/ViewModels/MainPageViewModel.cs` does the same and writes the `.ics` to the cache directory so it can be shared.

The command is the immutable record `AfvalKalender.Application/Commands/VerwerkKalenderCommand.cs`: `Postcode`, `Huisnummer`, `Jaar`, `HerinneringUur`, `OutputPad`, `CompanyCode` (default Twente Milieu), `ForceerVernieuwen`, `SyncProvider`, `SyncDoelUrlOfToken`, `SyncGebruiker`, `SyncWachtwoord`. The `CompanyCode` comes from the chosen `AfvalVerwerker` (`AfvalKalender.Domain/ValueObjects/AfvalVerwerker.cs`).

## 2. The decorator validates

The UI resolves `ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>` from DI. That is `AfvalKalender.Application/Commands/ValidatingCommandHandlerDecorator.cs`, which calls `AfvalKalender.Application/Commands/VerwerkKalenderCommandValidator.cs` first. Postcode must match `^[1-9][0-9]{3}[A-Z]{2}$`, huisnummer is required, `Jaar` is 2000 to 2100, `HerinneringUur` is 0 to 23, `OutputPad` is required and `CompanyCode` must parse as a `Guid`. A violation throws `ArgumentException` with a Dutch message and nothing else happens ([ADR-003](../adr/ADR-003-command-validation-decorator.md)).

## 3. The handler orchestrates

`AfvalKalender.Application/Commands/VerwerkKalenderCommandHandler.cs`, in order:

1. `IAfvalApi.HaalUniekAdresIdOpAsync` returns the Ximmio `UniqueId` for the address.
2. `IAfvalApi.HaalKalenderOpAsync` returns the year's `AfvalOphaalMoment` list.
3. `IAfvalRepository.SlaOpOfUpdateAsync` stores them.
4. `IAfvalRepository.HaalOpVoorAdresEnJaarAsync` reads back the persisted moments.
5. `IIcsExporter.ExporteerAsync` writes the `.ics` file.
6. `KalenderSynchronisatieService.SynchroniseerAsync` syncs when a provider is set.

The handler depends only on Domain ports.

## 4. API with cache

`IAfvalApi` is `AfvalKalender.Infrastructure/Cache/CacherendeAfvalApi.cs`, a decorator over `AfvalKalender.Infrastructure/Api/TwenteMilieuApi.cs`. The cache reads `adresid_<company>_<postcode>_<huisnummer>.json` and `kalender_<company>_<postcode>_<huisnummer>_<jaar>.json`. A file younger than 24 hours is returned without a network call, unless `ForceerVernieuwen` is true ([ADR-005](../adr/ADR-005-api-cache-decorator.md)). On a miss `TwenteMilieuApi` POSTs to `https://wasteapi.ximmio.com/api/FetchAdress` and `GetCalendar`, maps the Ximmio waste type to `AfvalType` and creates `AfvalOphaalMoment` objects. Creating one raises `AfvalOphaalMomentToegevoegd`.

## 5. Persistence and outbox

`AfvalKalender.Infrastructure/Persistence/EfAfvalRepository.cs` looks up each moment by postcode, huisnummer, date and type. New ones are added. Existing ones get `Update(omschrijving)` (`AfvalKalender.Domain/Entities/AfvalOphaalMoment.cs`), which changes `LaatstGewijzigd` and raises `AfvalOphaalMomentGewijzigd` only if the text differs. Before `SaveChangesAsync` the pending domain events of all tracked entities are serialised into `AfvalKalender.Infrastructure/Persistence/OutboxMessage.cs` rows in the same transaction ([ADR-004](../adr/ADR-004-domain-events-outbox.md), [Database](database.md)).

## 6. ICS export

`AfvalKalender.Infrastructure/Ics/IcsExporter.cs` builds one event per moment with Ical.Net, from 08:00 to 09:00 on the collection day. The UID is `<Type>_<yyyyMMdd>_<Postcode>`, so re-importing does not duplicate events, and a relative display alarm fires `HerinneringUur` hours before.

## 7. Optional sync

`AfvalKalender.Domain/Services/KalenderSynchronisatieService.cs` returns immediately for `SyncProvider.Geen`. Otherwise it finds the `IAfvalKalenderSynchronisator` whose `Ondersteunt` matches. `AfvalKalender.Infrastructure/Sync/WebDavSyncAdapter.cs` writes a temporary ICS file and HTTP PUTs it with optional Basic Auth ([ADR-002](../adr/ADR-002-webdav-caldav-sync.md)). An unsupported provider throws `NotSupportedException`.

## 8. Back to the UI

The handler returns the persisted moments. The console shows a table of the next collections and the file name. Desktop and Android show a status message (Android also offers sharing).

## Where to look when something breaks

| Symptom | Look at |
|---|---|
| "Postcode is ongeldig" | `VerwerkKalenderCommandValidator`, postcode normalisation in the UI |
| "Kon geen uniek adres ID vinden" | the provider does not serve the address; `TwenteMilieuApi` |
| Old data keeps showing | the 24 hour cache; use `ForceerVernieuwen` or delete `apicache/` |
| Duplicate calendar items | the ICS UID in `IcsExporter` |
