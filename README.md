# Afval Kalender (.NET)

Een moderne C# .NET console en desktop applicatie om afvalophaalschema's op te halen bij Nederlandse afvalverwerkers en te exporteren naar een `.ics` bestand voor gebruik in digitale agenda's zoals Google Calendar of Outlook.

## Doel van de applicatie

De applicatie automatiseert het proces van het bijhouden van de afvalkalender. In plaats van handmatig data over te nemen of een onhandige PDF te gebruiken, haalt dit programma de actuele data op direct bij de bron (Ximmio afvalverwerker API), slaat deze op in een lokale database, en genereert een gestandaardiseerd kalenderbestand met instelbare herinneringen. De applicatie ondersteunt meerdere afvalverwerkers in Nederland die gebruik maken van de Ximmio API.

## Functionaliteiten

-   **Meerdere afvalverwerkers:** Kies uit 16 ondersteunde afvalverwerkers in Nederland (Twente Milieu, ACV, Almere, Avalex, Avri, en meer).
-   **Postcode & Huisnummer:** Voer je eigen adresgegevens in.
-   **API Integratie:** Haalt data op via de Ximmio afvalverwerker API.
-   **Lokale Database:** Slaat ophaalmomenten op in een SQLite database.
-   **Update Logica:** Herkent wijzigingen in het ophaalschema en werkt deze bij, inclusief een tijdstempel van de laatste wijziging.
-   **ICS Export:** Genereert een `.ics` bestand voor import in Google Calendar, Outlook, etc.
-   **Aanpasbare Reminders:** Stel zelf in hoeveel uur van tevoren je een melding wilt krijgen.
-   **Nederlandstalig:** De interface en kalender-omschrijvingen zijn volledig in het Nederlands.
-   **Clickable Links:** Directe toegang tot het gegenereerde bestand vanuit de UI.
-   **API Cache:** Slaat API-antwoorden 24 uur op in lokale cachebestanden om rate-limiting te voorkomen.

## Gebruik

### Vereisten
-   **.NET 10.0 SDK** (als u de broncode zelf wilt compileren).
-   Geen vereisten als u de **Self-contained executables** gebruikt van de [GitHub Releases](https://github.com/marvinstorage/AfvalKalender/releases) pagina.

De applicatie is beschikbaar in twee varianten:
1.  **Console UI:** Een interactieve tekst-interface.
2.  **Desktop GUI:** Een grafische interface (Windows & Linux Gnome).

### Uitvoeren vanaf de broncode

#### Console UI
```bash
dotnet run --project AfvalKalender.ConsoleUI
```

#### Desktop GUI
```bash
dotnet run --project AfvalKalender.DesktopUI
```

### Self-contained Executables (Zonder .NET installatie)
U kunt kant-en-klare versies downloaden voor uw systeem:

#### Desktop GUI (Aanbevolen)
- **Windows:** Download `AfvalKalender_GUI_v1.1.0_win-x64.exe`.
- **Linux (Gnome/Ubuntu):** Download `AfvalKalender_GUI_v1.1.0_linux-x64`.

#### Console UI
- **Windows:** Download `AfvalKalender_CLI_v1.1.0_win-x64.exe`.
- **Linux:** Download `AfvalKalender_CLI_v1.1.0_linux-x64` of installeer via het `.deb` pakket.

---

### Installatie via .DEB (Ubuntu/Debian)
Voor Ubuntu-gebruikers is er een `.deb` pakket beschikbaar:
1. Download `afvalkalender-twente_1.1.0_amd64.deb` van de releases pagina.
2. Installeer het pakket:
   ```bash
   sudo dpkg -i afvalkalender-twente_1.1.0_amd64.deb
   ```
3. U kunt de applicatie nu overal starten met het commando:
   ```bash
   afvalkalender
   ```

---

## Architectuur en Design

De applicatie is opgezet volgens moderne software engineering principes:

### 1. Visualisatie van de Lagen (Clean Architecture / Hexagonal)
De onderstaande diagram toont hoe de verschillende onderdelen van de applicatie zich tot elkaar verhouden. De pijlen geven de richting van de afhankelijkheden aan (allemaal naar binnen, richting het domein).

```mermaid
graph TD
    subgraph UI [User Interface Lagen]
        Console[ConsoleUI]
        Desktop[DesktopUI]
    end

    subgraph Application [Application Laag]
        Handler[VerwerkKalenderCommandHandler]
        Command[VerwerkKalenderCommand]
        IHandler[ICommandHandler&lt;TCommand, TResult&gt;]
    end

    subgraph Infrastructure [Infrastructure Laag]
        Cache[CacherendeAfvalApi\nDecorator]
        Api[TwenteMilieuApi]
        Repo[EfAfvalRepository]
        Ics[IcsExporter]
    end

    subgraph Domain [Domain Laag]
        Entities[Entities: Adres, AfvalOphaalMoment]
        Interfaces[Interfaces: IAfvalApi, IAfvalRepository, IIcsExporter]
    end

    Console -- ICommandHandler --> Handler
    Desktop -- ICommandHandler --> Handler
    Command -. gebruikt door .-> Handler
    Handler --> Domain
    Cache -- implements --> Interfaces
    Cache -- wraps --> Api
    Repo -- implements --> Interfaces
    Ics -- implements --> Interfaces
    Infrastructure -. depends on .-> Domain
```

### 2. Systeem Context (C4 Model)
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
    App-->>UI: Succes melding
    UI-->>User: Toont clickable link naar ICS bestand
```

### 4. Domain Driven Design (DDD) & Event Storming
De structuur is gebaseerd op een duidelijke domeintaal (Ubiquitous Language). Concepten uit de realiteit, zoals "ophaalmomenten", zijn direct terug te vinden in de code. De logica rondom het updaten van ophaalmomenten is ingekapseld in de entiteit zelf (`Update` methode), wat zorgt voor een rijk domeinmodel in plaats van een anemic model.

### 5. Business Logic Details
-   **Afvalverwerker selectie:** De gebruiker kiest een verwerker uit `AfvalVerwerkers.Alle` (value object in het domein). De bijbehorende `CompanyCode` (een UUID) wordt meegegeven in `VerwerkKalenderCommand` en doorgegeven aan alle Ximmio API-aanroepen. Alle ondersteunde verwerkers gebruiken hetzelfde JSON-formaat.
-   **Postcode normalisatie:** Spaties in de postcode worden automatisch verwijderd in beide UIs vóór het aanmaken van het commando (`"1234 AB"` → `"1234AB"`). De Ximmio API accepteert geen postcodes met spaties.
-   **Uniek Adres:** De API vereist eerst een `UniqueId` op basis van postcode, huisnummer en `companyCode` voordat de kalender opgehaald kan worden. Als het adres niet gevonden wordt, geeft de applicatie een duidelijke foutmelding dat het adres mogelijk buiten het servicegebied van de geselecteerde verwerker valt.
-   **Idempotentie:** Als de applicatie meerdere keren wordt gedraaid voor hetzelfde jaar, zullen bestaande momenten in de database worden bijgewerkt als de omschrijving is veranderd. De kolom `LaatstGewijzigd` houdt bij wanneer dit voor het laatst is gebeurd.
-   **Reminder Trigger:** De ICS exporter gebruikt een relatieve trigger (`-PTnH`) om herinneringen in te stellen op het exacte aantal uren dat de gebruiker heeft opgegeven.
-   **API Cache:** `CacherendeAfvalApi` slaat zowel het adres-ID als de kalenderdata op als JSON-bestanden in de map `apicache/`, elk met een tijdstempel. De `companyCode` maakt deel uit van de bestandsnaam zodat cache-entries per verwerker worden gescheiden. De cache is 24 uur geldig. Bij een beschadigd of verlopen cachebestand valt de applicatie stil terug op een verse API-aanroep. Er worden geen extra NuGet-pakketten gebruikt; de serialisatie verloopt via `System.Text.Json` (BCL).

## Minimale Afhankelijkheden
De applicatie is ontworpen om zo min mogelijk externe libraries te gebruiken:
-   `Microsoft.EntityFrameworkCore.Sqlite`: Voor de database (GPL-compatibel).
-   `Ical.Net`: Voor het genereren van het ICS formaat (MIT - GPL compatibel).
-   `Newtonsoft.Json`: Voor API verwerking (MIT - GPL compatibel).
-   `Avalonia`: Voor de cross-platform GUI (MIT).
-   `CommunityToolkit.Mvvm`: Voor MVVM patronen in de GUI (MIT).

## Licentie
Dit project is gelicentieerd onder de **GNU General Public License v3.0 (GPL-3.0)**. Zie het [LICENSE](LICENSE) bestand voor de volledige tekst.
