using AfvalKalender.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.UnitTests.Domain;

public class AfvalTypeNaamTests
{
    [Theory]
    [InlineData(AfvalType.GRIJS, Taal.Nederlands, "Restafval")]
    [InlineData(AfvalType.GROEN, Taal.Nederlands, "GFT")]
    [InlineData(AfvalType.PAPIER, Taal.Nederlands, "Papier")]
    [InlineData(AfvalType.VERPAKKINGEN, Taal.Nederlands, "Verpakkingen")]
    [InlineData(AfvalType.KERSTBOOM, Taal.Nederlands, "Kerstboom")]
    [InlineData(AfvalType.ONBEKEND, Taal.Nederlands, "Onbekend")]
    [InlineData(AfvalType.GRIJS, Taal.Engels, "General waste")]
    [InlineData(AfvalType.GROEN, Taal.Engels, "Organic waste")]
    [InlineData(AfvalType.PAPIER, Taal.Engels, "Paper")]
    [InlineData(AfvalType.VERPAKKINGEN, Taal.Engels, "Packaging")]
    [InlineData(AfvalType.KERSTBOOM, Taal.Engels, "Christmas tree")]
    [InlineData(AfvalType.ONBEKEND, Taal.Engels, "Unknown")]
    public void Naam_PerTypeEnTaal_ZouJuisteNaamMoetenGeven(AfvalType type, Taal taal, string verwacht)
    {
        AfvalTypeVertaling.Naam(type, taal).Should().Be(verwacht);
    }
}
