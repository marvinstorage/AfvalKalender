using AfvalKalender.Domain.Entities;

namespace AfvalKalender.Domain.Interfaces;

public interface IAfvalApi
{
    Task<string> HaalUniekAdresIdOpAsync(string postcode, string huisnummer);
    Task<IEnumerable<AfvalOphaalMoment>> HaalKalenderOpAsync(string uniekAdresId, string postcode, string huisnummer, int jaar);
}
