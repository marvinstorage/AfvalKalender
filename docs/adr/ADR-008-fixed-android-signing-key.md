# ADR-008 — Fixed Android Signing Key and Obtainium Distribution

**Status:** Accepted  
**Date:** 2026-10-09  
**Deciders:** Marvin Storage

## Context

The Android app is distributed as an APK from GitHub Releases and sideloaded (no Play Store). Android only installs an update over an existing app when the signature matches. Without a signing configuration, every CI runner and every developer machine would sign with its own throwaway keystore, so each new APK would fail with a signature mismatch and the user would have to uninstall first, losing the local database.

Users also want automatic updates. [Obtainium](https://obtainium.imranr.dev/) can track a GitHub repository and install new release APKs, provided the signature stays the same and the version code increases.

## Decision

1. Commit a dedicated keystore, `AfvalKalender.AndroidUI/signing/afvalkalender.keystore` (alias `afvalkalender`, store and key password `android`). `.github/workflows/release.yml` passes it to `dotnet publish` through `AndroidKeyStore`, `AndroidSigningKeyStore` and the related properties. Local and CI builds of a release therefore produce the same signature.
2. The release workflow builds one universal APK (arm64 and x86_64, `RuntimeIdentifiers` `android-arm64;android-x64`) named `AfvalKalender-vX.Y.Z-android.apk` on every tag `vX.Y.Z` and attaches it to the GitHub Release.
3. The workflow derives `ApplicationDisplayVersion` (`X.Y.Z`) and `ApplicationVersion` (`major*10000 + minor*100 + patch`, so 1.2.3 becomes 10203) from the tag, so the version code always increases with the version.
4. Users add `https://github.com/marvinstorage/AfvalKalender` in Obtainium to get updates.

## Consequences

### Positive
- Sideloaded and Obtainium updates install in place and keep the user's data.
- No CI secrets to manage.

### Negative / Trade-offs
- The key is public, so anyone could sign an APK that Android accepts as an update to an installed copy. This is acceptable for a personal, non-store build. If the app is distributed more widely, move to a private keystore held in CI secrets and Play App Signing.
- **One-time reinstall:** a copy installed from an APK with another signature must be uninstalled once before the first build signed with this key can be installed.
- The universal APK is larger than a single-ABI build, but Obtainium needs exactly one `.apk` per release. 32-bit devices are not supported.

See [releases](../dev/releases.md) for the release steps.
