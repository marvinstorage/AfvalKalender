using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfvalKalender.Application.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace AfvalKalender.DesktopUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AfvalService _afvalService;

    [ObservableProperty]
    private string _postcode = string.Empty;

    [ObservableProperty]
    private string _huisnummer = string.Empty;

    [ObservableProperty]
    private int _jaar = DateTime.Now.Year;

    [ObservableProperty]
    private int _herinneringUur = 13;

    [ObservableProperty]
    private string _statusBericht = "Klaar voor gebruik";

    [ObservableProperty]
    private bool _isBezig = false;

    [ObservableProperty]
    private string _outputBestandPad = string.Empty;

    [ObservableProperty]
    private bool _heeftResultaat = false;

    public MainWindowViewModel(AfvalService afvalService)
    {
        _afvalService = afvalService;
    }

    // Default constructor for previewer
    public MainWindowViewModel()
    {
        _afvalService = null!;
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
    private async Task VerwerkAsync()
    {
        if (string.IsNullOrWhiteSpace(Postcode) || string.IsNullOrWhiteSpace(Huisnummer))
        {
            StatusBericht = "Fout: Postcode en huisnummer zijn verplicht.";
            return;
        }

        IsBezig = true;
        HeeftResultaat = false;
        StatusBericht = "Data ophalen...";

        try
        {
            string outputBestand = $"AfvalKalender_{Postcode.ToUpper()}_{Huisnummer}_{Jaar}.ics";
            var momenten = await _afvalService.VerwerkKalenderAsync(Postcode.ToUpper(), Huisnummer, Jaar, HerinneringUur, outputBestand);
            
            OutputBestandPad = System.IO.Path.GetFullPath(outputBestand);
            HeeftResultaat = true;
            StatusBericht = $"Succes! {momenten.Count()} ophaalmomenten geëxporteerd.";
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
