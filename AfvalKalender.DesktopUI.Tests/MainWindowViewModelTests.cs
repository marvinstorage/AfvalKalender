using AfvalKalender.DesktopUI.ViewModels;
using AfvalKalender.Application.Services;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Domain.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace AfvalKalender.DesktopUI.Tests;

public class MainWindowViewModelTests
{
    [Fact]
    public async Task VerwerkCommand_MetGeldigeInput_ZouHeeftResultaatOpTrueMoetenZetten()
    {
        // Arrange
        var mockApi = new Mock<IAfvalApi>();
        var mockRepo = new Mock<IAfvalRepository>();
        var mockIcs = new Mock<IIcsExporter>();
        
        var service = new AfvalService(mockApi.Object, mockRepo.Object, mockIcs.Object);
        var viewModel = new MainWindowViewModel(service)
        {
            Postcode = "1234AB",
            Huisnummer = "10",
            Jaar = 2026
        };

        var momenten = new List<AfvalOphaalMoment> { new AfvalOphaalMoment(AfvalType.GRIJS, DateTime.Now, "Test", "1234AB", "10") };
        mockApi.Setup(x => x.HaalUniekAdresIdOpAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("id");
        mockApi.Setup(x => x.HaalKalenderOpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(momenten);
        mockRepo.Setup(x => x.HaalOpVoorAdresEnJaarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(momenten);

        // Act
        await viewModel.VerwerkCommand.ExecuteAsync(null);

        // Assert
        viewModel.HeeftResultaat.Should().BeTrue();
        viewModel.OutputBestandPad.Should().NotBeEmpty();
    }

    [Fact]
    public void OpenBestandCommand_ZouMoetenBestaat()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();
        
        // Assert
        viewModel.OpenBestandCommand.Should().NotBeNull();
    }
}
