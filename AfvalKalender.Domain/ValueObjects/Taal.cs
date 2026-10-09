using System.Globalization;

namespace AfvalKalender.Domain.ValueObjects;

public enum Taal
{
    Nederlands,
    Engels
}

public static class TaalExtensies
{
    /// <summary>Engels voor een Engelse UI-taal, anders Nederlands.</summary>
    public static Taal VanCultuur(CultureInfo cultuur) =>
        cultuur.TwoLetterISOLanguageName == "en" ? Taal.Engels : Taal.Nederlands;

    public static Taal VanSysteem() => VanCultuur(CultureInfo.CurrentUICulture);
}
