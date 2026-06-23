using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.ConsoleUI;

public class ConsoleApp
{
    private readonly ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>> _handler;

    public ConsoleApp(ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>> handler)
    {
        _handler = handler;
    }

    public async Task RunAsync()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(@"
    _____________________________________________________
   /                                                     \
  |                                                       |
  |         A F V A L   K A L E N D E R                   |
  |               E X P O R T E U R                       |
  |                                                       |
  |                  _,,/|                                |
  |                 ""-f  |                                |
  |                   \  |   [ GENERIC XIMMIO ]           |
  |                    \ |                                |
  |                     \|                                |
  |                                                       |
  |         Schoon en Duurzaam Nederland!                 |
   \_____________________________________________________/
        ");
        Console.ResetColor();

        Console.WriteLine("--- Afvalkalender naar ICS Exporteur ---");

        Console.WriteLine("\nSelecteer uw afvalverwerker:");
        var verwerkers = AfvalVerwerkers.Alle;
        for (int i = 0; i < verwerkers.Count; i++)
        {
            Console.WriteLine($"{i + 1,2}. {verwerkers[i].Naam}");
        }
        Console.Write($"Kies afvalverwerker (1-{verwerkers.Count}) [1]: ");
        string keuzeInput = Console.ReadLine() ?? "";
        if (!int.TryParse(keuzeInput, out int keuzeIndex) || keuzeIndex < 1 || keuzeIndex > verwerkers.Count)
        {
            keuzeIndex = 1;
        }
        var geselecteerdeVerwerker = verwerkers[keuzeIndex - 1];
        Console.WriteLine($"Geselecteerd: {geselecteerdeVerwerker.Naam}\n");

        Console.Write("Voer uw postcode in (bijv. 1234AB): ");
        string postcode = (Console.ReadLine()?.ToUpper() ?? "").Replace(" ", "");

        Console.Write("Voer uw huisnummer in: ");
        string huisnummer = Console.ReadLine() ?? "";

        Console.Write("Voor welk jaar wilt u de kalender? (bijv. 2026): ");
        if (!int.TryParse(Console.ReadLine(), out int jaar)) jaar = DateTime.Now.Year;

        Console.Write("Hoeveel uur van tevoren wilt u een herinnering? (bijv. 13): ");
        if (!int.TryParse(Console.ReadLine(), out int herinneringUur)) herinneringUur = 13;

        Console.Write("\nOptioneel: WebDAV / CalDAV synchronisatie\n");
        Console.Write("WebDAV URL (laat leeg om over te slaan): ");
        string webDavUrl = Console.ReadLine() ?? "";
        
        string webDavGebruiker = "";
        string webDavWachtwoord = "";
        if (!string.IsNullOrWhiteSpace(webDavUrl))
        {
            Console.Write("Gebruikersnaam: ");
            webDavGebruiker = Console.ReadLine() ?? "";
            Console.Write("Wachtwoord: ");
            webDavWachtwoord = Console.ReadLine() ?? "";
        }

        string outputBestand = $"AfvalKalender_{postcode}_{huisnummer}_{jaar}.ics";

        try
        {
            Console.WriteLine("\nBezig met ophalen en verwerken van data...");
            var command = new VerwerkKalenderCommand(postcode, huisnummer, jaar, herinneringUur, outputBestand, geselecteerdeVerwerker.CompanyCode, false, webDavUrl, webDavGebruiker, webDavWachtwoord);
            var momenten = await _handler.HandleAsync(command);

            string absolutePath = Path.GetFullPath(outputBestand);
            Console.WriteLine($"\nSucces! Er zijn {momenten.Count} ophaalmomenten gevonden en opgeslagen.");
            Console.WriteLine("Het ICS bestand is aangemaakt:");

            Console.Write("\x1b]8;;file://");
            Console.Write(absolutePath);
            Console.Write("\x1b\\");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(absolutePath);
            Console.ResetColor();
            Console.WriteLine("\x1b]8;;\x1b\\");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nEr is een fout opgetreden: {ex.Message}");
        }

        Console.WriteLine("\nAfsluiten...");
    }
}
