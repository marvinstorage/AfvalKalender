using AfvalKalender.Domain.Entities;

namespace AfvalKalender.Domain.Interfaces;

public interface IAfvalRepository
{
    Task<IEnumerable<AfvalOphaalMoment>> HaalOpVoorAdresEnJaarAsync(string postcode, string huisnummer, int jaar);
    Task SlaOpOfUpdateAsync(IEnumerable<AfvalOphaalMoment> momenten);
}
