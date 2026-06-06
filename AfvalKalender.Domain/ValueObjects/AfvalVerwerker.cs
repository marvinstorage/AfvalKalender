using System.Collections.Generic;

namespace AfvalKalender.Domain.ValueObjects;

public record AfvalVerwerker(string Id, string Naam, string CompanyCode);

public static class AfvalVerwerkers
{
    public static readonly IReadOnlyList<AfvalVerwerker> Alle = new List<AfvalVerwerker>
    {
        new("twentemilieu", "Twente Milieu", "8d97bb56-5afd-4cbc-a651-b4f7314264b4"),
        new("acv", "ACV", "f8e2844a-095e-48f9-9f98-71fceb51d2c3"),
        new("almere", "Almere", "53d8db94-7945-42fd-9742-9bbc71dbe4c1"),
        new("areareiniging", "Area Reiniging", "adc418da-d19b-11e5-ab30-625662870761"),
        new("avalex", "Avalex", "f7a74ad1-fdbf-4a43-9f91-44644f4d4222"),
        new("avri", "Avri", "78cd4156-394b-413d-8936-d407e334559a"),
        new("blink", "Blink", "252d30d0-2e74-469c-8f1e-c0e2e434eb58"),
        new("hellendoorn", "Hellendoorn", "24434f5b-7244-412b-9306-3a2bd1e22bc1"),
        new("meerlanden", "Meerlanden", "800bf8d7-6dd1-4490-ba9d-b419d6dc8a45"),
        new("oostzaan", "Oostzaan", "6eb81e8f-ca5a-4bad-af0a-667650325511"),
        new("rad", "RAD", "13a2cad9-36d0-4b01-b877-efcb421a864d"),
        new("venlo", "Venlo", "280affe9-1428-443b-895a-b90431b8ca31"),
        new("waardlanden", "Waardlanden", "942abcf6-3775-400d-ae5d-7380d728b23c"),
        new("westland", "Westland", "6fc75608-126a-4a50-9241-a002ce8c8a6c"),
        new("woerden", "Woerden", "06856f74-6826-4c6a-aabf-69bc9d20b5a6"),
        new("ximmio", "Ximmio (Algemeen)", "800bf8d7-6dd1-4490-ba9d-b419d6dc8a45")
    }.AsReadOnly();
}
