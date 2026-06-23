using System.Collections.Generic;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Interfaces;

public interface IAfvalKalenderSynchronisator
{
    bool Ondersteunt(SyncProvider provider);
    Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur);
}
