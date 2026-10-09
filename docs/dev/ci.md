# Continuous integration

Every push and pull request is checked by GitHub Actions. This page lists what runs, when, and how to run the same checks on your own machine first. The workflow files are in `.github/workflows/`.

## Overview

| Workflow | File | Runs on | What it does | Blocks a merge? |
|----------|------|---------|--------------|-----------------|
| **CI** | `.github/workflows/ci.yml` | push and pull request to `main`/`master`, manual | Checks the docs, builds Console and Desktop, runs the three test projects with coverage | Yes (check name "Build & Test") |
| **Release** | `.github/workflows/release.yml` | tag `v*`, manual (dry run) | Runs the tests, builds the APK, two debs and two Windows zips, publishes a GitHub Release on a tag | No, it runs after the merge |
| **OpenSpec** | `.github/workflows/specs.yml` | push and pull request that change `openspec/**`, manual | Validates all specs and changes in strict mode | Yes, once you make it a required check |
| **CodeQL** | `.github/workflows/codeql.yml` | push and pull request to `main`/`master`, every Monday | Static security analysis of the C# code | Findings show under Security, Code scanning |
| **Dependabot** | `.github/dependabot.yml` | every Monday | Opens pull requests for newer NuGet packages and Actions | No, it only proposes changes |

"Blocks a merge" depends on branch protection, which is a GitHub setting and not part of the repository. See [Recommended GitHub settings](#recommended-github-settings).

## CI

In order:

1. **Check developer docs** runs `scripts/check-docs.sh`: relative links must resolve and source paths named in `docs/dev` must exist.
2. **Build** of `AfvalKalender.ConsoleUI` and `AfvalKalender.DesktopUI` in Release (this also builds Domain, Application and Infrastructure).
3. **Tests** for `AfvalKalender.UnitTests`, `AfvalKalender.Infrastructure.Tests` and `AfvalKalender.DesktopUI.Tests`, each with coverage ([Testing](testing.md)).
4. **Code statistics and coverage summary** runs `scripts/job-summary.sh` and writes a table to the run summary page. This step never fails the build.
5. **Upload coverage reports** attaches them as the `coverage-reports` artifact (kept 14 days).

The Android projects are not built or tested in CI. The Android build is exercised by the Release workflow.

## Release

See [Releases](releases.md). A manual run with a version number is a dry run that only uploads artifacts.

## OpenSpec

Runs `openspec validate --all --strict` with a pinned version, only when something under `openspec/` changes ([ADR-010](../adr/ADR-010-openspec-spec-driven-workflow.md)).

## CodeQL

Analyses the C# code on every push and pull request and once a week so new rules are applied to old code. It builds Console and Desktop by hand (no Android). Results are under **Security, Code scanning**. Do not also switch on the CodeQL "default setup" in the repository settings; the two conflict.

## Dependabot

Every Monday Dependabot looks for newer versions and opens grouped pull requests:

- **NuGet**, in groups: Avalonia; EF Core and `Microsoft.Extensions.*`; the test libraries. Major version bumps are ignored on purpose and done by hand.
- **GitHub Actions**, in one group.

Commit messages start with `chore(deps)` or `chore(ci)`. To handle a pull request: wait for the checks, read the release notes, merge. Dependabot never merges on its own. It cannot see the pinned OpenSpec version in `.github/workflows/specs.yml`; check that one now and then. It also cannot compile the Android project, so try the app after bumping MAUI-related packages.

## Run the same checks locally

```bash
bash scripts/check-docs.sh                         # docs links and paths
npx --no-install openspec validate --all --strict  # specs, when you touched openspec/
dotnet build AfvalKalender.ConsoleUI -c Release
dotnet build AfvalKalender.DesktopUI -c Release
dotnet test AfvalKalender.UnitTests/AfvalKalender.UnitTests.csproj
dotnet test AfvalKalender.Infrastructure.Tests/AfvalKalender.Infrastructure.Tests.csproj
dotnet test AfvalKalender.DesktopUI.Tests/AfvalKalender.DesktopUI.Tests.csproj
```

`scripts/check-docs.sh` only checks files tracked by git, so run `git add -N .` first to include new files.

## Recommended GitHub settings

Repository settings that only an admin can change, so they are not in the workflows. Suggested for `main`:

- Branch protection or a ruleset that requires the check **Build & Test** before merging. Do not require the OpenSpec check: it does not run when `openspec/` is untouched, and a required check that never runs blocks the merge.
- **Secret scanning** and **push protection**.
- Leave the CodeQL default setup off.

## Adding or changing a workflow

- Keep the triggers: pull request and push to `main`/`master`, plus `workflow_dispatch`.
- Use `ubuntu-latest` and .NET `10.0.x`.
- Give a job only the permissions it needs.
- A new check that may be noisy starts with `continue-on-error: true` until a run is clean.
- Add the workflow to the table at the top of this page.
