# Contributing to AfvalKalender

Thank you for helping. AfvalKalender fetches Dutch waste-collection schedules from the Ximmio API and exports them as an `.ics` calendar. The developer documentation is in [`docs/dev/`](docs/dev/README.md); start there to learn the code and the architecture.

Contributions in English or Dutch are welcome. The app and the domain model are Dutch (see [conventions](docs/dev/conventions.md)); developer docs are English.

## Ground rules

- **Privacy first.** Never commit real Dutch postcodes or addresses, `.db` databases or `apicache/` files ([`.agents/AGENTS.md`](.agents/AGENTS.md)). Use made-up values such as `1234AB` in tests and issues.
- **Hexagonal architecture.** Presentation -> Application -> Domain <- Infrastructure. The Domain has no NuGet dependencies ([architecture](docs/dev/architecture.md)).
- **Dutch ubiquitous language** for domain names, methods and events.
- **Keep it small.** Prefer your own code, then the BCL, then an MIT/Apache library ([Third-Party Library Policy](CLAUDE.md#third-party-library-policy)).

## How to contribute

1. **Open an issue first** for anything bigger than a small fix.
2. **Propose with OpenSpec** for a change in behaviour: use `/opsx-propose` (see [ADR-010](docs/adr/ADR-010-openspec-spec-driven-workflow.md)). The specs live in [`openspec/specs/`](openspec/specs/README.md). Typo fixes, small bug fixes and doc changes do not need a proposal.
3. **Implement** following [Getting started](docs/dev/getting-started.md) and [Conventions](docs/dev/conventions.md).
4. **Test.** Run each test project on its own ([Testing](docs/dev/testing.md)):

   ```bash
   dotnet test AfvalKalender.UnitTests/AfvalKalender.UnitTests.csproj
   dotnet test AfvalKalender.Infrastructure.Tests/AfvalKalender.Infrastructure.Tests.csproj
   dotnet test AfvalKalender.DesktopUI.Tests/AfvalKalender.DesktopUI.Tests.csproj
   ```

   Do not run `dotnet test` on the whole solution: the Android projects need the Android SDK.
5. **Update the docs** your change touches: README, CLAUDE.md, ADRs (a new feature needs one), specs, and a line under `Unreleased` in `CHANGELOG.md` (Dutch).
6. **Open a pull request** against `main` and fill in the template.

## Pull request checklist

- [ ] The three test projects pass
- [ ] New or changed behaviour has tests named `Method_Scenario_ExpectedResult`, in Dutch
- [ ] No real postcodes, addresses, `.db` or cache files
- [ ] Domain names are Dutch; layers respect the dependency rule
- [ ] README, ADRs and OpenSpec specs are up to date
- [ ] CHANGELOG entry under `Unreleased`
- [ ] `bash scripts/check-docs.sh` passes

## Commit messages

Use a prefix (`feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`), an imperative first line, and a body that explains why. Do not add AI attribution lines (no `Co-Authored-By` for tools, no "generated with" footers).

## Reporting a security problem

Do not open a public issue for a vulnerability. Use GitHub's private vulnerability reporting on the repository's Security tab.

## Licence

By contributing you agree that your contribution is licensed under the project's licence (GPL-3.0).
