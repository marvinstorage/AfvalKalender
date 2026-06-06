using Moq;
using FluentAssertions;
using AfvalKalender.AndroidUI.ViewModels;
using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AfvalKalender.AndroidUI.Tests;

public class MainPageViewModelTests
{
    private readonly Mock<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>> _handlerMock;
    private readonly MainPageViewModel _viewModel;

    public MainPageViewModelTests()
    {
        _handlerMock = new Mock<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>();
        _viewModel = new MainPageViewModel(_handlerMock.Object);
    }

    [Fact]
    public void Constructor_ZouInitialiserenMetDefaultWaarden()
    {
        _viewModel.Postcode.Should().BeEmpty();
        _viewModel.Huisnummer.Should().BeEmpty();
        _viewModel.IsBezig.Should().BeFalse();
        _viewModel.StatusBericht.Should().Be("Klaar voor gebruik");
        _viewModel.GeselecteerdeVerwerker.Should().NotBeNull();
    }

    [Fact]
    public async Task VerwerkAsync_ZouFoutMelden_WanneerGeenDataIngevuld()
    {
        _viewModel.Postcode = "";
        _viewModel.Huisnummer = "";

        await _viewModel.VerwerkCommand.ExecuteAsync(null);

        _viewModel.StatusBericht.Should().Contain("Fout");
        _viewModel.HeeftResultaat.Should().BeFalse();
    }

    [Fact]
    public async Task VerwerkAsync_ZouGeslaagdMelden_WanneerDataGeldigIs()
    {
        // Arrange
        _viewModel.Postcode = "1234AB";
        _viewModel.Huisnummer = "10";
        var momenten = new List<AfvalOphaalMoment>
        {
            new AfvalOphaalMoment(AfvalType.GRIJS, System.DateTime.Now, "Test", "1234AB", "10")
        };
        _handlerMock.Setup(h => h.HandleAsync(It.IsAny<VerwerkKalenderCommand>(), default))
            .ReturnsAsync(momenten);

        // Act
        await _viewModel.VerwerkCommand.ExecuteAsync(null);

        // Assert
        _viewModel.StatusBericht.Should().Contain("Succes");
        _viewModel.HeeftResultaat.Should().BeTrue();
        _viewModel.OutputBestandPad.Should().NotBeNullOrEmpty();
    }
}
