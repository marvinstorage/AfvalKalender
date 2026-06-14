using AfvalKalender.Infrastructure.Persistence;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.Infrastructure.Tests;

public class EfAfvalRepositoryTests
{
    private AfvalDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AfvalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AfvalDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task SlaOpOfUpdateAsync_NieuwMoment_ZouMoetenToevoegen()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new EfAfvalRepository(context);
        var moment = new AfvalOphaalMoment(AfvalType.GRIJS, DateTime.Now, "Test", "1234AB", "10");

        // Act
        await repo.SlaOpOfUpdateAsync(new[] { moment });

        // Assert
        var opgeslagen = await context.AfvalOphaalMomenten.ToListAsync();
        opgeslagen.Should().HaveCount(1);
        opgeslagen[0].Omschrijving.Should().Be("Test");
    }

    [Fact]
    public async Task SlaOpOfUpdateAsync_BestaandMoment_ZouMoetenUpdaten()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new EfAfvalRepository(context);
        var datum = DateTime.Now.Date;
        var moment1 = new AfvalOphaalMoment(AfvalType.GRIJS, datum, "Oud", "1234AB", "10");
        await repo.SlaOpOfUpdateAsync(new[] { moment1 });

        var moment2 = new AfvalOphaalMoment(AfvalType.GRIJS, datum, "Nieuw", "1234AB", "10");

        // Act
        await repo.SlaOpOfUpdateAsync(new[] { moment2 });

        // Assert
        var opgeslagen = await context.AfvalOphaalMomenten.ToListAsync();
        opgeslagen.Should().HaveCount(1);
        opgeslagen[0].Omschrijving.Should().Be("Nieuw");
    }

    [Fact]
    public async Task SlaOpOfUpdateAsync_NieuwMoment_ZouOutboxBerichtMoetenSchrijven()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new EfAfvalRepository(context);
        var moment = new AfvalOphaalMoment(AfvalType.GRIJS, DateTime.Now, "Nieuwe Ophaaldag", "1234AB", "10");

        // Act
        await repo.SlaOpOfUpdateAsync(new[] { moment });

        // Assert
        var outboxMessages = await context.OutboxMessages.ToListAsync();
        outboxMessages.Should().HaveCount(1);
        outboxMessages[0].EventType.Should().Contain("AfvalOphaalMomentToegevoegd");
        outboxMessages[0].Content.Should().Contain("Nieuwe Ophaaldag");
    }

    [Fact]
    public async Task SlaOpOfUpdateAsync_GewijzigdMoment_ZouOutboxBerichtMoetenSchrijven()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new EfAfvalRepository(context);
        var datum = DateTime.Now.Date;
        var moment1 = new AfvalOphaalMoment(AfvalType.GRIJS, datum, "Eerste Omschrijving", "1234AB", "10");
        await repo.SlaOpOfUpdateAsync(new[] { moment1 });
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();

        var moment2 = new AfvalOphaalMoment(AfvalType.GRIJS, datum, "Gewijzigde Omschrijving", "1234AB", "10");

        // Act
        await repo.SlaOpOfUpdateAsync(new[] { moment2 });

        // Assert
        var outboxMessages = await context.OutboxMessages.ToListAsync();
        outboxMessages.Should().HaveCount(1);
        outboxMessages[0].EventType.Should().Contain("AfvalOphaalMomentGewijzigd");
        outboxMessages[0].Content.Should().Contain("Gewijzigde Omschrijving");
        outboxMessages[0].Content.Should().Contain("Eerste Omschrijving");
    }
}
