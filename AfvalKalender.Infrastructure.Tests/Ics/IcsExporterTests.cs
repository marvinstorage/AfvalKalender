using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Infrastructure.Ics;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.Infrastructure.Tests.Ics;

public class IcsExporterTests
{
    private static async Task<string> ExporteerAsync(Taal taal)
    {
        var pad = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ics");
        try
        {
            var momenten = new[]
            {
                new AfvalOphaalMoment(AfvalType.GRIJS, new DateTime(2026, 3, 4), "Restafval wordt opgehaald", "1234AB", "10")
            };
            await new IcsExporter().ExporteerAsync(momenten, pad, 13, taal);
            return await File.ReadAllTextAsync(pad);
        }
        finally
        {
            File.Delete(pad);
        }
    }

    [Fact]
    public async Task ExporteerAsync_Nederlands_ZouOpgeslagenOmschrijvingMoetenGebruiken()
    {
        var ics = await ExporteerAsync(Taal.Nederlands);

        ics.Should().Contain("SUMMARY:Restafval wordt opgehaald");
        ics.Should().Contain("DESCRIPTION:Herinnering: Restafval wordt opgehaald");
    }

    [Fact]
    public async Task ExporteerAsync_Engels_ZouVertaaldeSamenvattingEnHerinneringMoetenGeven()
    {
        var ics = await ExporteerAsync(Taal.Engels);

        ics.Should().Contain("SUMMARY:General waste is collected");
        ics.Should().Contain("DESCRIPTION:Reminder: General waste is collected");
    }

    [Fact]
    public async Task ExporteerAsync_PerTaal_ZouDezelfdeUidMoetenGeven()
    {
        var nl = await ExporteerAsync(Taal.Nederlands);
        var en = await ExporteerAsync(Taal.Engels);

        nl.Should().Contain("UID:GRIJS_20260304_1234AB");
        en.Should().Contain("UID:GRIJS_20260304_1234AB");
    }
}
