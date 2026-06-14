using System.Net.Http.Headers;
using System.Text;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AfvalKalender.Infrastructure.Api;

public class TwenteMilieuApi : IAfvalApi
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://wasteapi.ximmio.com/api/";

    public TwenteMilieuApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
    }

    public async Task<string> HaalUniekAdresIdOpAsync(string postcode, string huisnummer, string companyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4", bool forceerVernieuwen = false)
    {
        var requestBody = new
        {
            companyCode = companyCode,
            postCode = postcode,
            houseNumber = huisnummer
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(BaseUrl + "FetchAdress", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<JObject>(json);
        var dataList = data?["dataList"] as JArray;

        var uniekId = dataList?.Count > 0 ? dataList[0]?["UniqueId"]?.ToString() : null;

        if (string.IsNullOrEmpty(uniekId))
            throw new Exception("Kon geen uniek adres ID vinden voor dit adres. Controleer of de geselecteerde afvalverwerker uw adres bedient.");

        return uniekId;
    }

    public async Task<IEnumerable<AfvalOphaalMoment>> HaalKalenderOpAsync(string uniekAdresId, string postcode, string huisnummer, int jaar, string companyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4", bool forceerVernieuwen = false)
    {
        var requestBody = new
        {
            companyCode = companyCode,
            uniqueAddressID = uniekAdresId,
            startDate = $"{jaar}-01-01",
            endDate = $"{jaar}-12-31"
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(BaseUrl + "GetCalendar", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<JObject>(json);
        var dataList = data?["dataList"] as JArray;

        var momenten = new List<AfvalOphaalMoment>();

        if (dataList != null)
        {
            foreach (var item in dataList)
            {
                var typeStr = item["_pickupTypeText"]?.ToString().Trim();
                var dates = item["pickupDates"] as JArray;
                var description = item["description"]?.ToString();

                if (string.IsNullOrEmpty(typeStr) || dates == null) continue;

                var type = MapAfvalType(typeStr);
                var omschrijving = MapOmschrijving(type);

                foreach (var dateToken in dates)
                {
                    if (DateTime.TryParse(dateToken.ToString(), out var datum))
                    {
                        momenten.Add(new AfvalOphaalMoment(type, datum, omschrijving, postcode, huisnummer));
                    }
                }
            }
        }

        return momenten;
    }

    private string MapOmschrijving(AfvalType type)
    {
        return type switch
        {
            AfvalType.GRIJS => "Restafval wordt opgehaald",
            AfvalType.GROEN => "GFT afval wordt opgehaald",
            AfvalType.PAPIER => "Oud papier wordt opgehaald",
            AfvalType.VERPAKKINGEN => "Plastic en drinkpakken worden opgehaald",
            AfvalType.KERSTBOOM => "Kerstboom wordt opgehaald",
            _ => "Afval wordt opgehaald"
        };
    }

    private AfvalType MapAfvalType(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "GREY" => AfvalType.GRIJS,
            "GREEN" => AfvalType.GROEN,
            "PAPER" => AfvalType.PAPIER,
            "PACKAGES" => AfvalType.VERPAKKINGEN,
            "TREE" => AfvalType.KERSTBOOM,
            _ => AfvalType.ONBEKEND
        };
    }
}
