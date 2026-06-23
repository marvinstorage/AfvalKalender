namespace AfvalKalender.Domain.ValueObjects;

public record SyncConfiguratie(
    SyncProvider Provider,
    string DoelUrlOfToken,
    string Gebruikersnaam,
    string Wachtwoord
);
