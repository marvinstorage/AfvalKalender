using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;

namespace AfvalKalender.Application.Commands;

public class VerwerkKalenderCommandHandler
    : ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>
{
    private readonly IAfvalApi _afvalApi;
    private readonly IAfvalRepository _afvalRepository;
    private readonly IIcsExporter _icsExporter;

    public VerwerkKalenderCommandHandler(
        IAfvalApi afvalApi,
        IAfvalRepository afvalRepository,
        IIcsExporter icsExporter)
    {
        _afvalApi = afvalApi;
        _afvalRepository = afvalRepository;
        _icsExporter = icsExporter;
    }

    public async Task<IReadOnlyList<AfvalOphaalMoment>> HandleAsync(
        VerwerkKalenderCommand command, CancellationToken ct = default)
    {
        var uniekId = await _afvalApi.HaalUniekAdresIdOpAsync(command.Postcode, command.Huisnummer, command.CompanyCode);
        var momenten = await _afvalApi.HaalKalenderOpAsync(uniekId, command.Postcode, command.Huisnummer, command.Jaar, command.CompanyCode);

        await _afvalRepository.SlaOpOfUpdateAsync(momenten);

        var opgeslagenMomenten = await _afvalRepository.HaalOpVoorAdresEnJaarAsync(
            command.Postcode, command.Huisnummer, command.Jaar);

        await _icsExporter.ExporteerAsync(opgeslagenMomenten, command.OutputPad, command.HerinneringUur);

        return opgeslagenMomenten.ToList().AsReadOnly();
    }
}
