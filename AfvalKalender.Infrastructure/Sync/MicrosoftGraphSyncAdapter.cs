using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Infrastructure.Sync;

public class MicrosoftGraphSyncAdapter : IAfvalKalenderSynchronisator
{
    private readonly HttpClient _httpClient;

    public MicrosoftGraphSyncAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool Ondersteunt(SyncProvider provider) => provider == SyncProvider.MicrosoftGraph;

    public async Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuratie.DoelUrlOfToken);
        // Note: Implementation omitted for demonstration. Here we would map `momenten` to Microsoft Graph JSON
        // and POST to https://graph.microsoft.com/v1.0/me/events
        await Task.CompletedTask;
    }
}
