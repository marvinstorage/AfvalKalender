using AfvalKalender.Domain.Entities;

namespace AfvalKalender.Domain.Interfaces;

public interface IIcsExporter
{
    Task ExporteerAsync(IEnumerable<AfvalOphaalMoment> momenten, string bestandspad, int herinneringUurVooraf);
}
