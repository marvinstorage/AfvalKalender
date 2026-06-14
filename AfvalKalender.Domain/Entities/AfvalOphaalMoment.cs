using AfvalKalender.Domain.Events;
using AfvalKalender.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace AfvalKalender.Domain.Entities;

public class AfvalOphaalMoment
{
    public int Id { get; private set; }
    public AfvalType Type { get; private set; }
    public DateTime Datum { get; private set; }
    public string Omschrijving { get; private set; }
    public DateTime LaatstGewijzigd { get; private set; }
    public string Postcode { get; private set; }
    public string Huisnummer { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    private AfvalOphaalMoment() 
    {
        Omschrijving = string.Empty;
        Postcode = string.Empty;
        Huisnummer = string.Empty;
    } // For EF

    public AfvalOphaalMoment(AfvalType type, DateTime datum, string omschrijving, string postcode, string huisnummer)
    {
        Type = type;
        Datum = datum;
        Omschrijving = omschrijving;
        Postcode = postcode;
        Huisnummer = huisnummer;
        LaatstGewijzigd = DateTime.Now;
        
        AddDomainEvent(new AfvalOphaalMomentToegevoegd(type, datum, omschrijving, postcode, huisnummer));
    }

    public void Update(string omschrijving)
    {
        if (Omschrijving != omschrijving)
        {
            var oudeOmschrijving = Omschrijving;
            Omschrijving = omschrijving;
            LaatstGewijzigd = DateTime.Now;
            AddDomainEvent(new AfvalOphaalMomentGewijzigd(Id, Type, Datum, oudeOmschrijving, omschrijving, Postcode, Huisnummer));
        }
    }
}
