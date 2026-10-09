using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AfvalKalender.DesktopUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>> _handler;

    public IReadOnlyList<AfvalVerwerker> AfvalVerwerkers { get; } = Domain.ValueObjects.AfvalVerwerkers.Alle;

    [ObservableProperty]
    private AfvalVerwerker _geselecteerdeVerwerker;

    public IReadOnlyList<Taal> Talen { get; } = new[] { Taal.Nederlands, Taal.Engels };

    [ObservableProperty]
    private Taal _geselecteerdeTaal = TaalExtensies.VanSysteem();

    [ObservableProperty]
    private string _postcode = string.Empty;

    [ObservableProperty]
    private string _huisnummer = string.Empty;

    [ObservableProperty]
    private int _jaar = DateTime.Now.Year;

    [ObservableProperty]
    private int _herinneringUur = 13;

    [ObservableProperty]
    private string _webDavUrl = string.Empty;

    [ObservableProperty]
    private string _webDavGebruiker = string.Empty;

    [ObservableProperty]
    private string _webDavWachtwoord = string.Empty;

    [ObservableProperty]
    private string _statusBericht = "Klaar voor gebruik";

    [ObservableProperty]
    private bool _isBezig = false;

    [ObservableProperty]
    private string _outputBestandPad = string.Empty;

    [ObservableProperty]
    private bool _heeftResultaat = false;

    [ObservableProperty]
    private bool _maakPdf = false;

    [ObservableProperty]
    private string _pdfBestandPad = string.Empty;

    [ObservableProperty]
    private bool _heeftPdf = false;

    public MainWindowViewModel(
        ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>> handler)
    {
        _handler = handler;
        _geselecteerdeVerwerker = AfvalVerwerkers[0];
    }

    // Default constructor for Avalonia previewer
    public MainWindowViewModel()
    {
        _handler = null!;
        _geselecteerdeVerwerker = AfvalVerwerkers[0];
    }

    [RelayCommand]
    private void OpenBestand()
    {
        if (string.IsNullOrEmpty(OutputBestandPad)) return;

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = OutputBestandPad,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            StatusBericht = $"Kon bestand niet openen: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenPdf()
    {
        if (string.IsNullOrEmpty(PdfBestandPad)) return;

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = PdfBestandPad,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            StatusBericht = $"Kon bestand niet openen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task VerwerkAsync()
    {
        if (string.IsNullOrWhiteSpace(Postcode) || string.IsNullOrWhiteSpace(Huisnummer))
        {
            StatusBericht = "Fout: Postcode en huisnummer zijn verplicht.";
            return;
        }

        IsBezig = true;
        HeeftResultaat = false;
        HeeftPdf = false;
        StatusBericht = "Data ophalen...";

        try
        {
            string postcode = Postcode.ToUpper().Replace(" ", "");
            string outputBestand = $"AfvalKalender_{postcode}_{Huisnummer}_{Jaar}.ics";
            string? pdfBestand = MaakPdf ? $"AfvalKalender_{postcode}_{Huisnummer}_{Jaar}.pdf" : null;
            var command = new VerwerkKalenderCommand(postcode, Huisnummer, Jaar, HerinneringUur, outputBestand, GeselecteerdeVerwerker.CompanyCode, false, string.IsNullOrWhiteSpace(WebDavUrl) ? SyncProvider.Geen : SyncProvider.WebDav, WebDavUrl, WebDavGebruiker, WebDavWachtwoord, GeselecteerdeTaal, pdfBestand);
            var momenten = await _handler.HandleAsync(command);

            OutputBestandPad = System.IO.Path.GetFullPath(outputBestand);
            PdfBestandPad = pdfBestand is null ? string.Empty : System.IO.Path.GetFullPath(pdfBestand);
            HeeftPdf = pdfBestand is not null;
            HeeftResultaat = true;
            StatusBericht = $"Succes! {momenten.Count} ophaalmomenten geëxporteerd.";
        }
        catch (Exception ex)
        {
            StatusBericht = $"Fout: {ex.Message}";
        }
        finally
        {
            IsBezig = false;
        }
    }
}
