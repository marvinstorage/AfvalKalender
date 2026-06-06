using AfvalKalender.Application.Commands;
using AfvalKalender.DesktopUI.ViewModels;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AfvalKalender.DesktopUI.Tests;

public class MainWindowViewModelTests
{
    private static Mock<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>
        MaakMockHandler(IReadOnlyList<AfvalOphaalMoment>? resultaat = null)
    {
        var mock = new Mock<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>();
        mock.Setup(x => x.HandleAsync(It.IsAny<VerwerkKalenderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultaat ?? new List<AfvalOphaalMoment>());
        return mock;
    }

    [Fact]
    public async Task VerwerkCommand_MetGeldigeInput_ZouHeeftResultaatOpTrueMoetenZetten()
    {
        // Arrange
        var momenten = new List<AfvalOphaalMoment>
        {
            new(AfvalType.GRIJS, DateTime.Now, "Test", "1234AB", "10")
        };
        var viewModel = new MainWindowViewModel(MaakMockHandler(momenten).Object)
        {
            Postcode = "1234AB",
            Huisnummer = "10",
            Jaar = 2026
        };

        // Act
        await viewModel.VerwerkCommand.ExecuteAsync(null);

        // Assert
        viewModel.HeeftResultaat.Should().BeTrue();
        viewModel.OutputBestandPad.Should().NotBeEmpty();
        viewModel.StatusBericht.Should().Contain("1");
    }

    [Fact]
    public async Task VerwerkCommand_MetLegePostcode_ZouFoutBerichtMoetenTonen()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(MaakMockHandler().Object)
        {
            Postcode = "",
            Huisnummer = "10"
        };

        // Act
        await viewModel.VerwerkCommand.ExecuteAsync(null);

        // Assert
        viewModel.HeeftResultaat.Should().BeFalse();
        viewModel.StatusBericht.Should().Contain("verplicht");
    }

    [Fact]
    public async Task VerwerkCommand_StuurJuisteCommandNaarHandler()
    {
        // Arrange
        var mock = MaakMockHandler();
        var viewModel = new MainWindowViewModel(mock.Object)
        {
            Postcode = "7522NG",
            Huisnummer = "45",
            Jaar = 2026,
            HerinneringUur = 8
        };

        // Act
        await viewModel.VerwerkCommand.ExecuteAsync(null);

        // Assert
        mock.Verify(x => x.HandleAsync(
            It.Is<VerwerkKalenderCommand>(c =>
                c.Postcode == "7522NG" &&
                c.Huisnummer == "45" &&
                c.Jaar == 2026 &&
                c.HerinneringUur == 8),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void OpenBestandCommand_ZouMoetenBestaan()
    {
        var viewModel = new MainWindowViewModel(MaakMockHandler().Object);
        viewModel.OpenBestandCommand.Should().NotBeNull();
    }
}
