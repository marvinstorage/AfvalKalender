## 1. Domain

- [x] 1.1 Add `Taal` (Nederlands, Engels) with `VanCultuur(CultureInfo)` in `AfvalKalender.Domain/ValueObjects`
- [x] 1.2 Add `AfvalTypeVertaling` with descriptions per AfvalType and Taal and the reminder prefix
- [x] 1.3 Add a `Taal` parameter to `IIcsExporter` and `IAfvalKalenderSynchronisator`; pass it through `KalenderSynchronisatieService`
- [x] 1.4 Unit tests (UnitTests): every type/language combination, Dutch strings unchanged, `VanCultuur` for en-GB and de-DE

## 2. Application

- [x] 2.1 Add `Taal Taal = Taal.Nederlands` as last parameter of `VerwerkKalenderCommand`
- [x] 2.2 Pass the language from `VerwerkKalenderCommandHandler` to exporter and synchronisation service
- [x] 2.3 Update the `AfvalServiceTests` mocks and add handler tests (Engels is passed on, stored description unchanged)

## 3. Infrastructure

- [x] 3.1 `IcsExporter`: summary and alarm text per language, UID unchanged
- [x] 3.2 `TwenteMilieuApi.MapOmschrijving` uses `AfvalTypeVertaling` for Nederlands
- [x] 3.3 `WebDavSyncAdapter` and the Google/Microsoft stubs: accept and forward `Taal`
- [x] 3.4 Infrastructure.Tests: English export summary and alarm, same UID in both languages, WebDAV passes Engels to the exporter

## 4. Presentation

- [x] 4.1 ConsoleUI: language prompt with system default, put it in the command
- [x] 4.2 DesktopUI: language list and selection in `MainWindowViewModel` and the axaml, put it in the command
- [x] 4.3 AndroidUI: same in `MainPageViewModel` and `MainPage.xaml`
- [x] 4.4 DesktopUI.Tests and AndroidUI.Tests: initial language, command contains the language

## 5. Docs and specs

- [x] 5.1 Update README.md (language option, re-import note) and CHANGELOG.md (Unreleased)
- [x] 5.2 Update CLAUDE.md (command fields table, domain value objects, ADR table with ADR-011) and docs/dev as needed
- [x] 5.3 Run `openspec validate --all --strict` and `bash scripts/check-docs.sh`, then archive the change
- [x] 5.4 No `.db`, `apicache/` or real postcodes committed; commits without AI attribution trailer
