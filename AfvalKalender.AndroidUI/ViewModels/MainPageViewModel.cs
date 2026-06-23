using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace AfvalKalender.AndroidUI.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>> _handler;

    public IReadOnlyList<AfvalVerwerker> AfvalVerwerkers { get; } = Domain.ValueObjects.AfvalVerwerkers.Alle;

    [ObservableProperty]
    private AfvalVerwerker _geselecteerdeVerwerker;

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

    public MainPageViewModel(
        ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>> handler)
    {
        _handler = handler;
        _geselecteerdeVerwerker = AfvalVerwerkers[0];
    }

    [RelayCommand]
    private async Task DeelBestandAsync()
    {
        if (string.IsNullOrEmpty(OutputBestandPad)) return;

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Deel Afvalkalender",
            File = new ShareFile(OutputBestandPad)
        });
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
            string postcode = Postcode.ToUpper().Replace(" ", "");
            // In Android, we write to the cache directory for sharing
            string fileName = $"AfvalKalender_{postcode}_{Huisnummer}_{Jaar}.ics";
            
            string cacheDir;
            try { cacheDir = FileSystem.CacheDirectory; }
            catch { cacheDir = Path.GetTempPath(); } // Fallback for unit tests
            
            string fullPath = Path.Combine(cacheDir, fileName);
            
            var command = new VerwerkKalenderCommand(postcode, Huisnummer, Jaar, HerinneringUur, fullPath, GeselecteerdeVerwerker.CompanyCode, false, WebDavUrl, WebDavGebruiker, WebDavWachtwoord);
            var momenten = await _handler.HandleAsync(command);

            OutputBestandPad = fullPath;
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
