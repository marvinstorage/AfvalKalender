using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AfvalKalender.Infrastructure.Sync;

public class WebDavSyncAdapter : IAfvalKalenderSynchronisator
{
    private readonly HttpClient _httpClient;
    private readonly IIcsExporter _icsExporter;

    public WebDavSyncAdapter(HttpClient httpClient, IIcsExporter icsExporter)
    {
        _httpClient = httpClient;
        _icsExporter = icsExporter;
    }

    public bool Ondersteunt(SyncProvider provider) => provider == SyncProvider.WebDav;

    public async Task SynchroniseerAsync(
        IEnumerable<AfvalOphaalMoment> momenten, 
        SyncConfiguratie configuratie, 
        int herinneringUur)
    {
        if (string.IsNullOrWhiteSpace(configuratie.DoelUrlOfToken))
            throw new ArgumentException("WebDAV URL mag niet leeg zijn.", nameof(configuratie.DoelUrlOfToken));

        // Genereer tijdelijk bestand om de ICS content te krijgen
        var tijdelijkBestand = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ics");
        try
        {
            await _icsExporter.ExporteerAsync(momenten, tijdelijkBestand, herinneringUur);
            var icsInhoud = await File.ReadAllTextAsync(tijdelijkBestand);

            using var request = new HttpRequestMessage(HttpMethod.Put, configuratie.DoelUrlOfToken);
            
            // Stel Basic Authentication in
            if (!string.IsNullOrEmpty(configuratie.Gebruikersnaam))
            {
                var authBytes = Encoding.UTF8.GetBytes($"{configuratie.Gebruikersnaam}:{configuratie.Wachtwoord}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
            }
            
            request.Content = new StringContent(icsInhoud, Encoding.UTF8, "text/calendar");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            if (File.Exists(tijdelijkBestand))
                File.Delete(tijdelijkBestand);
        }
    }
}
