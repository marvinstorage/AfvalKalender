using AfvalKalender.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfvalKalender.Infrastructure.Persistence;

public class AfvalDbContext : DbContext
{
    public DbSet<AfvalOphaalMoment> AfvalOphaalMomenten { get; set; }

    public AfvalDbContext(DbContextOptions<AfvalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AfvalOphaalMoment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Datum).IsRequired();
            entity.Property(e => e.Postcode).IsRequired();
            entity.Property(e => e.Huisnummer).IsRequired();
        });
    }
}
