import os

def update_gemini_md():
    path = "GEMINI.md"
    with open(path, "r") as f:
        code = f.read()

    # Update Domain description
    code = code.replace("and the **outbound port interfaces** (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`, `IAfvalKalenderSynchronisator`).",
                        "and the **outbound port interfaces** (`IAfvalApi`, `IAfvalRepository`, `IIcsExporter`, `IAfvalKalenderSynchronisator`) and **domain services** (`KalenderSynchronisatieService`).")
    
    # Update Infrastructure description
    code = code.replace("`WebDavSyncAdapter` (HTTP PUT to a WebDAV/CalDAV endpoint).",
                        "`WebDavSyncAdapter`, `GoogleCalendarSyncAdapter`, and `MicrosoftGraphSyncAdapter`.")

    # Update Architecture diagram
    old_diag = """┌──────────────────────────────────────┬──────────────────────┐
│  Domain (Core Business Logic)        │  Infrastructure      │
│  Entities: Adres, AfvalOphaalMoment  │  (Driven Adapters)   │
│  ValueObjects: AfvalType, Verwerker  │  TwenteMilieuApi     │
│  Interfaces (Outbound Ports):        │  EfAfvalRepository   │
│    IAfvalApi, IAfvalRepository,      │  IcsExporter         │
│    IIcsExporter,                     │  CacherendeAfvalApi  │
│    IAfvalKalenderSynchronisator      │  WebDavSyncAdapter   │
└──────────────────────────────────────┴──────────────────────┘"""

    new_diag = """┌──────────────────────────────────────┬──────────────────────┐
│  Domain (Core Business Logic)        │  Infrastructure      │
│  Entities: Adres, AfvalOphaalMoment  │  (Driven Adapters)   │
│  ValueObjects: SyncConfiguratie      │  TwenteMilieuApi     │
│  Services: KalenderSynchronisatieSvc │  EfAfvalRepository   │
│  Interfaces (Outbound Ports):        │  IcsExporter         │
│    IAfvalApi, IAfvalRepository,      │  CacherendeAfvalApi  │
│    IIcsExporter,                     │  WebDavSyncAdapter   │
│    IAfvalKalenderSynchronisator      │  GoogleCalendarSync..│
│                                      │  MicrosoftGraphSync..│
└──────────────────────────────────────┴──────────────────────┘"""
    code = code.replace(old_diag, new_diag)

    # Update Core workflow
    code = code.replace("6. `IAfvalKalenderSynchronisator.SynchroniseerAsync` → (Optional) Pushes the calendar to a WebDAV endpoint if a URL was provided.",
                        "6. `KalenderSynchronisatieService.SynchroniseerAsync` → Orchestrates syncing the calendar via the correct `IAfvalKalenderSynchronisator` (WebDAV, Google, Microsoft) based on the `SyncProvider` in the command.")

    with open(path, "w") as f:
        f.write(code)

def update_claude_md():
    path = "CLAUDE.md"
    with open(path, "r") as f:
        code = f.read()
    
    # Update components
    code = code.replace("| `WebDavSyncAdapter` | `IAfvalKalenderSynchronisator` | HTTP Client | Generates temp ICS, HTTP PUTs to CalDAV server with optional Basic Auth. See [ADR-002](docs/adr/ADR-002-webdav-caldav-sync.md). |",
                        "| `WebDavSyncAdapter` | `IAfvalKalenderSynchronisator` | HTTP Client | Generates temp ICS, HTTP PUTs to CalDAV server with optional Basic Auth. |\n| `GoogleCalendarSyncAdapter` | `IAfvalKalenderSynchronisator` | HTTP Client | Syncs to Google Calendar via OAuth2 APIs. |\n| `MicrosoftGraphSyncAdapter` | `IAfvalKalenderSynchronisator` | HTTP Client | Syncs to MS Graph via OAuth2 APIs. |")

    with open(path, "w") as f:
        f.write(code)

update_gemini_md()
update_claude_md()
print("Docs updated")
