using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.Services;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Application.Commands;

public class VerwerkKalenderCommandHandler
    : ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>
{
    private readonly IAfvalApi _afvalApi;
    private readonly IAfvalRepository _afvalRepository;
    private readonly IIcsExporter _icsExporter;
    private readonly KalenderSynchronisatieService _synchronisatieService;

    public VerwerkKalenderCommandHandler(
        IAfvalApi afvalApi,
        IAfvalRepository afvalRepository,
        IIcsExporter icsExporter,
        KalenderSynchronisatieService synchronisatieService)
    {
        _afvalApi = afvalApi;
        _afvalRepository = afvalRepository;
        _icsExporter = icsExporter;
        _synchronisatieService = synchronisatieService;
    }

    public async Task<IReadOnlyList<AfvalOphaalMoment>> HandleAsync(
        VerwerkKalenderCommand command, CancellationToken ct = default)
    {
        var uniekId = await _afvalApi.HaalUniekAdresIdOpAsync(command.Postcode, command.Huisnummer, command.CompanyCode, command.ForceerVernieuwen);
        var momenten = await _afvalApi.HaalKalenderOpAsync(uniekId, command.Postcode, command.Huisnummer, command.Jaar, command.CompanyCode, command.ForceerVernieuwen);

        await _afvalRepository.SlaOpOfUpdateAsync(momenten);

        var opgeslagenMomenten = await _afvalRepository.HaalOpVoorAdresEnJaarAsync(
            command.Postcode, command.Huisnummer, command.Jaar);

        await _icsExporter.ExporteerAsync(opgeslagenMomenten, command.OutputPad, command.HerinneringUur);

        var config = new SyncConfiguratie(command.SyncProvider, command.SyncDoelUrlOfToken ?? "", command.SyncGebruiker ?? "", command.SyncWachtwoord ?? "");
        await _synchronisatieService.SynchroniseerAsync(opgeslagenMomenten, config, command.HerinneringUur);

        return opgeslagenMomenten.ToList().AsReadOnly();
    }
}
