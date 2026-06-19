using System.Collections.Generic;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;

namespace AfvalKalender.Domain.Interfaces;

public interface IAfvalKalenderSynchronisator
{
    Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, string webDavUrl, string gebruikersnaam, string wachtwoord, int herinneringUur);
}
