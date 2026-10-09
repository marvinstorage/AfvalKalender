## ADDED Requirements

### Requirement: Optional PDF export
`VerwerkKalenderCommand` SHALL carry an optional `PdfOutputPad` (default null). When it is set, the handler MUST call `IPdfExporter` with the persisted moments, the command year and the command Taal after the ICS export; when it is null or empty, the exporter MUST NOT be called.

#### Scenario: PDF requested
- **WHEN** a command with PdfOutputPad "out.pdf" and Taal Engels is handled
- **THEN** the PDF exporter is called with that path, the year and Engels

#### Scenario: No PDF requested
- **WHEN** a command without PdfOutputPad is handled
- **THEN** the PDF exporter is not called and the ICS export is unchanged
