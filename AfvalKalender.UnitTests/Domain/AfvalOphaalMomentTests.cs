using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.UnitTests.Domain;

public class AfvalOphaalMomentTests
{
    [Fact]
    public void Update_MetNieuweOmschrijving_ZouOmschrijvingEnDatumMoetenBijwerken()
    {
        // Arrange
        var moment = new AfvalOphaalMoment(AfvalType.GRIJS, DateTime.Now, "Oud", "1234AB", "10");
        var oudeDatum = moment.LaatstGewijzigd;
        Thread.Sleep(10); // Zorg voor tijdsverschil

        // Act
        moment.Update("Nieuw");

        // Assert
        moment.Omschrijving.Should().Be("Nieuw");
        moment.LaatstGewijzigd.Should().BeAfter(oudeDatum);
    }

    [Fact]
    public void Update_MetZelfdeOmschrijving_ZouNietsMoetenVeranderen()
    {
        // Arrange
        var omschrijving = "Hetzelfde";
        var moment = new AfvalOphaalMoment(AfvalType.GRIJS, DateTime.Now, omschrijving, "1234AB", "10");
        var oudeDatum = moment.LaatstGewijzigd;

        // Act
        moment.Update(omschrijving);

        // Assert
        moment.Omschrijving.Should().Be(omschrijving);
        moment.LaatstGewijzigd.Should().Be(oudeDatum);
    }
}
