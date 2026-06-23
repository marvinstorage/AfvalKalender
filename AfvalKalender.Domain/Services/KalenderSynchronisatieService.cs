using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Services;

public class KalenderSynchronisatieService
{
    private readonly IEnumerable<IAfvalKalenderSynchronisator> _synchronisatoren;

    public KalenderSynchronisatieService(IEnumerable<IAfvalKalenderSynchronisator> synchronisatoren)
    {
        _synchronisatoren = synchronisatoren;
    }

    public async Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur)
    {
        if (configuratie.Provider == SyncProvider.Geen)
        {
            return;
        }

        var synchronisator = _synchronisatoren.FirstOrDefault(s => s.Ondersteunt(configuratie.Provider));
        if (synchronisator == null)
        {
            throw new System.NotSupportedException($"Geen synchronisator gevonden voor provider {configuratie.Provider}.");
        }

        await synchronisator.SynchroniseerAsync(momenten, configuratie, herinneringUur);
    }
}
