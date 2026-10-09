## 1. Domain

- [ ] 1.1 Add `Taal` (Nederlands, Engels) with `VanCultuur(CultureInfo)` in `AfvalKalender.Domain/ValueObjects`
- [ ] 1.2 Add `AfvalTypeVertaling` with descriptions per AfvalType and Taal and the reminder prefix
- [ ] 1.3 Add a `Taal` parameter to `IIcsExporter` and `IAfvalKalenderSynchronisator`; pass it through `KalenderSynchronisatieService`
- [ ] 1.4 Unit tests (UnitTests): every type/language combination, Dutch strings unchanged, `VanCultuur` for en-GB and de-DE

## 2. Application

- [ ] 2.1 Add `Taal Taal = Taal.Nederlands` as last parameter of `VerwerkKalenderCommand`
- [ ] 2.2 Pass the language from `VerwerkKalenderCommandHandler` to exporter and synchronisation service
- [ ] 2.3 Update the `AfvalServiceTests` mocks and add handler tests (Engels is passed on, stored description unchanged)

## 3. Infrastructure

- [ ] 3.1 `IcsExporter`: summary and alarm text per language, UID unchanged
- [ ] 3.2 `TwenteMilieuApi.MapOmschrijving` uses `AfvalTypeVertaling` for Nederlands
- [ ] 3.3 `WebDavSyncAdapter` and the Google/Microsoft stubs: accept and forward `Taal`
- [ ] 3.4 Infrastructure.Tests: English export summary and alarm, same UID in both languages, WebDAV passes Engels to the exporter

## 4. Presentation

- [ ] 4.1 ConsoleUI: language prompt with system default, put it in the command
- [ ] 4.2 DesktopUI: language list and selection in `MainWindowViewModel` and the axaml, put it in the command
- [ ] 4.3 AndroidUI: same in `MainPageViewModel` and `MainPage.xaml`
- [ ] 4.4 DesktopUI.Tests and AndroidUI.Tests: initial language, command contains the language

## 5. Docs and specs

- [ ] 5.1 Update README.md (language option, re-import note) and CHANGELOG.md (Unreleased)
- [ ] 5.2 Update CLAUDE.md (command fields table, domain value objects, ADR table with ADR-011) and docs/dev as needed
- [ ] 5.3 Run `openspec validate --all --strict` and `bash scripts/check-docs.sh`, then archive the change
- [ ] 5.4 No `.db`, `apicache/` or real postcodes committed; commits without AI attribution trailer
