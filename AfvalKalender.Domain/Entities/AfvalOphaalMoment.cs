using AfvalKalender.Domain.ValueObjects;

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
    }

    public void Update(string omschrijving)
    {
        if (Omschrijving != omschrijving)
        {
            Omschrijving = omschrijving;
            LaatstGewijzigd = DateTime.Now;
        }
    }
}
