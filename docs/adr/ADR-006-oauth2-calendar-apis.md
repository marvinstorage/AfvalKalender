# ADR-006 — OAuth2 Synchronization via Native Cloud APIs (Google & Microsoft)

**Status:** Proposed  
**Date:** 2026-06-23  
**Deciders:** Marvin Storage

## Context
Users want to synchronize their waste collection calendars directly to modern cloud calendar providers like Google Calendar and Microsoft Office 365. 

Initially, a standard WebDAV/CalDAV synchronization mechanism via `HTTP PUT` with Basic Authentication was introduced in [ADR-002](ADR-002-webdav-caldav-sync.md). However, major cloud providers (Google, Microsoft) have deprecated or heavily restricted Basic Authentication for CalDAV endpoints due to security concerns. Attempting to push ICS files directly to consumer WebDAV URLs (e.g., `https://calendar.google.com/...`) results in `405 Method Not Allowed` errors, as these endpoints require OAuth2 authentication flows and specific proprietary API integrations (Google Calendar API and Microsoft Graph API).

To offer a seamless, automated synchronization experience for the majority of users (who use Google or Microsoft ecosystems), the application needs to support modern authorization (OAuth2) and the specific REST APIs of these providers.

## Decision
We propose extending the existing synchronization architecture by introducing new adapters that implement the `IAfvalKalenderSynchronisator` outbound port using OAuth2 and vendor-specific APIs.

1.  **Multiple Sync Adapters:** Alongside the existing `WebDavSyncAdapter`, we will introduce new adapters:
    *   `GoogleCalendarSyncAdapter` (utilizing the Google.Apis.Calendar.v3 library or custom HTTP clients).
    *   `MicrosoftGraphSyncAdapter` (utilizing the Microsoft.Graph library or custom HTTP clients).
2.  **OAuth2 Integration:** The application must implement OAuth2 authorization flows (e.g., PKCE for Desktop/Mobile and Device Code Flow for Console) to obtain and securely store access and refresh tokens.
3.  **UI Updates:** The UIs (Desktop, Android, Console) must be updated to offer distinct options: "Export to ICS", "Sync to WebDAV/CalDAV", "Sync to Google Calendar", and "Sync to Office 365".
4.  **Event Mapping:** Instead of pushing raw `.ics` files, the new adapters will parse the generated `AfvalOphaalMomenten` and translate them into native `Event` objects for the respective APIs.

## Consequences
*   **Pros:**
    *   Provides a seamless, "one-click" synchronization experience for Google and Office 365 users without requiring manual ICS imports.
    *   Adheres to modern security standards (OAuth2), avoiding the risk of sending basic credentials.
    *   Finer control over event updates and deletions natively in the user's calendar.
*   **Cons:**
    *   Increases the complexity of the application, particularly in handling OAuth2 flows across three different UI platforms (Console, Desktop, Mobile).
    *   Requires registering the application in the Google Cloud Console and Microsoft Entra ID (Azure AD) to obtain Client IDs.
    *   Likely requires adding third-party NuGet dependencies (e.g., Google/Microsoft SDKs or OAuth2 libraries), deviating slightly from the strict "minimal dependencies" rule. 
*   **Next Steps:** Wait for approval and team capacity before beginning implementation, as this requires creating cloud developer accounts and significant UI changes.
