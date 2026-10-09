## 1. Assets

- [x] 1.1 Add `assets/logo.svg`
- [x] 1.2 Generate `AfvalKalender.DesktopUI/Assets/app-logo.ico` from the SVG and remove the Avalonia placeholder icon

## 2. Android

- [x] 2.1 Add the `MauiIcon` item to `AfvalKalender.AndroidUI.csproj`
- [ ] 2.2 Verify with the `release.yml` dry run and on the emulator (launcher icon visible, app starts)

## 3. Desktop

- [x] 3.1 Point `Window Icon` in `MainWindow.axaml` (and the csproj, if needed) at `app-logo.ico`
- [x] 3.2 Use the icon for the .deb start-menu entry in the packaging script

## 4. Docs

- [x] 4.1 Show the logo in `README.md`; add a CHANGELOG entry under Unreleased
- [ ] 4.2 `bash scripts/check-docs.sh` and `openspec validate --all --strict`; archive the change
