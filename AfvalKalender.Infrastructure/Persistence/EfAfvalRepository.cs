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

        await _context.SaveChangesAsync();
    }
}
