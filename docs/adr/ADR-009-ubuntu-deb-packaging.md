# ADR-009 — Ubuntu .deb Packaging and Per-User Data Directory

**Status:** Accepted  
**Date:** 2026-10-09  
**Deciders:** Marvin Storage

## Context

Ubuntu/Debian users want to install the terminal and desktop apps with the package manager. An older hand-made package (`afvalkalender-exporteur`) existed outside any pipeline. The console and desktop apps used to write `afvalkalender.db` and `apicache/` next to the executable, which fails when the program is installed read-only under `/opt`.

## Decision

1. Build **two packages** per release with `scripts/build-deb.sh`, called from `.github/workflows/release.yml`:
   - `afvalkalender-console_X.Y.Z_amd64.deb` installs `/opt/afvalkalender-console` and the command `/usr/bin/afvalkalender-console` (the Spectre.Console TUI).
   - `afvalkalender-desktop_X.Y.Z_amd64.deb` installs `/opt/afvalkalender-desktop`, the command `/usr/bin/afvalkalender-desktop` and a `.desktop` launcher in `/usr/share/applications`.
2. Each package contains a **self-contained** `linux-x64` publish, so no .NET runtime is needed. `Depends` lists only the native libraries the runtime and Avalonia need.
3. The workflow installs each package on the runner as a smoke test before uploading it.
4. Console and Desktop store `afvalkalender.db` and `apicache/` in `Environment.SpecialFolder.LocalApplicationData/AfvalKalender` (for example `~/.local/share/AfvalKalender`). Android keeps using its own app data and cache directories.
5. The same release also publishes self-contained Windows x64 zips for both apps.

## Consequences

### Positive
- `sudo apt install ./package.deb` works without .NET and without write access to `/opt`.
- Installing or removing the package never touches user data.

### Negative / Trade-offs
- Packages are large (the runtime is bundled).
- amd64 only; no apt repository, so updates are manual downloads.
- A database next to the executable from older versions is no longer read. Data is rebuilt on the next fetch (see the changelog upgrade note).
