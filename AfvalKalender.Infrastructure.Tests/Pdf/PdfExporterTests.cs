using System.Text;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Infrastructure.Pdf;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.Infrastructure.Tests.Pdf;

public class PdfExporterTests
{
    private static async Task<string> ExporteerAsync(IEnumerable<AfvalOphaalMoment> momenten, Taal taal, int jaar = 2026)
    {
        var pad = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        try
        {
            await new PdfExporter().ExporteerAsync(momenten, pad, jaar, taal);
            return Encoding.Latin1.GetString(await File.ReadAllBytesAsync(pad));
        }
        finally
        {
            File.Delete(pad);
        }
    }

    private static AfvalOphaalMoment Moment(AfvalType type, int maand, int dag, string huisnummer = "10") =>
        new(type, new DateTime(2026, maand, dag), "x", "1234AB", huisnummer);

    [Fact]
    public async Task ExporteerAsync_MetMomenten_ZouGeldigeA4PdfMoetenSchrijven()
    {
        var pdf = await ExporteerAsync(new[] { Moment(AfvalType.GRIJS, 3, 4) }, Taal.Nederlands);

        pdf.Should().StartWith("%PDF-");
        pdf.TrimEnd().Should().EndWith("%%EOF");
        pdf.Should().Contain("/MediaBox [0 0 595 842]");
        pdf.Should().Contain("/Count 1");
    }

    [Fact]
    public async Task ExporteerAsync_ZouXrefOffsetsMoetenKloppen()
    {
        var pdf = await ExporteerAsync(new[] { Moment(AfvalType.GRIJS, 3, 4) }, Taal.Nederlands);

        var startxref = int.Parse(pdf[(pdf.LastIndexOf("startxref", StringComparison.Ordinal) + 10)..].Split('\n')[0]);
        pdf.Substring(startxref, 4).Should().Be("xref");
        var eerste = pdf.IndexOf("1 0 obj", StringComparison.Ordinal);
        pdf.Should().Contain($"{eerste:D10} 00000 n");
    }

    [Fact]
    public async Task ExporteerAsync_Nederlands_ZouNederlandseTekstenMoetenGebruiken()
    {
        var pdf = await ExporteerAsync(new[] { Moment(AfvalType.GRIJS, 3, 4) }, Taal.Nederlands);

        pdf.Should().Contain("(Restafval)").And.Contain("(Maart)").And.Contain("(Afvalkalender 2026 - 1234AB 10)");
    }

    [Fact]
    public async Task ExporteerAsync_Engels_ZouEngelseTekstenMoetenGebruiken()
    {
        var pdf = await ExporteerAsync(new[] { Moment(AfvalType.GRIJS, 3, 4) }, Taal.Engels);

        pdf.Should().Contain("(General waste)").And.Contain("(March)").And.Contain("(Waste calendar 2026 - 1234AB 10)");
        pdf.Should().NotContain("(Restafval)");
    }

    [Fact]
    public async Task ExporteerAsync_TweeTypes_ZouPerTypeEenLegendaItemMoetenTonen()
    {
        var pdf = await ExporteerAsync(
            new[] { Moment(AfvalType.GRIJS, 3, 4), Moment(AfvalType.GRIJS, 3, 11), Moment(AfvalType.PAPIER, 3, 18) },
            Taal.Nederlands);

        pdf.Should().Contain("(Restafval)").And.Contain("(Papier)");
        pdf.Should().NotContain("(GFT)").And.NotContain("(Kerstboom)");
        pdf.Split("(Restafval)").Length.Should().Be(2);
    }

    [Fact]
    public async Task ExporteerAsync_VerschillendeTypes_ZouVerschillendeKleurenMoetenGebruiken()
    {
        var pdf = await ExporteerAsync(
            new[] { Moment(AfvalType.GRIJS, 3, 4), Moment(AfvalType.GROEN, 3, 5) }, Taal.Nederlands);

        pdf.Should().Contain("0.45 0.45 0.45 rg").And.Contain("0.2 0.65 0.25 rg");
    }

    [Fact]
    public async Task ExporteerAsync_LegeLijst_ZouGeldigeLegePdfMoetenSchrijven()
    {
        var pdf = await ExporteerAsync(Array.Empty<AfvalOphaalMoment>(), Taal.Nederlands);

        pdf.Should().StartWith("%PDF-");
        pdf.Should().Contain("(Januari)").And.Contain("(December)").And.Contain("(Afvalkalender 2026)");
        pdf.Should().NotContain("(Restafval)");
    }

    [Fact]
    public async Task ExporteerAsync_SpecialeTekens_ZouTekstMoetenEscapen()
    {
        var pdf = await ExporteerAsync(new[] { Moment(AfvalType.GRIJS, 3, 4, "1(a)\\b") }, Taal.Nederlands);

        pdf.Should().Contain("1\\(a\\)\\\\b");
        pdf.TrimEnd().Should().EndWith("%%EOF");
    }

    [Fact]
    public void Escape_NietLatin1Teken_ZouVraagtekenMoetenGeven()
    {
        PdfExporter.Escape("a中b").Should().Be("a?b");
    }
}
