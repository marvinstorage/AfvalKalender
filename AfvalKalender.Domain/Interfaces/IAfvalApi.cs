using AfvalKalender.Domain.Entities;

namespace AfvalKalender.Domain.Interfaces;

public interface IAfvalApi
{
    Task<string> HaalUniekAdresIdOpAsync(string postcode, string huisnummer, string companyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4", bool forceerVernieuwen = false);
    Task<IEnumerable<AfvalOphaalMoment>> HaalKalenderOpAsync(string uniekAdresId, string postcode, string huisnummer, int jaar, string companyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4", bool forceerVernieuwen = false);
}
