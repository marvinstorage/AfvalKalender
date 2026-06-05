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
    private async Task VerwerkAsync()
    {
        if (string.IsNullOrWhiteSpace(Postcode) || string.IsNullOrWhiteSpace(Huisnummer))
        {
            StatusBericht = "Fout: Postcode en huisnummer zijn verplicht.";
            return;
        }

        IsBezig = true;
        StatusBericht = "Data ophalen...";

        try
        {
            string outputBestand = $"AfvalKalender_{Postcode.ToUpper()}_{Huisnummer}_{Jaar}.ics";
            var momenten = await _afvalService.VerwerkKalenderAsync(Postcode.ToUpper(), Huisnummer, Jaar, HerinneringUur, outputBestand);
            
            StatusBericht = $"Succes! {momenten.Count()} ophaalmomenten geëxporteerd naar {outputBestand}";
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
