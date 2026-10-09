using System.Globalization;
using AfvalKalender.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.UnitTests.Domain;

public class TaalTests
{
    [Theory]
    [InlineData("en-GB", Taal.Engels)]
    [InlineData("en-US", Taal.Engels)]
    [InlineData("nl-NL", Taal.Nederlands)]
    [InlineData("de-DE", Taal.Nederlands)]
    public void VanCultuur_MetCultuur_ZouJuisteTaalMoetenGeven(string cultuur, Taal verwacht)
    {
        TaalExtensies.VanCultuur(new CultureInfo(cultuur)).Should().Be(verwacht);
    }

    [Theory]
    [InlineData(AfvalType.GRIJS, "Restafval wordt opgehaald")]
    [InlineData(AfvalType.GROEN, "GFT afval wordt opgehaald")]
    [InlineData(AfvalType.PAPIER, "Oud papier wordt opgehaald")]
    [InlineData(AfvalType.VERPAKKINGEN, "Plastic en drinkpakken worden opgehaald")]
    [InlineData(AfvalType.KERSTBOOM, "Kerstboom wordt opgehaald")]
    [InlineData(AfvalType.ONBEKEND, "Afval wordt opgehaald")]
    public void Omschrijving_Nederlands_ZouOngewijzigdeTekstMoetenGeven(AfvalType type, string verwacht)
    {
        AfvalTypeVertaling.Omschrijving(type, Taal.Nederlands).Should().Be(verwacht);
    }

    [Theory]
    [InlineData(AfvalType.GRIJS, "General waste is collected")]
    [InlineData(AfvalType.GROEN, "Organic waste (GFT) is collected")]
    [InlineData(AfvalType.PAPIER, "Paper is collected")]
    [InlineData(AfvalType.VERPAKKINGEN, "Plastic and drink cartons are collected")]
    [InlineData(AfvalType.KERSTBOOM, "Christmas tree is collected")]
    [InlineData(AfvalType.ONBEKEND, "Waste is collected")]
    public void Omschrijving_Engels_ZouVertaaldeTekstMoetenGeven(AfvalType type, string verwacht)
    {
        AfvalTypeVertaling.Omschrijving(type, Taal.Engels).Should().Be(verwacht);
    }

    [Fact]
    public void HerinneringVoorvoegsel_PerTaal_ZouJuisteTekstMoetenGeven()
    {
        AfvalTypeVertaling.HerinneringVoorvoegsel(Taal.Nederlands).Should().Be("Herinnering:");
        AfvalTypeVertaling.HerinneringVoorvoegsel(Taal.Engels).Should().Be("Reminder:");
    }
}
