# ui-desktop Specification

## Purpose
Defines the Avalonia MVVM desktop UI (`MainWindowViewModel`, CommunityToolkit.Mvvm) for Windows and Linux.

## Requirements

### Requirement: Initial state
The view model SHALL expose all AfvalVerwerkers, preselect the first, and start with empty postcode, huisnummer and WebDAV fields, the current year, HerinneringUur 13, StatusBericht "Klaar voor gebruik", IsBezig false and HeeftResultaat false.

#### Scenario: Created
- **WHEN** the view model is created
- **THEN** it has these defaults

### Requirement: Required input
`VerwerkAsync` SHALL set StatusBericht to a Dutch error and not call the handler when Postcode or Huisnummer is blank.

#### Scenario: Empty postcode
- **WHEN** Postcode is empty and the command runs
- **THEN** StatusBericht starts with "Fout" and the handler is not called

### Requirement: Processing
`VerwerkAsync` SHALL set IsBezig while running, normalise the postcode (spaces removed, upper-case), build the command with the selected CompanyCode, ForceerVernieuwen false, and sync provider WebDav only when the URL is non-blank, and write `AfvalKalender_<postcode>_<huisnummer>_<jaar>.ics` to the working directory.

#### Scenario: Success
- **WHEN** the handler returns moments
- **THEN** HeeftResultaat is true, OutputBestandPad is the absolute path and StatusBericht reports the count
- **AND** IsBezig is false again

#### Scenario: Handler failure
- **WHEN** the handler throws
- **THEN** StatusBericht shows "Fout: <message>" and IsBezig is false

### Requirement: Open file
`OpenBestand` SHALL open the ICS file with the shell default application and report failure in StatusBericht; it does nothing without a path.

#### Scenario: Open failure
- **WHEN** the process cannot be started
- **THEN** StatusBericht says the file could not be opened

### Requirement: Composition
`App` SHALL build the service provider, call `EnsureCreated()` on `afvalkalender.db` in the working directory, and use `apicache` in the working directory.

#### Scenario: Start
- **WHEN** the window opens
- **THEN** its DataContext is a MainWindowViewModel
