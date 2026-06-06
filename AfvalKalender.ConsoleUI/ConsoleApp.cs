using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Entities;

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
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"
    _____________________________________________________
   /                                                     \
  |                                                       |
  |     A F V A L   K A L E N D E R   T W E N T E         |
  |                                                       |
  |                  _,,/|                                |
  |                 ""-f  |                                |
  |                   \  |   [ TWENTSE ROS ]              |
  |                    \ |                                |
  |                     \|                                |
  |                                                       |
  |          Schoon Twente, Mooi Twente!                  |
   \_____________________________________________________/
        ");
        Console.ResetColor();

        Console.WriteLine("--- Afvalkalender naar ICS Exporteur ---");

        Console.Write("Voer uw postcode in (bijv. 1234AB): ");
        string postcode = Console.ReadLine()?.ToUpper() ?? "";

        Console.Write("Voer uw huisnummer in: ");
        string huisnummer = Console.ReadLine() ?? "";

        Console.Write("Voor welk jaar wilt u de kalender? (bijv. 2026): ");
        if (!int.TryParse(Console.ReadLine(), out int jaar)) jaar = DateTime.Now.Year;

        Console.Write("Hoeveel uur van tevoren wilt u een herinnering? (bijv. 13): ");
        if (!int.TryParse(Console.ReadLine(), out int herinneringUur)) herinneringUur = 13;

        string outputBestand = $"AfvalKalender_{postcode}_{huisnummer}_{jaar}.ics";

        try
        {
            Console.WriteLine("\nBezig met ophalen en verwerken van data...");
            var command = new VerwerkKalenderCommand(postcode, huisnummer, jaar, herinneringUur, outputBestand);
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
