# Getting started

## Prerequisites

- The **.NET 10 SDK**.
- For the Android app only: the Android SDK and **JDK 21**, and the workloads `dotnet workload install android maui-android`.
- Optional: Node (`npx`) to validate OpenSpec specs.

## Get the code and build

```bash
git clone https://github.com/marvinstorage/AfvalKalender.git
cd AfvalKalender
dotnet build AfvalKalender.ConsoleUI
dotnet build AfvalKalender.DesktopUI
```

Do not run `dotnet build` or `dotnet test` on the whole solution unless the Android SDK is installed; the Android projects fail without it.

## Run

```bash
dotnet run --project AfvalKalender.ConsoleUI     # terminal app (Spectre.Console)
dotnet run --project AfvalKalender.DesktopUI     # Avalonia desktop app
dotnet build AfvalKalender.AndroidUI -t:Run -f net10.0-android   # needs a device or emulator
```

The first start creates the database. Use a made-up postcode such as `1234AB` when you only want to try the UI. A real fetch needs an address that your chosen provider serves, and real addresses must never be committed ([`.agents/AGENTS.md`](../../.agents/AGENTS.md)).

Console and Desktop keep `afvalkalender.db` and `apicache/` in `LocalApplicationData/AfvalKalender` (see [Database](database.md)). Delete that folder to start fresh. The console writes the `.ics` file to the current directory.

## Test

```bash
dotnet test AfvalKalender.UnitTests/AfvalKalender.UnitTests.csproj
dotnet test AfvalKalender.Infrastructure.Tests/AfvalKalender.Infrastructure.Tests.csproj
dotnet test AfvalKalender.DesktopUI.Tests/AfvalKalender.DesktopUI.Tests.csproj
```

More in [Testing](testing.md).

## Build the Android APK locally

```bash
dotnet publish AfvalKalender.AndroidUI -c Release -f net10.0-android \
  -p:RuntimeIdentifiers="android-arm64;android-x64" -p:AndroidPackageFormat=apk \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore=AfvalKalender.AndroidUI/signing/afvalkalender.keystore \
  -p:AndroidSigningKeyAlias=afvalkalender \
  -p:AndroidSigningKeyPass=pass:android -p:AndroidSigningStorePass=pass:android
```

The result is a signed `*-Signed.apk` under `AfvalKalender.AndroidUI/bin`. The key is the committed fixed key ([ADR-008](../adr/ADR-008-fixed-android-signing-key.md)); the CI recipe is in `.github/workflows/release.yml`.

## Check the docs

```bash
bash scripts/check-docs.sh
```

## Where to go next

Read [Architecture](architecture.md) and then [Life of a command](life-of-a-command.md).
