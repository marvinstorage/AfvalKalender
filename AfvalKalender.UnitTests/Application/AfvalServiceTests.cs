using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AfvalKalender.UnitTests.Application;

public class VerwerkKalenderCommandHandlerTests
{
    private readonly Mock<IAfvalApi> _mockApi = new();
    private readonly Mock<IAfvalRepository> _mockRepo = new();
    private readonly Mock<IIcsExporter> _mockIcs = new();
    private readonly Mock<IAfvalKalenderSynchronisator> _mockSync = new();

    private VerwerkKalenderCommandHandler MaakHandler() =>
        new(_mockApi.Object, _mockRepo.Object, _mockIcs.Object, _mockSync.Object);

    [Fact]
    public async Task HandleAsync_ZouJuisteVolgordeMoetenAanhouden()
    {
        // Arrange
        var momenten = new List<AfvalOphaalMoment>
        {
            new(AfvalType.GRIJS, DateTime.Now, "Test", "1234AB", "10")
        };
        _mockApi.Setup(x => x.HaalUniekAdresIdOpAsync("1234AB", "10")).ReturnsAsync("uniek-123");
        _mockApi.Setup(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026)).ReturnsAsync(momenten);
        _mockRepo.Setup(x => x.HaalOpVoorAdresEnJaarAsync("1234AB", "10", 2026)).ReturnsAsync(momenten);

        var command = new VerwerkKalenderCommand("1234AB", "10", 2026, 13, "test.ics");

        // Act
        await MaakHandler().HandleAsync(command);

        // Assert
        _mockApi.Verify(x => x.HaalUniekAdresIdOpAsync("1234AB", "10"), Times.Once);
        _mockApi.Verify(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026), Times.Once);
        _mockRepo.Verify(x => x.SlaOpOfUpdateAsync(momenten), Times.Once);
        _mockIcs.Verify(x => x.ExporteerAsync(momenten, "test.ics", 13), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOpgeslagenMomenten()
    {
        // Arrange
        var apiMomenten = new List<AfvalOphaalMoment> { new(AfvalType.GRIJS, DateTime.Now, "Api", "1234AB", "10") };
        var dbMomenten  = new List<AfvalOphaalMoment> { new(AfvalType.GRIJS, DateTime.Now, "Db",  "1234AB", "10") };

        _mockApi.Setup(x => x.HaalUniekAdresIdOpAsync("1234AB", "10")).ReturnsAsync("id");
        _mockApi.Setup(x => x.HaalKalenderOpAsync("id", "1234AB", "10", 2026)).ReturnsAsync(apiMomenten);
        _mockRepo.Setup(x => x.HaalOpVoorAdresEnJaarAsync("1234AB", "10", 2026)).ReturnsAsync(dbMomenten);

        var command = new VerwerkKalenderCommand("1234AB", "10", 2026, 13, "test.ics");

        // Act
        var resultaat = await MaakHandler().HandleAsync(command);

        // Assert — handler returns the DB read, not the raw API response
        resultaat.Should().HaveCount(1);
        resultaat[0].Omschrijving.Should().Be("Db");
    }

    [Fact]
    public async Task HandleAsync_CommandPropagatesAllParameters()
    {
        // Arrange
        var momenten = new List<AfvalOphaalMoment> { new(AfvalType.GROEN, DateTime.Now, "GFT", "9999ZZ", "99") };
        _mockApi.Setup(x => x.HaalUniekAdresIdOpAsync("9999ZZ", "99")).ReturnsAsync("xyz");
        _mockApi.Setup(x => x.HaalKalenderOpAsync("xyz", "9999ZZ", "99", 2027)).ReturnsAsync(momenten);
        _mockRepo.Setup(x => x.HaalOpVoorAdresEnJaarAsync("9999ZZ", "99", 2027)).ReturnsAsync(momenten);

        var command = new VerwerkKalenderCommand("9999ZZ", "99", 2027, 8, "output.ics");

        // Act
        await MaakHandler().HandleAsync(command);

        // Assert — all command values are forwarded correctly
        _mockApi.Verify(x => x.HaalKalenderOpAsync("xyz", "9999ZZ", "99", 2027), Times.Once);
        _mockIcs.Verify(x => x.ExporteerAsync(momenten, "output.ics", 8), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MetWebDavUrl_ZouSynchronisatorMoetenAanroepen()
    {
        // Arrange
        var momenten = new List<AfvalOphaalMoment> { new(AfvalType.GRIJS, DateTime.Now, "Test", "1234AB", "10") };
        _mockRepo.Setup(x => x.HaalOpVoorAdresEnJaarAsync("1234AB", "10", 2026)).ReturnsAsync(momenten);

        var command = new VerwerkKalenderCommand("1234AB", "10", 2026, 13, "test.ics", "company", false, "https://dav", "user", "pass");

        // Act
        await MaakHandler().HandleAsync(command);

        // Assert
        _mockSync.Verify(x => x.SynchroniseerAsync(momenten, "https://dav", "user", "pass", 13), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ZonderWebDavUrl_ZouSynchronisatorNietMoetenAanroepen()
    {
        // Arrange
        var command = new VerwerkKalenderCommand("1234AB", "10", 2026, 13, "test.ics");

        // Act
        await MaakHandler().HandleAsync(command);

        // Assert
        _mockSync.Verify(x => x.SynchroniseerAsync(It.IsAny<IEnumerable<AfvalOphaalMoment>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }
}
