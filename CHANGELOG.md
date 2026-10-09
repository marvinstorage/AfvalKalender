# Changelog

Alle noemenswaardige wijzigingen in AfvalKalender. Het formaat volgt [Keep a Changelog](https://keepachangelog.com/nl/1.1.0/) en versies volgen [Semantic Versioning](https://semver.org/lang/nl/). Elke release krijgt de tag `vX.Y.Z`; de sectie hieronder wordt gepubliceerd als release-notities op GitHub.

## [Unreleased]

### Added
- **Afdrukbare PDF (issue #10):** Console en Desktop kunnen naast het `.ics` bestand een A4-jaaroverzicht maken (12 maanden, gekleurde ophaaldagen, legenda, in de gekozen taal). De PDF wordt zonder extra bibliotheek geschreven door `PdfExporter` ([ADR-012](docs/adr/ADR-012-pdf-export-zonder-bibliotheek.md)).

## [1.2.0] - 2026-10-09

Eerste release sinds 1.1.0. Bevat ook de wijzigingen die eerder, ongetagd, onder 1.3.0 en 1.2.0 waren samengesteld.

### Added
- **Taalkeuze voor agenda-items (issue #13):** Console, Desktop en Android laten kiezen tussen Nederlands en Engels. Samenvatting en herinnering in het `.ics` bestand en de WebDAV-sync volgen die taal; de standaard volgt de systeemtaal. De opgeslagen omschrijving en de UID blijven gelijk, dus opnieuw importeren werkt bestaande afspraken bij ([ADR-011](docs/adr/ADR-011-meertalige-ics-uitvoer.md)).
- **Release-workflow:** een tag `vX.Y.Z` bouwt automatisch een ondertekende Android-APK, twee Ubuntu-pakketten en twee Windows-zips en publiceert een GitHub Release met de notities uit deze changelog ([Releases](docs/dev/releases.md)).
- **Android-APK voor Obtainium:** `AfvalKalender-vX.Y.Z-android.apk` is steeds met dezelfde vaste sleutel ondertekend, zodat updates over de vorige versie installeren. Voeg de repository-URL toe in Obtainium voor automatische updates ([ADR-008](docs/adr/ADR-008-fixed-android-signing-key.md)).
- **Ubuntu-pakketten:** `afvalkalender-console_X.Y.Z_amd64.deb` (commando `afvalkalender-console`) en `afvalkalender-desktop_X.Y.Z_amd64.deb` (commando `afvalkalender-desktop` met startmenu-item), self-contained in `/opt` ([ADR-009](docs/adr/ADR-009-ubuntu-deb-packaging.md)).
- **Windows-downloads:** `AfvalKalender-console-vX.Y.Z-win-x64.zip` en `AfvalKalender-desktop-vX.Y.Z-win-x64.zip`, self-contained, zonder .NET installatie.
- **CI:** GitHub Actions voor build en tests met coverage (`ci.yml`), CodeQL (`codeql.yml`), OpenSpec-validatie (`specs.yml`) en Dependabot voor NuGet en Actions ([Continuous integration](docs/dev/ci.md)).
- **Specificaties met OpenSpec:** het huidige gedrag is vastgelegd in `openspec/specs/` en wijzigingen lopen via `/opsx-propose`, `/opsx-apply`, `/opsx-sync` en `/opsx-archive` ([ADR-010](docs/adr/ADR-010-openspec-spec-driven-workflow.md)).
- **Ontwikkelaarsdocs** in `docs/dev/`, `CONTRIBUTING.md`, een pull-request-template en issue-templates.
- **Docs-check:** `scripts/check-docs.sh` controleert links en bronpaden in de documentatie.
- **WebDAV/CalDAV sync** via `IAfvalKalenderSynchronisator` en `WebDavSyncAdapter`, beschikbaar in alle UI's ([ADR-002](docs/adr/ADR-002-webdav-caldav-sync.md)).
- **Synchronisatie-architectuur** met `SyncProvider`, `SyncConfiguratie` en de domeinservice `KalenderSynchronisatieService`. Adapters voor Google Calendar en Microsoft Graph bestaan als stubs ([ADR-006](docs/adr/ADR-006-oauth2-calendar-apis.md)).
- **Rijke terminal-interface (TUI)** met Spectre.Console: keuzelijst voor de afvalverwerker, invoervalidatie, voortgangsindicator en een tabel met de eerstvolgende ophaalmomenten ([ADR-007](docs/adr/ADR-007-Rich-TUI-Spectre-Console.md)).
- Privacyregels voor contributors in `.agents/AGENTS.md`.
- **Meerdere afvalverwerkers:** keuze uit 16 verwerkers via `AfvalVerwerkers.Alle` en een `CompanyCode` per aanroep.
- **Android-app** met .NET MAUI.
- **API cache** van 24 uur tegen rate-limiting, met `ForceerVernieuwen` om de cache te omzeilen ([ADR-005](docs/adr/ADR-005-api-cache-decorator.md)).
- **Light CQRS** met `ICommandHandler` en `VerwerkKalenderCommand` ([ADR-001](docs/adr/ADR-001-light-cqrs-icommandhandler.md)).
- **Commandovalidatie** via een decorator ([ADR-003](docs/adr/ADR-003-command-validation-decorator.md)).
- **Domein-events en transactionele outbox** ([ADR-004](docs/adr/ADR-004-domain-events-outbox.md)).
- Architectuurdiagrammen (Mermaid) in de README.

### Changed
- **Datamap per gebruiker:** de Console- en Desktop-app bewaren `afvalkalender.db` en `apicache/` nu in `LocalApplicationData/AfvalKalender` in plaats van naast het programma. Nodig omdat het `.deb` pakket alleen-lezen in `/opt` installeert.
- `README.md` is herschreven en alle verouderde bestandsnamen en versies zijn verwijderd.
- Gevoelige gegevens uit de repository verwijderd en de documentatie bijgewerkt.

### Fixed
- **Release-workflow:** de omgevingsvariabele `VERSION` werd door MSBuild als `$(Version)` gelezen en liet het herstellen van NuGet-pakketten mislukken; hernoemd naar `RELEASE_TAG`.
- 403-fouten opgelost door over te stappen op het eindpunt `wasteapi.ximmio.com`.
- Android: zichtbaarheid van de app, netwerkverbindingen en toegang tot de cachemap.

### Upgrade note
- Een bestaande `afvalkalender.db` naast het programma wordt niet meer gebruikt. De app maakt in de nieuwe map een lege database aan; de gegevens worden bij het eerstvolgende ophalen opnieuw opgebouwd.
- Een Android-installatie van een eerdere, anders ondertekende APK moet eenmalig worden verwijderd voordat de eerste release met de vaste sleutel kan worden geïnstalleerd.

## [1.1.0] - 2026-06-05

### Added
- **Desktop-app** met Avalonia (Windows en Linux).
- Klikbare links naar het gegenereerde `.ics` bestand en uitgebreide testprojecten.
- `.deb` pakket voor Ubuntu en versie- en auteursgegevens in de projectbestanden.

### Changed
- Upgrade naar .NET 10 (LTS).

## [1.0.0] - 2026-06-05

### Added
- Eerste versie: console-app met hexagonale architectuur die afvalophaalmomenten ophaalt, opslaat in SQLite en exporteert naar een `.ics` bestand met herinneringen.
