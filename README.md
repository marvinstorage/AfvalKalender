# AfvalKalender Exporteur (.NET)

Een moderne C# .NET console, desktop en mobiele applicatie om afvalophaalschema's op te halen bij Nederlandse afvalverwerkers en te exporteren naar een `.ics` bestand voor gebruik in digitale agenda's zoals Google Calendar of Outlook.

## Doel van de applicatie

De applicatie automatiseert het proces van het bijhouden van de afvalkalender. In plaats van handmatig data over te nemen of een onhandige PDF te gebruiken, haalt dit programma de actuele data op direct bij de bron (Ximmio afvalverwerker API), slaat deze op in een lokale database, en genereert een gestandaardiseerd kalenderbestand met instelbare herinneringen. De applicatie ondersteunt meerdere afvalverwerkers in Nederland die gebruik maken van de Ximmio API.

## Functionaliteiten

-   **Meerdere afvalverwerkers:** Kies uit 16 ondersteunde afvalverwerkers in Nederland (Twente Milieu, ACV, Almere, Avalex, Avri, en meer).
-   **Postcode & Huisnummer:** Voer je eigen adresgegevens in.
-   **API Integratie:** Haalt data op via de Ximmio afvalverwerker API.
-   **Lokale Database:** Slaat ophaalmomenten op in een SQLite database.
-   **Update Logica:** Herkent wijzigingen in het ophaalschema en werkt deze bij, inclusief een tijdstempel van de laatste wijziging.
-   **ICS Export & Share:** Genereert een `.ics` bestand of deelt deze direct met je agenda-app op mobiel.
-   **WebDAV / CalDAV Sync:** Directe synchronisatie naar je Google Calendar, iCloud, Nextcloud, of Office 365.
-   **Aanpasbare Reminders:** Stel zelf in hoeveel uur van tevoren je een melding wilt krijgen.
-   **Nederlandstalig:** De interface en kalender-omschrijvingen zijn volledig in het Nederlands.
-   **Groene Recycle Branding:** Een frisse, herkenbare interface met recycle-icoon.
-   **API Cache:** Slaat API-antwoorden 24 uur op in lokale cachebestanden om rate-limiting te voorkomen.

## Gebruik

### Vereisten
-   **.NET 10.0 SDK** (als u de broncode zelf wilt compileren).
-   Geen vereisten als u de **Self-contained executables** gebruikt van de [GitHub Releases](https://github.com/marvinstorage/AfvalKalender/releases) pagina.

De applicatie is beschikbaar in drie varianten:
1.  **Console UI:** Een interactieve, rijke Terminal User Interface (TUI) gebouwd met Spectre.Console, met keuzemenu's en kleurrijke tabellen.
2.  **Desktop GUI:** Een grafische interface (Windows & Linux).
3.  **Android App:** Een native mobiele app (.apk).

### Uitvoeren vanaf de broncode

#### Console UI
```bash
dotnet run --project AfvalKalender.ConsoleUI
```

#### Desktop GUI
```bash
dotnet run --project AfvalKalender.DesktopUI
```

#### Android App (MAUI)
```bash
dotnet build AfvalKalender.AndroidUI -t:Run -f net10.0-android
```

### Self-contained Executables (Zonder .NET installatie)
U kunt kant-en-klare versies downloaden voor uw systeem:

#### Desktop GUI (Aanbevolen)
- **Windows:** Download `AfvalKalender_Exporteur_GUI_v1.2.0_win-x64.exe`.
- **Linux:** Download `AfvalKalender_Exporteur_GUI_v1.2.0_linux-x64`.

#### Console UI
- **Windows:** Download `AfvalKalender_Exporteur_CLI_v1.2.0_win-x64.exe`.
- **Linux:** Download `AfvalKalender_Exporteur_CLI_v1.2.0_linux-x64` of installeer via het `.deb` pakket.

#### Android App
- **Android:** Download `AfvalKalender_Exporteur_v1.2.0.apk` voor side-loading op apparaten zoals de Pixel 7 of Motorola G84.

---

### Installatie via .DEB (Ubuntu/Debian)
Voor Ubuntu-gebruikers is er een `.deb` pakket beschikbaar:
1. Download `afvalkalender-exporteur_1.2.0_amd64.deb` van de releases pagina.
2. Installeer het pakket:
   ```bash
   sudo dpkg -i afvalkalender-exporteur_1.2.0_amd64.deb
   ```
3. U kunt de applicatie nu overal starten met het commando:
   ```bash
   afvalkalender-exporteur
   ```

---

## Architectuur en Design

De applicatie is opgezet volgens moderne software engineering principes, met een focus op testbaarheid, onderhoudbaarheid en ontkoppeling van externe systemen.

### 1. Hexagonale Architectuur (Clean Architecture)
We gebruiken een **Hexagonale Architectuur** (ook wel Ports & Adapters genoemd). Dit zorgt ervoor dat de kern van de applicatie (het Domein en de Applicatielaag) volledig onafhankelijk is van de gebruikersinterface, de database en de externe API's van afvalverwerkers.

-   **Domein Laag:** Bevat de pure business logica, entiteiten (`Adres`, `AfvalOphaalMoment`), domein-events (`AfvalOphaalMomentToegevoegd`, `AfvalOphaalMomentGewijzigd`) en de definities van de uitgaande poorten (Interfaces). Geen externe afhankelijkheden.
-   **Applicatie Laag:** Bevat de use-case orkestratie. We gebruiken een **Light CQRS** patroon met een hand-gerolde `ICommandHandler`. Dit vervangt complexe libraries zoals MediatR en biedt een duidelijke 'inbound port' voor alle interfaces. Een validatiedecorator (`ValidatingCommandHandlerDecorator`) valideert automatisch alle binnenkomende commando's op postcodeformaat, geldige GUID en overige parameters.
-   **Infrastructure Laag:** Bevat de concrete implementaties ('adapters') voor de buitenwereld, zoals de `TwenteMilieuApi` (HTTP), `EfAfvalRepository` (SQLite) en de `IcsExporter`. De repository implementeert tevens een transactionele **Outbox Pattern** (`OutboxMessage`) om domein-events betrouwbaar in de database te loggen tijdens ophaalschema-updates.
-   **Presentation Laag:** De verschillende interfaces (Console, Desktop, Android) zijn slechts dunne schillen die commando's sturen naar de applicatielaag.

```mermaid
graph TD
    subgraph UI ["Presentation Lagen (Driving Adapters)"]
        Console[ConsoleUI]
        Desktop[DesktopUI]
        Android[AndroidUI - MAUI 10]
    end

    subgraph Application ["Application Laag (Inbound Ports)"]
        Handler[VerwerkKalenderCommandHandler]
        Command[VerwerkKalenderCommand]
        IHandler[ICommandHandler]
    end

    subgraph Infrastructure ["Infrastructure Laag (Driven Adapters)"]
        Cache[CacherendeAfvalApi\nDecorator]
        Api[XimmioApi / TwenteMilieuApi\nHTTP adapter]
        Repo[EfAfvalRepository]
        Ics[IcsExporter]
    end

    subgraph Domain ["Domain Laag (Core)"]
        Entities[Entities: Adres, AfvalOphaalMoment]
        ValueObjects[Value Objects: AfvalVerwerker, AfvalType]
        Interfaces[Outbound Ports: IAfvalApi, IAfvalRepository, IIcsExporter]
    end

    Console -- ICommandHandler --> Handler
    Desktop -- ICommandHandler --> Handler
    Android -- ICommandHandler --> Handler
    Console -. "AfvalVerwerkers.Alle" .-> ValueObjects
    Desktop -. "AfvalVerwerkers.Alle" .-> ValueObjects
    Android -. "AfvalVerwerkers.Alle" .-> ValueObjects
    Command -. gebruikt door .-> Handler
    Handler --> Domain
    Cache -- implements --> Interfaces
    Cache -- wraps --> Api
    Repo -- implements --> Interfaces
    Ics -- implements --> Interfaces
    Infrastructure -. depends on .-> Domain
```

### 2. Moderne Mobile Development (MAUI 10)
De Android applicatie is geoptimaliseerd voor **.NET 10** en **Android 16** readiness:
-   **CreateWindow Pattern:** Gebruikt het nieuwste MAUI windowing model voor betere lifecycle management.
-   **Compiled Bindings:** Alle XAML bindings zijn voorzien van `x:DataType` voor maximale runtime performance en compile-time validatie.

### 3. Systeem Context (C4 Model)
Dit diagram laat zien hoe de applicatie samenwerkt met de gebruiker en externe systemen.

```mermaid
C4Context
    title System Context diagram voor Afval Kalender
    Person(user, "Inwoner van Nederland", "Wil zijn/haar afvalophaalschema in een digitale agenda hebben.")
    System(app, "Afval Kalender", "Haalt ophaaldata op bij de gekozen afvalverwerker en genereert een universeel ICS bestand.")
    System_Ext(ximmio, "Ximmio API (wasteapi.ximmio.com)", "Gedeeld API-platform voor meerdere Nederlandse afvalverwerkers.")
    System_Ext(calendar, "Digitale Agenda", "Google Calendar, Outlook, Apple Calendar, etc.")

    Rel(user, app, "Kiest afvalverwerker, voert adresgegevens en voorkeuren in")
    Rel(app, ximmio, "Haalt actuele data op met companyCode per verwerker", "HTTPS/JSON")
    Rel(app, calendar, "Importeert gegenereerd ICS bestand")
```

### 3. Procesverloop (Sequence Diagram)
Hieronder zie je de stappen die de applicatie doorloopt wanneer er op de "Verwerk" knop wordt gedrukt of het commando wordt uitgevoerd.

```mermaid
sequenceDiagram
    actor User
    participant UI as Desktop/Console UI
    participant App as VerwerkKalenderCommandHandler
    participant Cache as CacherendeAfvalApi
    participant API as TwenteMilieuApi
    participant DB as EfAfvalRepository (SQLite)
    participant ICS as IcsExporter

    User->>UI: Kies afvalverwerker, voer Postcode & Huisnummer in
    UI->>App: HandleAsync(VerwerkKalenderCommand met companyCode)
    App->>Cache: HaalUniekAdresIdOpAsync(postcode, huisnummer, companyCode)
    alt Cache geldig (minder dan 24 uur oud)
        Cache-->>App: UniqueId (uit cache)
    else Cache verlopen of afwezig
        Cache->>API: HaalUniekAdresIdOpAsync(postcode, huisnummer, companyCode)
        API-->>Cache: UniqueId
        Cache-->>App: UniqueId (opgeslagen in cache)
    end
    App->>Cache: HaalKalenderOpAsync(UniqueId, jaar, companyCode)
    alt Cache geldig (minder dan 24 uur oud)
        Cache-->>App: Lijst van ophaalmomenten (uit cache)
    else Cache verlopen of afwezig
        Cache->>API: HaalKalenderOpAsync(UniqueId, jaar, companyCode)
        API-->>Cache: Lijst van ophaalmomenten
        Cache-->>App: Lijst van ophaalmomenten (opgeslagen in cache)
    end
    App->>DB: SlaOpOfUpdateAsync(momenten)
    DB-->>App: Database bijgewerkt & LaatstGewijzigd getrackt
    App->>ICS: ExporteerAsync(momenten, bestandspad)
    ICS-->>App: ICS bestand aangemaakt (met herinneringen)
    opt WebDAV / CalDAV Url aanwezig
        App->>WebDAV: HTTP PUT met ICS data
        WebDAV-->>App: Succesvol gesynchroniseerd
    end
    App-->>UI: Succes melding
    UI-->>User: Toont clickable link naar ICS bestand of sync succes
```

### 4. Domain Driven Design (DDD) & Event Storming
De structuur is gebaseerd op een duidelijke domeintaal (Ubiquitous Language). Concepten uit de realiteit, zoals "ophaalmomenten", zijn direct terug te vinden in de code. De logica rondom het updaten van ophaalmomenten is ingekapseld in de entiteit zelf (`Update` methode), wat zorgt voor een rijk domeinmodel in plaats van een anemic model.

### 5. Business Logic Details
-   **Afvalverwerker selectie:** De gebruiker kiest een verwerker uit `AfvalVerwerkers.Alle` (value object in het domein). De bijbehorende `CompanyCode` (een UUID) wordt meegegeven in `VerwerkKalenderCommand` en doorgegeven aan alle Ximmio API-aanroepen via de `IAfvalApi` poort. Alle ondersteunde verwerkers gebruiken hetzelfde JSON-formaat.
-   **Postcode normalisatie:** Spaties in de postcode worden automatisch verwijderd in alle UIs (Console, Desktop, Android) vóór het aanmaken van het commando (`"1234 AB"` → `"1234AB"`). De Ximmio API accepteert geen postcodes met spaties.
-   **Uniek Adres:** De API vereist eerst een `UniqueId` op basis van postcode, huisnummer en `companyCode` voordat de kalender opgehaald kan worden. Als het adres niet gevonden wordt, geeft de applicatie een duidelijke foutmelding dat het adres mogelijk buiten het servicegebied van de geselecteerde verwerker valt.
-   **Idempotentie:** Als de applicatie meerdere keren wordt gedraaid voor hetzelfde jaar, zullen bestaande momenten in de database worden bijgewerkt als de omschrijving is veranderd. De kolom `LaatstGewijzigd` houdt bij wanneer dit voor het laatst is gebeurd.
-   **Reminder Trigger:** De ICS exporter gebruikt een relatieve trigger (`-PTnH`) om herinneringen in te stellen op het exacte aantal uren dat de gebruiker heeft opgegeven.
-   **API Cache:** `CacherendeAfvalApi` slaat zowel het adres-ID als de kalenderdata op als JSON-bestanden in de map `apicache/` (of de cache-directory van het platform op mobiel). De `companyCode` maakt deel uit van de bestandsnaam zodat cache-entries per verwerker worden gescheiden. De cache is 24 uur geldig. De cache kan worden gepasseerd/geïnvalideerd door de `ForceerVernieuwen` parameter op de command/API-aanroep te gebruiken.

## Afhankelijkheden

De applicatie is opgezet met een minimale en bewuste selectie van externe libraries. Hieronder een overzicht per categorie:

### Domein & Infrastructuur
| Package | Versie | Doel | Licentie |
|---|---|---|---|
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.9 | SQLite persistentie via EF Core | MIT |
| `Ical.Net` | 5.2.2 | Genereren van ICS/iCalendar bestanden | MIT |
| `Newtonsoft.Json` | 13.0.4 | JSON verwerking van API-responses | MIT |
| `Microsoft.Extensions.DependencyInjection` | 10.0.9 | Dependency Injection container | MIT |
| `Microsoft.Extensions.Http` | 8.0.1 | Typed `HttpClient` factory | MIT |
| `Microsoft.Extensions.Logging` | 10.0.9 | Logging abstractie | MIT |

### Console UI
| Package | Versie | Doel | Licentie |
|---|---|---|---|
| `Spectre.Console` | 0.57.1 | Rijke terminal UI (tabellen, prompts, live display) | MIT |
| `Microsoft.Extensions.Hosting` | 8.0.1 | Generic Host voor DI & lifecycle beheer | MIT |

### Desktop UI (Avalonia)
| Package | Versie | Doel | Licentie |
|---|---|---|---|
| `Avalonia` | 12.0.4 | Cross-platform GUI framework | MIT |
| `Avalonia.Desktop` | 12.0.4 | Avalonia desktop runtime | MIT |
| `Avalonia.Themes.Fluent` | 12.0.4 | Fluent Design thema | MIT |
| `Avalonia.Fonts.Inter` | 12.0.4 | Inter lettertype bundel | MIT |
| `CommunityToolkit.Mvvm` | 8.4.0 | MVVM patronen (source generators) | MIT |

### Android UI (MAUI)
| Package | Versie | Doel | Licentie |
|---|---|---|---|
| `Microsoft.Maui.Controls` | *(MauiVersion)* | Cross-platform MAUI framework | MIT |

### Testen
| Package | Versie | Doel | Licentie |
|---|---|---|---|
| `xunit` | 2.9.3 | Test framework | Apache 2.0 |
| `Moq` | 4.20.72 | Mocking van interfaces | BSD-3 |
| `FluentAssertions` | 8.10.0 | Leesbare assertions | Apache 2.0 |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.8 | In-memory EF Core voor repository-tests | MIT |
| `Avalonia.Headless.XUnit` | 11.1.0 | Headless Avalonia voor ViewModel-tests | MIT |
| `coverlet.collector` | 6.0.4 | Code coverage collectie | MIT |

## Licentie
Dit project is gelicentieerd onder de **GNU General Public License v3.0 (GPL-3.0)**. Zie het [LICENSE](LICENSE) bestand voor de volledige tekst.
