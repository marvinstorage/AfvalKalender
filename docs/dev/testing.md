# Testing

Frameworks: xUnit, FluentAssertions, Moq, EF Core InMemory and Avalonia.Headless.XUnit.

## What is tested where

| Project | Covers | Examples |
|---|---|---|
| `AfvalKalender.UnitTests` | Domain, Application and the console app | `AfvalKalender.UnitTests/Domain/AdresTests.cs`, `AfvalKalender.UnitTests/Domain/AfvalOphaalMomentTests.cs`, `AfvalKalender.UnitTests/Application/VerwerkKalenderCommandValidatorTests.cs`, `AfvalKalender.UnitTests/Presentation/ConsoleAppTests.cs` |
| `AfvalKalender.Infrastructure.Tests` | Repository, cache, sync adapters | `AfvalKalender.Infrastructure.Tests/EfAfvalRepositoryTests.cs`, `AfvalKalender.Infrastructure.Tests/Cache/CacherendeAfvalApiTests.cs`, `AfvalKalender.Infrastructure.Tests/Sync/WebDavSyncAdapterTests.cs` |
| `AfvalKalender.DesktopUI.Tests` | Desktop view model with a mocked handler | `AfvalKalender.DesktopUI.Tests/MainWindowViewModelTests.cs` |
| `AfvalKalender.AndroidUI.Tests` | Android view model | `AfvalKalender.AndroidUI.Tests/MainPageViewModelTests.cs` |

## Run

Run each project on its own. `dotnet test` on the whole solution fails where the Android SDK is missing, and CI does not run the Android tests.

```bash
dotnet test AfvalKalender.UnitTests/AfvalKalender.UnitTests.csproj
dotnet test AfvalKalender.Infrastructure.Tests/AfvalKalender.Infrastructure.Tests.csproj
dotnet test AfvalKalender.DesktopUI.Tests/AfvalKalender.DesktopUI.Tests.csproj

# one test by name
dotnet test AfvalKalender.UnitTests --filter "FullyQualifiedName~AdresTests.Constructor_MetGeldigeData"
```

## Conventions

- Test names are `Method_Scenario_ExpectedResult` in Dutch, for example `Constructor_MetGeldigeData_ZouAdresMoetenAanmaken`.
- Arrange, Act and Assert comments separate the three parts.
- **Never mock domain entities**; create them directly.
- **Mock outbound ports** (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`, `IPdfExporter`, `IAfvalKalenderSynchronisator`) with Moq.
- Repository tests use the EF Core InMemory provider.
- View model tests use Avalonia.Headless with a mocked `ICommandHandler<,>`.
- Sync adapter tests intercept `HttpMessageHandler.SendAsync` with `Moq.Protected`.
- Cache tests inject a fake clock (`Func<DateTime>`) to control the 24 hour expiry.
- Use made-up postcodes (`1234AB`), never real ones.

## Coverage

CI collects coverage with `coverlet.collector` for the three projects above and uploads it as the `coverage-reports` artifact; `scripts/job-summary.sh` writes a summary on the run page ([Continuous integration](ci.md)). Locally:

```bash
dotnet test AfvalKalender.UnitTests/AfvalKalender.UnitTests.csproj --collect:"XPlat Code Coverage"
```
