## Context

The handler exports an ICS file and optionally syncs. Issue #10 suggests QuestPDF. The project library policy prefers a custom implementation, then BCL, then MIT/Apache libraries. The Domain stays dependency free (spec architecture-governance).

## Goals / Non-Goals

**Goals:**
- A printable A4 year overview: 12 month blocks, coloured collection days, legend, title with postcode, house number and year.
- Follow the chosen `Taal` for the title, month names, weekday initials and the waste type names.
- Work the same on Windows, Linux and Android without font installation.

**Non-Goals:**
- A PDF on Android (needs a share flow and cannot be verified in CI).
- Multi-year or per-month pages, custom themes, embedded fonts or non-Latin text.
- Storing the PDF path between runs.

## Decisions

1. **Custom writer, no library.** A PDF page with text, filled rectangles and lines needs only a handful of PDF operators. The adapter builds the content stream, the object table and the xref itself (about 150 lines) and uses the built-in Helvetica and Helvetica-Bold fonts (WinAnsi), so there are no font files and no licence questions. Rejected: QuestPDF (community licence is not MIT/Apache and needs a revenue declaration, native dependencies per platform), PdfSharp/MigraDoc (MIT, but on Linux and Android it needs a font resolver and bundled fonts for the same result).
2. **New port `IPdfExporter`** with `ExporteerAsync(momenten, bestandspad, jaar, taal)`, mirroring `IIcsExporter`. `jaar` is explicit so an empty list still yields a titled calendar. The title uses the postcode and house number of the moments when present.
3. **Layout:** A4 portrait (595 x 842 pt), 3 columns x 4 rows of month blocks. Each block has the month name, a weekday header (Monday first) and day numbers; a collection day gets a filled cell in the colour of its type. Two types on one day split the cell. A legend at the bottom maps colour to waste type name in the chosen language.
4. **Colours and names live in one place each.** Colours per `AfvalType` are a static map in the Infrastructure adapter (a presentation concern); names come from the new Domain method `AfvalTypeVertaling.Naam(type, taal)` ("Restafval" / "General waste", ...), next to the existing descriptions.
5. **Command carries an optional path.** `PdfOutputPad` (string?, appended last) keeps positional callers compiling. The handler exports the PDF after the ICS export from the same persisted moments, so the PDF never differs from the ICS.
6. **Text encoding.** Month names, weekday letters and type names are Latin-1; text is written as a WinAnsi literal string with `(`, `)` and `\` escaped. A unit test covers the escaping.

## Risks / Trade-offs

- [Hand-written PDF may render wrongly in some viewers] -> keep to the minimal, standard object set (catalog, pages, page, two fonts, content stream), a correct xref and trailer, test the structure and open the file in a viewer during review.
- [Limited typography] -> acceptable for a fridge calendar; revisit with a superseding ADR if richer layout is requested.
- [Handler gets another dependency] -> follows the existing pattern; one more registration per UI.

## Open Questions

- Should Android share the PDF as well? Left for a follow-up once the desktop and console flow is accepted.
