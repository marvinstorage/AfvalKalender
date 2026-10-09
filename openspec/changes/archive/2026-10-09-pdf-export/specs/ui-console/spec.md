## ADDED Requirements

### Requirement: PDF option
The console UI SHALL ask whether a printable PDF should also be created (default no) and, when yes, put the PDF path `AfvalKalender_<postcode>_<huisnummer>_<jaar>.pdf` in the command and show its full path after success.

#### Scenario: PDF chosen
- **WHEN** the user answers yes
- **THEN** the command has a PdfOutputPad and the result shows the PDF path
