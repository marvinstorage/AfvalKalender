# ADR-007: Rich Terminal User Interface via Spectre.Console

**Status:** Accepted
**Date:** 2026-07-01

## Context
The `AfvalKalender.ConsoleUI` originally provided a sequential, text-based interactive prompt to gather user input (postcode, house number, provider). While functional, it lacked visual affordances, input validation, and a structured way to present the retrieved calendar data (the user was only told that an `.ics` file was generated).

## Decision
We decided to adopt `Spectre.Console` in the `AfvalKalender.ConsoleUI` project to build a Rich Terminal User Interface (TUI). 

The `ConsoleApp` was rewritten to utilize:
- `SelectionPrompt` for selecting the waste provider from a scrollable list.
- `TextPrompt` with validation for numbers (year, hours) and `.Secret()` masking for WebDAV passwords.
- `Status` spinner for asynchronous feedback during data fetching.
- `Table` for rendering a color-coded grid of the next 10 upcoming waste collections directly in the terminal.

## Consequences
**Positive:**
- Substantially improved user experience with clear visual cues and colors.
- Built-in input validation prevents crashes caused by malformed user input (e.g., parsing strings into integers).
- Users get immediate visual gratification by seeing their upcoming schedule in a table, rather than having to open the exported `.ics` file.

**Negative:**
- Adds a third-party dependency (`Spectre.Console`) to the Presentation layer. However, this is confined strictly to the `ConsoleUI` project and does not bleed into the Application or Domain layers, adhering to the Clean Architecture boundaries.
- Makes automated unit testing of the interactive UI prompts slightly more complex without injecting `IAnsiConsole`.
