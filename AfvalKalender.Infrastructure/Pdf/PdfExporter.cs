using System.Globalization;
using System.Text;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Infrastructure.Pdf;

/// <summary>
/// Schrijft het jaaroverzicht als A4-PDF zonder externe bibliotheek (ADR-012):
/// standaard Helvetica, gevulde vlakken en tekst in een enkele content stream.
/// </summary>
public class PdfExporter : IPdfExporter
{
    private const double PaginaBreedte = 595;
    private const double PaginaHoogte = 842;
    private const double Marge = 30;
    private const int Kolommen = 3;
    private const double Tussenruimte = 8;
    private const double CelHoogte = 19;

    private static readonly string[] MaandenNl =
        { "Januari", "Februari", "Maart", "April", "Mei", "Juni", "Juli", "Augustus", "September", "Oktober", "November", "December" };
    private static readonly string[] MaandenEn =
        { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    private static readonly string[] DagenNl = { "M", "D", "W", "D", "V", "Z", "Z" };
    private static readonly string[] DagenEn = { "M", "T", "W", "T", "F", "S", "S" };

    private static (double R, double G, double B) Kleur(AfvalType type) => type switch
    {
        AfvalType.GRIJS => (0.45, 0.45, 0.45),
        AfvalType.GROEN => (0.20, 0.65, 0.25),
        AfvalType.PAPIER => (0.20, 0.45, 0.85),
        AfvalType.VERPAKKINGEN => (0.93, 0.50, 0.08),
        AfvalType.KERSTBOOM => (0.05, 0.35, 0.18),
        _ => (0.65, 0.65, 0.65)
    };

    public async Task ExporteerAsync(IEnumerable<AfvalOphaalMoment> momenten, string bestandspad, int jaar, Taal taal)
    {
        var lijst = momenten.Where(m => m.Datum.Year == jaar).ToList();
        var bytes = Bouw(lijst, jaar, taal);

        var map = Path.GetDirectoryName(Path.GetFullPath(bestandspad));
        if (!string.IsNullOrEmpty(map)) Directory.CreateDirectory(map);
        await File.WriteAllBytesAsync(bestandspad, bytes);
    }

    internal static byte[] Bouw(IReadOnlyList<AfvalOphaalMoment> momenten, int jaar, Taal taal)
    {
        var inhoud = BouwInhoud(momenten, jaar, taal);
        var latin1 = Encoding.Latin1;

        var objecten = new List<byte[]>
        {
            latin1.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"),
            latin1.GetBytes("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"),
            latin1.GetBytes(string.Create(CultureInfo.InvariantCulture,
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PaginaBreedte} {PaginaHoogte}] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>")),
            latin1.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
            latin1.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"),
        };
        var stream = latin1.GetBytes(inhoud);
        var contentObject = new List<byte>();
        contentObject.AddRange(latin1.GetBytes($"<< /Length {stream.Length} >>\nstream\n"));
        contentObject.AddRange(stream);
        contentObject.AddRange(latin1.GetBytes("\nendstream"));
        objecten.Add(contentObject.ToArray());

        using var ms = new MemoryStream();
        void Schrijf(string s) { var b = latin1.GetBytes(s); ms.Write(b, 0, b.Length); }

        Schrijf("%PDF-1.4\n");
        var offsets = new List<long>();
        for (var i = 0; i < objecten.Count; i++)
        {
            offsets.Add(ms.Position);
            Schrijf($"{i + 1} 0 obj\n");
            ms.Write(objecten[i], 0, objecten[i].Length);
            Schrijf("\nendobj\n");
        }

        var xref = ms.Position;
        Schrijf($"xref\n0 {objecten.Count + 1}\n0000000000 65535 f \n");
        foreach (var o in offsets) Schrijf($"{o:D10} 00000 n \n");
        Schrijf($"trailer\n<< /Size {objecten.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return ms.ToArray();
    }

    private static string BouwInhoud(IReadOnlyList<AfvalOphaalMoment> momenten, int jaar, Taal taal)
    {
        var maanden = taal == Taal.Engels ? MaandenEn : MaandenNl;
        var dagen = taal == Taal.Engels ? DagenEn : DagenNl;
        var perDag = momenten
            .GroupBy(m => m.Datum.Date)
            .ToDictionary(g => g.Key, g => g.Select(m => m.Type).Distinct().OrderBy(t => t).ToList());

        var sb = new StringBuilder();
        var inv = CultureInfo.InvariantCulture;

        var adres = momenten.FirstOrDefault();
        var titel = (taal == Taal.Engels ? "Waste calendar " : "Afvalkalender ") + jaar;
        if (adres != null) titel += $" - {adres.Postcode} {adres.Huisnummer}";
        Tekst(sb, titel, Marge, PaginaHoogte - Marge - 16, 18, true, 0, 0, 0);

        var rasterBovenkant = PaginaHoogte - Marge - 40;
        var blokBreedte = (PaginaBreedte - 2 * Marge - (Kolommen - 1) * Tussenruimte) / Kolommen;
        var celBreedte = blokBreedte / 7;
        var blokHoogte = 14 + 12 + 6 * CelHoogte + 12;

        for (var m = 0; m < 12; m++)
        {
            var kolom = m % Kolommen;
            var rij = m / Kolommen;
            var x0 = Marge + kolom * (blokBreedte + Tussenruimte);
            var yBoven = rasterBovenkant - rij * blokHoogte;

            Tekst(sb, maanden[m], x0, yBoven - 11, 11, true, 0, 0, 0);
            for (var d = 0; d < 7; d++)
                Tekst(sb, dagen[d], x0 + d * celBreedte + celBreedte / 2 - 2.5, yBoven - 23, 7, false, 0.4, 0.4, 0.4);

            var eerste = new DateTime(jaar, m + 1, 1);
            var offset = ((int)eerste.DayOfWeek + 6) % 7; // maandag = 0
            var dagenInMaand = DateTime.DaysInMonth(jaar, m + 1);
            var rasterY = yBoven - 26;

            for (var dag = 1; dag <= dagenInMaand; dag++)
            {
                var index = offset + dag - 1;
                var cx = x0 + (index % 7) * celBreedte;
                var cy = rasterY - (index / 7 + 1) * CelHoogte;
                var datum = new DateTime(jaar, m + 1, dag);
                var gemarkeerd = perDag.TryGetValue(datum, out var types);

                if (gemarkeerd)
                {
                    var deel = (celBreedte - 1) / types!.Count;
                    for (var t = 0; t < types.Count; t++)
                    {
                        var (r, g, b) = Kleur(types[t]);
                        sb.Append(inv, $"{r:0.##} {g:0.##} {b:0.##} rg {cx + 0.5 + t * deel:0.##} {cy + 0.5:0.##} {deel:0.##} {CelHoogte - 1:0.##} re f\n");
                    }
                }

                var tekst = dag.ToString(inv);
                var breedte = tekst.Length * 0.556 * 8;
                if (gemarkeerd) Tekst(sb, tekst, cx + celBreedte / 2 - breedte / 2, cy + 6, 8, true, 1, 1, 1);
                else Tekst(sb, tekst, cx + celBreedte / 2 - breedte / 2, cy + 6, 8, false, 0, 0, 0);
            }

            sb.Append(inv, $"0.7 0.7 0.7 RG 0.5 w {x0:0.##} {rasterY:0.##} m {x0 + blokBreedte:0.##} {rasterY:0.##} l S\n");
        }

        var legendaY = Marge + 6;
        var legendaX = Marge;
        var voorkomend = perDag.Values.SelectMany(t => t).Distinct().OrderBy(t => t).ToList();
        foreach (var type in voorkomend)
        {
            var (r, g, b) = Kleur(type);
            var naam = AfvalTypeVertaling.Naam(type, taal);
            sb.Append(inv, $"{r:0.##} {g:0.##} {b:0.##} rg {legendaX:0.##} {legendaY:0.##} 10 10 re f\n");
            Tekst(sb, naam, legendaX + 14, legendaY + 2, 9, false, 0, 0, 0);
            legendaX += 14 + naam.Length * 0.52 * 9 + 18;
        }

        return sb.ToString();
    }

    private static void Tekst(StringBuilder sb, string tekst, double x, double y, double grootte, bool vet, double r, double g, double b)
    {
        var inv = CultureInfo.InvariantCulture;
        sb.Append(inv, $"{r:0.##} {g:0.##} {b:0.##} rg BT /{(vet ? "F2" : "F1")} {grootte:0.##} Tf {x:0.##} {y:0.##} Td ({Escape(tekst)}) Tj ET\n");
    }

    /// <summary>Maakt tekst veilig als PDF-literal: ( ) \ geescaped, niet-Latin-1 vervangen door een vraagteken.</summary>
    public static string Escape(string tekst)
    {
        var sb = new StringBuilder(tekst.Length);
        foreach (var c in tekst)
        {
            if (c is '(' or ')' or '\\') sb.Append('\\').Append(c);
            else if (c < 32 || c > 255) sb.Append('?');
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
