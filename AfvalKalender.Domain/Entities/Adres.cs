namespace AfvalKalender.Domain.Entities;

public class Adres
{
    public string Postcode { get; private set; }
    public string Huisnummer { get; private set; }
    public string? UniekId { get; private set; }

    public Adres(string postcode, string huisnummer, string? uniekId = null)
    {
        if (string.IsNullOrWhiteSpace(postcode)) throw new ArgumentException("Postcode is verplicht", nameof(postcode));
        if (string.IsNullOrWhiteSpace(huisnummer)) throw new ArgumentException("Huisnummer is verplicht", nameof(huisnummer));
        
        Postcode = postcode;
        Huisnummer = huisnummer;
        UniekId = uniekId;
    }

    public void SetUniekId(string uniekId)
    {
        UniekId = uniekId;
    }
}
