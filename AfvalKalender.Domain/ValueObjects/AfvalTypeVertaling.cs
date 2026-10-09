using System;

namespace AfvalKalender.Domain.ValueObjects;

/// <summary>Beschrijving van een ophaalmoment per afvaltype en taal.</summary>
public static class AfvalTypeVertaling
{
    public static string Omschrijving(AfvalType type, Taal taal) => taal switch
    {
        Taal.Engels => type switch
        {
            AfvalType.GRIJS => "General waste is collected",
            AfvalType.GROEN => "Organic waste (GFT) is collected",
            AfvalType.PAPIER => "Paper is collected",
            AfvalType.VERPAKKINGEN => "Plastic and drink cartons are collected",
            AfvalType.KERSTBOOM => "Christmas tree is collected",
            _ => "Waste is collected"
        },
        Taal.Nederlands => type switch
        {
            AfvalType.GRIJS => "Restafval wordt opgehaald",
            AfvalType.GROEN => "GFT afval wordt opgehaald",
            AfvalType.PAPIER => "Oud papier wordt opgehaald",
            AfvalType.VERPAKKINGEN => "Plastic en drinkpakken worden opgehaald",
            AfvalType.KERSTBOOM => "Kerstboom wordt opgehaald",
            _ => "Afval wordt opgehaald"
        },
        _ => throw new ArgumentOutOfRangeException(nameof(taal), taal, null)
    };

    public static string HerinneringVoorvoegsel(Taal taal) =>
        taal == Taal.Engels ? "Reminder:" : "Herinnering:";
}
