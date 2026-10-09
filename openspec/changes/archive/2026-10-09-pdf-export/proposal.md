## Why

Issue #10: many households still hang a printed schedule on the fridge. Besides the calendar file, users want a printable A4 overview of the whole year, colour-coded per waste type, in the language they chose (issue #13).

## What Changes

- A new outbound port `IPdfExporter` in the Domain and a dependency-free adapter `PdfExporter` in the Infrastructure that writes an A4 PDF with the year schedule: one block per month, each collection date marked with the colour of its waste type, plus a legend.
- `VerwerkKalenderCommand` gets an optional `PdfOutputPad` (default `null`). When set, the handler also exports the PDF from the stored moments, in the chosen `Taal`. When empty, behaviour is unchanged.
- Console and Desktop get an option to also create the PDF. Android keeps its current flow (out of scope, see design).
- No change to the stored data, the ICS file or the UID.

## Capabilities

### New Capabilities
- `pdf-export`: layout and content of the printable year schedule and the `IPdfExporter` contract.

### Modified Capabilities
- `afval-ophaalmomenten`: the command carries an optional PDF path; the handler exports the PDF only when it is set.
- `ui-console`: option to create the PDF.
- `ui-desktop`: option to create the PDF.

## Impact

- Layers: Domain (`IPdfExporter`), Application (command, handler), Infrastructure (`PdfExporter`), Presentation (Console, Desktop; the Android DI registration is added because the handler is shared).
- The handler constructor gets a fifth dependency; the unit tests that build it are updated.
- No SQLite schema change and no new NuGet package: the PDF is written by a small custom writer using the standard Helvetica font (see design and ADR-012).
