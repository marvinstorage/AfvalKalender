using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Interfaces;

public interface IPdfExporter
{
    Task ExporteerAsync(IEnumerable<AfvalOphaalMoment> momenten, string bestandspad, int jaar, Taal taal);
}
