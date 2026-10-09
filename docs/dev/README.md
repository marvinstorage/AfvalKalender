# Developer documentation

Welcome. These pages explain how AfvalKalender is built and how to contribute. They are written in English so contributors from any country can join. The app, the domain model and the README are Dutch.

AfvalKalender fetches Dutch household waste-collection schedules from the Ximmio API, stores them in SQLite and exports an `.ics` calendar or syncs it to a CalDAV server. It ships as a terminal app, a desktop app and an Android app.

## Reading order

| # | Page | Read it to |
|---|------|------------|
| 1 | [Getting started](getting-started.md) | build, test and run the apps |
| 2 | [Architecture](architecture.md) | understand the layers and the dependency rule |
| 3 | [Life of a command](life-of-a-command.md) | follow one `VerwerkKalenderCommand` through every layer (the best way to learn the code) |
| 4 | [Conventions](conventions.md) | know the rules the code follows |
| 5 | [Testing](testing.md) | see what is tested where and how to run it |
| 6 | [Database](database.md) | see the tables and how they relate |
| 7 | [Releases](releases.md) | understand versions, tags and the release build |
| 8 | [Continuous integration](ci.md) | see which GitHub Actions run and how to run the same checks locally |

## Where the other knowledge lives

- **[Architecture Decision Records](../adr/ADR-001-light-cqrs-icommandhandler.md)**: why a decision was made. All ADRs are in `docs/adr/`.
- **[OpenSpec specs](../../openspec/specs/README.md)**: what the system must do, as requirements with scenarios. Proposed work lives in `openspec/changes/`.
- **[CLAUDE.md](../../CLAUDE.md)**: the compact technical overview for contributors and AI assistants.

## Contributing

Start with [CONTRIBUTING.md](../../CONTRIBUTING.md): propose with OpenSpec, implement, test, update the docs, add a changelog entry.
