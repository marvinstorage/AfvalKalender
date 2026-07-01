using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using Spectre.Console;
using System.Text.RegularExpressions;

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
        // Header
        AnsiConsole.Write(
            new FigletText("Afvalkalender")
                .Centered()
                .Color(Color.Green));
        
        AnsiConsole.MarkupLine("[bold green]Schoon en Duurzaam Nederland![/]");
        AnsiConsole.WriteLine();

        // Verwerker Selectie
        var verwerkers = AfvalVerwerkers.Alle;
        var providerPrompt = new SelectionPrompt<AfvalVerwerker>()
            .Title("Selecteer uw [green]afvalverwerker[/]:")
            .PageSize(10)
            .MoreChoicesText("[grey](Beweeg omhoog en omlaag om meer te zien)[/]")
            .UseConverter(v => v.Naam);

        foreach (var v in verwerkers)
        {
            providerPrompt.AddChoice(v);
        }

        var geselecteerdeVerwerker = AnsiConsole.Prompt(providerPrompt);
        AnsiConsole.MarkupLine($"Geselecteerd: [bold cyan]{geselecteerdeVerwerker.Naam}[/]\n");

        // Input
        string postcode = AnsiConsole.Ask<string>("Voer uw [green]postcode[/] in (bijv. 1234AB):")
                                     .Replace(" ", "").ToUpper();

        string huisnummer = AnsiConsole.Ask<string>("Voer uw [green]huisnummer[/] in:");

        int jaar = AnsiConsole.Prompt(
            new TextPrompt<int>("Voor welk [green]jaar[/] wilt u de kalender?")
                .DefaultValue(DateTime.Now.Year)
                .ValidationErrorMessage("[red]Voer een geldig jaar in.[/]"));

        int herinneringUur = AnsiConsole.Prompt(
            new TextPrompt<int>("Hoeveel uur van tevoren wilt u een [green]herinnering[/]?")
                .DefaultValue(13)
                .ValidationErrorMessage("[red]Voer een geldig getal in.[/]"));

        // WebDAV Sync
        bool gebruikWebDav = AnsiConsole.Confirm("Wilt u [green]WebDAV / CalDAV synchronisatie[/] configureren?", false);
        string webDavUrl = "";
        string webDavGebruiker = "";
        string webDavWachtwoord = "";

        if (gebruikWebDav)
        {
            webDavUrl = AnsiConsole.Ask<string>("WebDAV [green]URL[/]:");
            webDavGebruiker = AnsiConsole.Ask<string>("WebDAV [green]Gebruikersnaam[/]:");
            webDavWachtwoord = AnsiConsole.Prompt(
                new TextPrompt<string>("WebDAV [green]Wachtwoord[/]:")
                    .Secret());
        }

        string outputBestand = $"AfvalKalender_{postcode}_{huisnummer}_{jaar}.ics";

        try
        {
            IReadOnlyList<AfvalOphaalMoment> momenten = new List<AfvalOphaalMoment>();

            await AnsiConsole.Status()
                .StartAsync("Bezig met ophalen en verwerken van data...", async ctx => 
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    var command = new VerwerkKalenderCommand(
                        postcode, 
                        huisnummer, 
                        jaar, 
                        herinneringUur, 
                        outputBestand, 
                        geselecteerdeVerwerker.CompanyCode, 
                        false, 
                        gebruikWebDav ? SyncProvider.WebDav : SyncProvider.Geen, 
                        webDavUrl, 
                        webDavGebruiker, 
                        webDavWachtwoord);
                        
                    momenten = await _handler.HandleAsync(command);
                });

            string absolutePath = Path.GetFullPath(outputBestand);
            AnsiConsole.MarkupLine($"\n[bold green]Succes![/] Er zijn [yellow]{momenten.Count}[/] ophaalmomenten gevonden en opgeslagen.");
            
            AnsiConsole.MarkupLine($"Het ICS bestand is aangemaakt: [link={absolutePath}]{absolutePath}[/]");

            // Toon een tabel met de eerstvolgende ophaalmomenten
            if (momenten.Any())
            {
                AnsiConsole.WriteLine();
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn("[bold]Datum[/]");
                table.AddColumn("[bold]Afvaltype[/]");

                // Toon de komende 10 momenten, of minder
                var komendeMomenten = momenten
                    .Where(m => m.Datum >= DateTime.Today.AddDays(-1)) // Inclusief gisteren voor context
                    .OrderBy(m => m.Datum)
                    .Take(10);

                foreach (var m in komendeMomenten)
                {
                    string typeNaam = m.Type.ToString();
                    string color = typeNaam.ToLower() switch
                    {
                        "groen" => "green",
                        "grijs" => "grey",
                        "papier" => "blue",
                        "verpakkingen" => "yellow",
                        "kerstboom" => "green",
                        _ => "white"
                    };

                    table.AddRow($"[bold]{m.Datum:dd-MM-yyyy}[/]", $"[{color}]{typeNaam}[/]");
                }

                AnsiConsole.Write(
                    new Panel(table)
                        .Header("Komende Ophaalmomenten (Max 10)")
                        .Expand());
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"\n[bold red]Er is een fout opgetreden:[/] {ex.Message}");
        }

        AnsiConsole.MarkupLine("\n[grey]Afsluiten...[/]");
    }
}
