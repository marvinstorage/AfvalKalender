using System;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Events;

public record AfvalOphaalMomentToegevoegd(
    AfvalType Type,
    DateTime Datum,
    string Omschrijving,
    string Postcode,
    string Huisnummer) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
