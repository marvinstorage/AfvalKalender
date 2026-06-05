using AfvalKalender.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.UnitTests.Domain;

public class AdresTests
{
    [Fact]
    public void Constructor_MetGeldigeData_ZouAdresMoetenAanmaken()
    {
        // Arrange
        var postcode = "1234AB";
        var huisnummer = "10";

        // Act
        var adres = new Adres(postcode, huisnummer);

        // Assert
        adres.Postcode.Should().Be(postcode);
        adres.Huisnummer.Should().Be(huisnummer);
    }

    [Theory]
    [InlineData("", "10")]
    [InlineData("1234AB", "")]
    [InlineData(null, "10")]
    [InlineData("1234AB", null)]
    public void Constructor_MetOngeldigeData_ZouExceptionMoetenGooien(string postcode, string huisnummer)
    {
        // Act
        Action act = () => new Adres(postcode, huisnummer);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
