using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;

namespace AfvalKalender.Application.Services;

public class AfvalService
{
    private readonly IAfvalApi _afvalApi;
    private readonly IAfvalRepository _afvalRepository;
    private readonly IIcsExporter _icsExporter;

    public AfvalService(IAfvalApi afvalApi, IAfvalRepository afvalRepository, IIcsExporter icsExporter)
    {
        _afvalApi = afvalApi;
        _afvalRepository = afvalRepository;
        _icsExporter = icsExporter;
    }

    public async Task<IEnumerable<AfvalOphaalMoment>> VerwerkKalenderAsync(string postcode, string huisnummer, int jaar, int herinneringUur, string outputPad)
    {
        // 1. Haal uniek adres ID op via API
        var uniekId = await _afvalApi.HaalUniekAdresIdOpAsync(postcode, huisnummer);
        
        // 2. Haal kalender data op via API
        var momenten = await _afvalApi.HaalKalenderOpAsync(uniekId, postcode, huisnummer, jaar);
        
        // 3. Sla op in database (of update bestaande)
        await _afvalRepository.SlaOpOfUpdateAsync(momenten);
        
        // 4. Haal meest recente data op uit database voor de zekerheid
        var opgeslagenMomenten = await _afvalRepository.HaalOpVoorAdresEnJaarAsync(postcode, huisnummer, jaar);
        
        // 5. Exporteer naar ICS
        await _icsExporter.ExporteerAsync(opgeslagenMomenten, outputPad, herinneringUur);
        
        return opgeslagenMomenten;
    }
}
