## Context

The logo is drawn as plain SVG (no text-dependent shapes except the word "AFVAL", set in a generic sans-serif). .NET MAUI renders `MauiIcon` SVGs into all Android icon densities and an adaptive icon, so Android needs no raster files.

## Decisions

- **Single source `assets/logo.svg`**, referenced by the Android project as `<MauiIcon Include="..\assets\logo.svg" Link="Resources\AppIcon\logo.svg" Color="#2E7D32" ForegroundScale="0.65" />`. The green `Color` is the adaptive-icon background; the foreground is the SVG scaled to fit the safe zone.
- **Desktop uses a generated `.ico`** (16, 32, 48, 256 px) because Avalonia `Window.Icon` and the Windows taskbar need a raster/ICO format. The `.ico` is committed next to the SVG source; a small script in `scripts/` documents how it is generated so it can be refreshed.
- **No new library**: the ICO is written by a short script that rasterises the SVG with tools already available in CI-free local use; the committed `.ico` is what the build consumes.
- **Console unchanged**: terminals have no application icon.
- No new ADR: this is presentation assets, not an architectural decision.

## Risks

- The Android build is not exercised by normal CI. Verify with the release dry run and the emulator before merging.
- The user's own pasted logo was not available as a file; this logo is a drawn replacement and can be swapped by replacing `assets/logo.svg` and the `.ico`.
