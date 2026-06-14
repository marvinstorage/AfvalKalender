using System.Text.Json;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Infrastructure.Cache;

public class CacherendeAfvalApi : IAfvalApi
{
    private readonly IAfvalApi _innerApi;
    private readonly string _cachePad;
    private readonly Func<DateTime> _klok;
    private static readonly TimeSpan CacheDuur = TimeSpan.FromHours(24);

    public CacherendeAfvalApi(IAfvalApi innerApi, string cachePad, Func<DateTime>? klok = null)
    {
        _innerApi = innerApi;
        _cachePad = cachePad;
        _klok = klok ?? (() => DateTime.UtcNow);
        Directory.CreateDirectory(cachePad);
    }

    public async Task<string> HaalUniekAdresIdOpAsync(string postcode, string huisnummer, string companyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4", bool forceerVernieuwen = false)
    {
        var bestand = CacheBestand($"adresid_{companyCode}_{postcode}_{huisnummer}.json");
        if (!forceerVernieuwen)
        {
            var gecached = LeesUitCache<string>(bestand);
            if (gecached is not null) return gecached;
        }

        var uniekId = await _innerApi.HaalUniekAdresIdOpAsync(postcode, huisnummer, companyCode, forceerVernieuwen);
        SchrijfNaarCache(bestand, uniekId);
        return uniekId;
    }

    public async Task<IEnumerable<AfvalOphaalMoment>> HaalKalenderOpAsync(
        string uniekAdresId, string postcode, string huisnummer, int jaar, string companyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4", bool forceerVernieuwen = false)
    {
        var bestand = CacheBestand($"kalender_{companyCode}_{postcode}_{huisnummer}_{jaar}.json");
        if (!forceerVernieuwen)
        {
            var gecached = LeesUitCache<List<AfvalMomentDto>>(bestand);
            if (gecached is not null)
                return gecached.Select(DtoNaarMoment);
        }

        var momenten = (await _innerApi.HaalKalenderOpAsync(uniekAdresId, postcode, huisnummer, jaar, companyCode, forceerVernieuwen)).ToList();
        SchrijfNaarCache(bestand, momenten.Select(MomentNaarDto).ToList());
        return momenten;
    }

    private string CacheBestand(string bestandsnaam) => Path.Combine(_cachePad, bestandsnaam);

    private T? LeesUitCache<T>(string pad) where T : class
    {
        if (!File.Exists(pad)) return null;
        try
        {
            var json = File.ReadAllText(pad);
            var envelop = JsonSerializer.Deserialize<CacheEnvelop<T>>(json);
            if (envelop is null || _klok() - envelop.OpgeslagenOp > CacheDuur) return null;
            return envelop.Inhoud;
        }
        catch
        {
            return null;
        }
    }

    private void SchrijfNaarCache<T>(string pad, T inhoud)
    {
        try
        {
            var envelop = new CacheEnvelop<T>(_klok(), inhoud);
            File.WriteAllText(pad, JsonSerializer.Serialize(envelop));
        }
        catch
        {
            // Cache schrijven is best-effort; mislukken mag de gebruiker niet blokkeren
        }
    }

    private static AfvalOphaalMoment DtoNaarMoment(AfvalMomentDto dto)
    {
        var type = Enum.TryParse<AfvalType>(dto.Type, out var parsed) ? parsed : AfvalType.ONBEKEND;
        return new AfvalOphaalMoment(type, dto.Datum, dto.Omschrijving, dto.Postcode, dto.Huisnummer);
    }

    private static AfvalMomentDto MomentNaarDto(AfvalOphaalMoment m) =>
        new(m.Type.ToString(), m.Datum, m.Omschrijving, m.Postcode, m.Huisnummer);
}

internal record CacheEnvelop<T>(DateTime OpgeslagenOp, T Inhoud);

internal record AfvalMomentDto(
    string Type,
    DateTime Datum,
    string Omschrijving,
    string Postcode,
    string Huisnummer);
