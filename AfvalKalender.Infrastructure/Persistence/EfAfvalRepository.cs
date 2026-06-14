using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AfvalKalender.Infrastructure.Persistence;

public class EfAfvalRepository : IAfvalRepository
{
    private readonly AfvalDbContext _context;

    public EfAfvalRepository(AfvalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AfvalOphaalMoment>> HaalOpVoorAdresEnJaarAsync(string postcode, string huisnummer, int jaar)
    {
        return await _context.AfvalOphaalMomenten
            .Where(m => m.Postcode == postcode && m.Huisnummer == huisnummer && m.Datum.Year == jaar)
            .ToListAsync();
    }

    public async Task SlaOpOfUpdateAsync(IEnumerable<AfvalOphaalMoment> momenten)
    {
        foreach (var moment in momenten)
        {
            var bestaand = await _context.AfvalOphaalMomenten
                .FirstOrDefaultAsync(m => m.Postcode == moment.Postcode && 
                                          m.Huisnummer == moment.Huisnummer && 
                                          m.Datum.Date == moment.Datum.Date && 
                                          m.Type == moment.Type);

            if (bestaand == null)
            {
                _context.AfvalOphaalMomenten.Add(moment);
            }
            else
            {
                bestaand.Update(moment.Omschrijving);
            }
        }

        var entitiesWithEvents = _context.ChangeTracker.Entries<AfvalOphaalMoment>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    EventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                    Content = Newtonsoft.Json.JsonConvert.SerializeObject(domainEvent),
                    OccurredOn = domainEvent.OccurredOn
                };
                _context.OutboxMessages.Add(outboxMessage);
            }
            entity.ClearDomainEvents();
        }

        await _context.SaveChangesAsync();
    }
}
