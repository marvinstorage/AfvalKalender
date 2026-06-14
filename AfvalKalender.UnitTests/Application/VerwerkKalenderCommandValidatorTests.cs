using System;
using AfvalKalender.Application.Commands;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.UnitTests.Application;

public class VerwerkKalenderCommandValidatorTests
{
    private readonly VerwerkKalenderCommandValidator _validator = new();

    [Fact]
    public void Validate_MetGeldigeData_ZouNietMoetenGoorden()
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: "1234AB",
            Huisnummer: "10",
            Jaar: 2026,
            HerinneringUur: 13,
            OutputPad: "test.ics",
            CompanyCode: "8d97bb56-5afd-4cbc-a651-b4f7314264b4"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_MetNullCommand_ZouArgumentNullExceptionMoetenGooien()
    {
        // Act & Assert
        Action act = () => _validator.Validate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_MetLegePostcode_ZouArgumentExceptionMoetenGooien(string postcode)
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: postcode,
            Huisnummer: "10",
            Jaar: 2026,
            HerinneringUur: 13,
            OutputPad: "test.ics",
            CompanyCode: "8d97bb56-5afd-4cbc-a651-b4f7314264b4"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().Throw<ArgumentException>().WithMessage("*Postcode is verplicht*");
    }

    [Theory]
    [InlineData("1234 AB")] // space not allowed (should be stripped beforehand)
    [InlineData("123AB")]   // too short digits
    [InlineData("12345AB")]  // too long digits
    [InlineData("1234A")]   // too short letters
    [InlineData("1234ABC")]  // too long letters
    [InlineData("0123AB")]   // starts with 0 (invalid Dutch postcode)
    [InlineData("1234ab")]   // lower-case letters not allowed at use-case boundary (normalized to upper-case beforehand)
    public void Validate_MetOngeldigePostcodeFormaat_ZouArgumentExceptionMoetenGooien(string postcode)
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: postcode,
            Huisnummer: "10",
            Jaar: 2026,
            HerinneringUur: 13,
            OutputPad: "test.ics",
            CompanyCode: "8d97bb56-5afd-4cbc-a651-b4f7314264b4"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().Throw<ArgumentException>().WithMessage("*Postcode is ongeldig*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_MetLeegHuisnummer_ZouArgumentExceptionMoetenGooien(string huisnummer)
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: "1234AB",
            Huisnummer: huisnummer,
            Jaar: 2026,
            HerinneringUur: 13,
            OutputPad: "test.ics",
            CompanyCode: "8d97bb56-5afd-4cbc-a651-b4f7314264b4"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().Throw<ArgumentException>().WithMessage("*Huisnummer is verplicht*");
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Validate_MetJaarBuitenBereik_ZouArgumentExceptionMoetenGooien(int jaar)
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: "1234AB",
            Huisnummer: "10",
            Jaar: jaar,
            HerinneringUur: 13,
            OutputPad: "test.ics",
            CompanyCode: "8d97bb56-5afd-4cbc-a651-b4f7314264b4"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().Throw<ArgumentException>().WithMessage("*Jaar moet tussen*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    public void Validate_MetHerinneringUurBuitenBereik_ZouArgumentExceptionMoetenGooien(int uur)
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: "1234AB",
            Huisnummer: "10",
            Jaar: 2026,
            HerinneringUur: uur,
            OutputPad: "test.ics",
            CompanyCode: "8d97bb56-5afd-4cbc-a651-b4f7314264b4"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().Throw<ArgumentException>().WithMessage("*HerinneringUur moet tussen*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_MetLeegOutputPad_ZouArgumentExceptionMoetenGooien(string pad)
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: "1234AB",
            Huisnummer: "10",
            Jaar: 2026,
            HerinneringUur: 13,
            OutputPad: pad,
            CompanyCode: "8d97bb56-5afd-4cbc-a651-b4f7314264b4"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().Throw<ArgumentException>().WithMessage("*OutputPad is verplicht*");
    }

    [Fact]
    public void Validate_MetOngeldigeCompanyCode_ZouArgumentExceptionMoetenGooien()
    {
        // Arrange
        var command = new VerwerkKalenderCommand(
            Postcode: "1234AB",
            Huisnummer: "10",
            Jaar: 2026,
            HerinneringUur: 13,
            OutputPad: "test.ics",
            CompanyCode: "ongeldige-guid"
        );

        // Act & Assert
        Action act = () => _validator.Validate(command);
        act.Should().Throw<ArgumentException>().WithMessage("*CompanyCode is geen geldige GUID*");
    }
}
