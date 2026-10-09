## ADDED Requirements

### Requirement: PDF option
The desktop UI SHALL offer an "also create PDF" choice (default off). When on, the command carries the PDF path next to the ICS file and the result offers to open the PDF.

#### Scenario: PDF switched on
- **WHEN** the option is on and the user processes
- **THEN** the command has a PdfOutputPad and the view model exposes the PDF path
