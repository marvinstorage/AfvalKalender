# Afval Kalender Twente (.NET)

Een moderne C# .NET console applicatie om afvalophaalschema's van Twente Milieu op te halen en te exporteren naar een `.ics` bestand voor gebruik in digitale agenda's zoals Google Calendar of Outlook.

## Doel van de applicatie

De applicatie automatiseert het proces van het bijhouden van de afvalkalender in de regio Twente. In plaats van handmatig data over te nemen of een onhandige PDF te gebruiken, haalt dit programma de actuele data op direct bij de bron (Twente Milieu API), slaat deze op in een lokale database, en genereert een gestandaardiseerd kalenderbestand met instelbare herinneringen.

## Functionaliteiten

-   **Postcode & Huisnummer:** Voer je eigen adresgegevens in.
-   **API Integratie:** Haalt data op via de Twente Milieu API.
-   **Lokale Database:** Slaat ophaalmomenten op in een SQLite database.
-   **Update Logica:** Herkent wijzigingen in het ophaalschema en werkt deze bij, inclusief een tijdstempel van de laatste wijziging.
-   **ICS Export:** Genereert een `.ics` bestand voor import in Google Calendar, Outlook, etc.
-   **Aanpasbare Reminders:** Stel zelf in hoeveel uur van tevoren je een melding wilt krijgen.
-   **Nederlandstalig:** De interface en kalender-omschrijvingen zijn volledig in het Nederlands.

## Gebruik

### Vereisten
-   **.NET 10.0 SDK** (als u de broncode zelf wilt compileren).
-   Geen vereisten als u de **Self-contained executables** gebruikt van de [GitHub Releases](https://github.com/marvinstorage/AfvalKalender/releases) pagina.

### Uitvoeren vanaf de broncode
1. Clone de repository.
2. Navigeer naar de projectmap.
3. Gebruik het passende commando voor uw platform:

#### Windows (PowerShell of CMD)
```powershell
dotnet run --project AfvalKalender.ConsoleUI
```

#### Linux (Ubuntu/Debian/etc.)
```bash
export PATH=$PATH:$HOME/.dotnet
dotnet run --project AfvalKalender.ConsoleUI
```

#### macOS
```bash
dotnet run --project AfvalKalender.ConsoleUI
```

### Self-contained Executables (Zonder .NET installatie)
U kunt kant-en-klare versies downloaden voor uw systeem:
- **Windows:** Download `AfvalKalender_win-x64.zip`, pak het uit en start `AfvalKalender.ConsoleUI.exe`.
- **Linux:** Download `AfvalKalender_linux-x64`, maak het uitvoerbaar (`chmod +x`) en start het met `./AfvalKalender.ConsoleUI`.
- **macOS:** Download de versie voor Intel (`osx-x64`) of Apple Silicon (`osx-arm64`).

---

## Architectuur en Design

De applicatie is opgezet volgens moderne software engineering principes:

### 1. Clean Architecture (Hexagonal)
Het project is verdeeld in vier lagen om een strikte scheiding van verantwoordelijkheden te garanderen:

-   **Domain:** Bevat de business entiteiten (`Adres`, `AfvalOphaalMoment`) en interfaces. Dit is het hart van de applicatie en heeft geen afhankelijkheden van externe bibliotheken of andere lagen.
-   **Application:** Bevat de orchestration logic (`AfvalService`). Hier wordt bepaald welke stappen nodig zijn om de use case "verwerk kalender" uit te voeren.
-   **Infrastructure:** Bevat de concrete implementaties van externe systemen:
    -   `TwenteMilieuApi`: De HTTP client voor de API.
    -   `EfAfvalRepository`: De database implementatie met Entity Framework Core en SQLite.
    -   `IcsExporter`: De logica voor het genereren van het `.ics` bestand.
-   **ConsoleUI:** De gebruikersinterface die de input verzamelt en de `Application` laag aanstuurt.

### 2. Domain Driven Design (DDD) & Event Storming
De structuur is gebaseerd op een duidelijke domeintaal (Ubiquitous Language). Concepten uit de realiteit, zoals "ophaalmomenten", zijn direct terug te vinden in de code. De logica rondom het updaten van ophaalmomenten is ingekapseld in de entiteit zelf (`Update` methode), wat zorgt voor een rijk domeinmodel in plaats van een anemic model.

### 3. Business Logic Details
-   **Uniek Adres:** De API vereist eerst een `UniqueId` op basis van postcode en huisnummer voordat de kalender opgehaald kan worden.
-   **Idempotentie:** Als de applicatie meerdere keren wordt gedraaid voor hetzelfde jaar, zullen bestaande momenten in de database worden bijgewerkt als de omschrijving is veranderd. De kolom `LaatstGewijzigd` houdt bij wanneer dit voor het laatst is gebeurd.
-   **Reminder Trigger:** De ICS exporter gebruikt een relatieve trigger (`-PTnH`) om herinneringen in te stellen op het exacte aantal uren dat de gebruiker heeft opgegeven.

## Minimale Afhankelijkheden
De applicatie is ontworpen om zo min mogelijk externe libraries te gebruiken:
-   `Microsoft.EntityFrameworkCore.Sqlite`: Voor de database (GPL-compatibel).
-   `Ical.Net`: Voor het genereren van het ICS formaat (MIT - GPL compatibel).
-   `Newtonsoft.Json`: Voor API verwerking (MIT - GPL compatibel).

## Licentie
Dit project is gelicentieerd onder de **GNU General Public License v3.0 (GPL-3.0)**. Zie het [LICENSE](LICENSE) bestand voor de volledige tekst.
