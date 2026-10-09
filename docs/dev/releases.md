# Releases

Maintainers cut releases. This page explains how, so you know what happens after your change is merged.

## Versioning

Semantic Versioning, tags `vX.Y.Z`. The version is stored in three places:

| Where | Property | Used for |
|---|---|---|
| `AfvalKalender.ConsoleUI/AfvalKalender.ConsoleUI.csproj` | `<Version>` | console app version |
| `AfvalKalender.DesktopUI/AfvalKalender.DesktopUI.csproj` | `<Version>` | desktop app version |
| `AfvalKalender.AndroidUI/AfvalKalender.AndroidUI.csproj` | `ApplicationDisplayVersion` (`X.Y.Z`) and `ApplicationVersion` (integer) | Android version name and version code |

The release workflow overrides these from the tag, so a tag alone produces correctly versioned artifacts. Keep the files in step anyway so local builds report the right version. The Android version code is `major*10000 + minor*100 + patch` (1.2.3 becomes 10203). It must increase with every release, because Android and Obtainium use it to detect updates.

## Steps

1. Move the entries under `## [Unreleased]` in `CHANGELOG.md` to a new `## [X.Y.Z] - YYYY-MM-DD` section. The heading must have exactly this shape, because the workflow extracts the release notes with it. Use `Added`, `Changed`, `Fixed` and an `Upgrade note` when users need to know something.
2. Bump `<Version>` in the ConsoleUI and DesktopUI csproj files, and `ApplicationDisplayVersion` and `ApplicationVersion` in the AndroidUI csproj.
3. Run the three test projects ([Testing](testing.md)) and `bash scripts/check-docs.sh`.
4. Commit (`chore: release X.Y.Z`) and push to `main`.
5. Tag and push the tag:

   ```bash
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```

6. `.github/workflows/release.yml` runs.

## What the workflow produces

After the tests pass, four jobs run and a final job publishes the GitHub Release.

| Job | Output |
|---|---|
| Android APK | `AfvalKalender-vX.Y.Z-android.apk`, one signed universal APK (arm64 and x86_64) |
| Ubuntu package (console) | `afvalkalender-console_X.Y.Z_amd64.deb`, command `afvalkalender-console` |
| Ubuntu package (desktop) | `afvalkalender-desktop_X.Y.Z_amd64.deb`, command `afvalkalender-desktop` and a `.desktop` launcher |
| Windows zip (console, desktop) | `AfvalKalender-console-vX.Y.Z-win-x64.zip` and `AfvalKalender-desktop-vX.Y.Z-win-x64.zip`, self-contained |
| Publish GitHub Release | release notes from the matching CHANGELOG section plus an install section in Dutch, with all files attached |

A tag containing a hyphen (for example `v1.3.0-rc1`) is published as a pre-release. A section is missing from the CHANGELOG: the release gets notes generated from the commits.

**Dry run:** start the workflow by hand (Actions, Release, Run workflow) and enter a version. It builds and uploads artifacts but does not publish a release.

## Obtainium

Obtainium tracks the repository URL `https://github.com/marvinstorage/AfvalKalender` and installs the newest release APK. It works because:

- every release has exactly one `.apk` file;
- the version code increases with the version;
- every build is signed with the same key (next section).

## Signing

Every APK is signed with the committed keystore `AfvalKalender.AndroidUI/signing/afvalkalender.keystore` (alias `afvalkalender`, password `android`), so updates install over the previous version and keep the user's data ([ADR-008](../adr/ADR-008-fixed-android-signing-key.md)). The key is public on purpose; that is acceptable for a personal build. Do not replace it: a new key forces everyone to uninstall first.

## Ubuntu packages and data

The debs are built by `scripts/build-deb.sh` and install read-only under `/opt`. Data is kept per user in `~/.local/share/AfvalKalender` ([ADR-009](../adr/ADR-009-ubuntu-deb-packaging.md)).

## After the release

Check the release page. Archive the finished OpenSpec change so the specs describe the shipped behaviour (`/opsx-sync`, then `/opsx-archive`).
