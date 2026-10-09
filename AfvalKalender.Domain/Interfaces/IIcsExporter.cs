using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Interfaces;

public interface IIcsExporter
{
    Task ExporteerAsync(IEnumerable<AfvalOphaalMoment> momenten, string bestandspad, int herinneringUurVooraf, Taal taal);
}
