## 1. Domain

- [ ] 1.1 Add `IPdfExporter.ExporteerAsync(momenten, bestandspad, jaar, taal)` in `AfvalKalender.Domain/Interfaces`
- [ ] 1.2 Add `AfvalTypeVertaling.Naam(type, taal)` (Restafval/General waste, GFT/Organic waste, Papier/Paper, Verpakkingen/Packaging, Kerstboom/Christmas tree, Onbekend/Unknown)
- [ ] 1.3 Unit tests for `Naam` per type and language

## 2. Application

- [ ] 2.1 Add `string? PdfOutputPad = null` as last parameter of `VerwerkKalenderCommand`
- [ ] 2.2 Inject `IPdfExporter` into `VerwerkKalenderCommandHandler`; call it after the ICS export only when the path is set
- [ ] 2.3 Update the handler construction in `AfvalServiceTests`; add tests for "PDF requested" and "no PDF requested"

## 3. Infrastructure

- [ ] 3.1 `PdfExporter`: minimal PDF writer (catalog, pages, page, Helvetica and Helvetica-Bold, content stream, xref, trailer)
- [ ] 3.2 Layout: title, 3 x 4 month blocks, weekday header Monday first, coloured collection cells, legend
- [ ] 3.3 Month names, weekday initials and labels per Taal; escape `(`, `)` and `\`
- [ ] 3.4 Infrastructure.Tests: valid header, trailer and MediaBox, Dutch and English legend, one entry per occurring type, empty list, escaping

## 4. Presentation

- [ ] 4.1 Register `IPdfExporter` in Console, Desktop and Android DI
- [ ] 4.2 ConsoleUI: confirm prompt, PDF path in the command, show the path
- [ ] 4.3 DesktopUI: option in `MainWindowViewModel` and the axaml, PDF path property and open command
- [ ] 4.4 DesktopUI.Tests: command contains the PDF path when the option is on and none when off

## 5. Docs and specs

- [ ] 5.1 Create `docs/adr/ADR-012-pdf-export-zonder-bibliotheek.md` and add it to the ADR table in CLAUDE.md
- [ ] 5.2 Update README.md, CHANGELOG.md (Unreleased), CLAUDE.md (port list, command fields, adapters, project structure) and docs/dev as needed
- [ ] 5.3 Run `openspec validate --all --strict` and `bash scripts/check-docs.sh`, then archive the change
- [ ] 5.4 No `.db`, `apicache/`, generated PDFs or real postcodes committed; commits without AI attribution trailer
