using System;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Events;

public record AfvalOphaalMomentGewijzigd(
    int MomentId,
    AfvalType Type,
    DateTime Datum,
    string OudeOmschrijving,
    string NieuweOmschrijving,
    string Postcode,
    string Huisnummer) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
