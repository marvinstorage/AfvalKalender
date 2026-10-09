## Context

The logo is the artwork supplied by the user (`assets/logo-source.jpg`, 1024 px). A JPEG has no transparency, so `scripts/make-logo-icons.py` (Python + Pillow) derives the files the apps need.

## Decisions

- **Source `assets/logo-source.jpg`**; generated and committed: `assets/logo.png` (512 px rounded square for the README), `assets/logo-256.png` (.deb icon), `assets/logo-foreground.png` (artwork with the light background made transparent) and `AfvalKalender.DesktopUI/Assets/app-logo.ico`.
- **Android** uses `<MauiIcon Include="..\assets\logo-foreground.png" Link="Resources\AppIcon\appicon.png" Color="#FFFFFF" ForegroundScale="0.8" />`. The white `Color` is the adaptive-icon background; MAUI generates all densities and the round icon. The manifest references `@mipmap/appicon` and `@mipmap/appicon_round`.
- **Desktop uses the generated `.ico`** (16, 32, 48, 256 px, PNG entries) because Avalonia `Window.Icon` and the Windows taskbar need ICO.
- **No new runtime library**: Pillow is only a local tool for regenerating the committed files; the build consumes the committed files.
- **Console unchanged**: terminals have no application icon.
- No new ADR: this is presentation assets, not an architectural decision.

## Risks

- The Android build is not exercised by normal CI. Verify with the release dry run and the emulator before merging.
- A raster logo is less sharp than a vector at very large sizes; 1024 px source is enough for launcher and window icons.
