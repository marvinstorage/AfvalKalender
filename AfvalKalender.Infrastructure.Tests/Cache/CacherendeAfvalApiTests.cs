using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Infrastructure.Cache;
using FluentAssertions;
using Moq;
using Xunit;

namespace AfvalKalender.Infrastructure.Tests.Cache;

public class CacherendeAfvalApiTests : IDisposable
{
    private readonly string _tijdelijkePad = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly Mock<AfvalKalender.Domain.Interfaces.IAfvalApi> _mockInnerApi = new();

    public void Dispose()
    {
        if (Directory.Exists(_tijdelijkePad))
            Directory.Delete(_tijdelijkePad, recursive: true);
    }

    private AfvalOphaalMoment MaakMoment(AfvalType type = AfvalType.GRIJS) =>
        new(type, new DateTime(2026, 1, 15), "Restafval wordt opgehaald", "1234AB", "10");

    [Fact]
    public async Task HaalUniekAdresIdOpAsync_EersteAanroep_RoeptApiAanEnCachesResultaat()
    {
        // Arrange
        _mockInnerApi.Setup(x => x.HaalUniekAdresIdOpAsync("1234AB", "10")).ReturnsAsync("uniek-123");
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);

        // Act
        var resultaat = await sut.HaalUniekAdresIdOpAsync("1234AB", "10");

        // Assert
        resultaat.Should().Be("uniek-123");
        _mockInnerApi.Verify(x => x.HaalUniekAdresIdOpAsync("1234AB", "10"), Times.Once);
    }

    [Fact]
    public async Task HaalUniekAdresIdOpAsync_TweedeAanroepBinnenTtl_RoeptApiNietNogmaalsAan()
    {
        // Arrange
        _mockInnerApi.Setup(x => x.HaalUniekAdresIdOpAsync("1234AB", "10")).ReturnsAsync("uniek-123");
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);
        await sut.HaalUniekAdresIdOpAsync("1234AB", "10");

        // Act
        var resultaat = await sut.HaalUniekAdresIdOpAsync("1234AB", "10");

        // Assert
        resultaat.Should().Be("uniek-123");
        _mockInnerApi.Verify(x => x.HaalUniekAdresIdOpAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HaalUniekAdresIdOpAsync_CacheVerlopen_RoeptApiNogmaalsAan()
    {
        // Arrange
        var tijdstip = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _mockInnerApi.Setup(x => x.HaalUniekAdresIdOpAsync("1234AB", "10")).ReturnsAsync("uniek-123");
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad, () => tijdstip);
        await sut.HaalUniekAdresIdOpAsync("1234AB", "10");

        // Zet klok 25 uur vooruit (voorbij TTL van 24 uur)
        var verlopen = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad,
            () => tijdstip.AddHours(25));

        // Act
        await verlopen.HaalUniekAdresIdOpAsync("1234AB", "10");

        // Assert
        _mockInnerApi.Verify(x => x.HaalUniekAdresIdOpAsync("1234AB", "10"), Times.Exactly(2));
    }

    [Fact]
    public async Task HaalKalenderOpAsync_EersteAanroep_RoeptApiAanEnCachesResultaat()
    {
        // Arrange
        var momenten = new[] { MaakMoment() };
        _mockInnerApi.Setup(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026))
            .ReturnsAsync(momenten);
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);

        // Act
        var resultaat = (await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026)).ToList();

        // Assert
        resultaat.Should().HaveCount(1);
        resultaat[0].Type.Should().Be(AfvalType.GRIJS);
        resultaat[0].Datum.Should().Be(new DateTime(2026, 1, 15));
        _mockInnerApi.Verify(x => x.HaalKalenderOpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HaalKalenderOpAsync_TweedeAanroepBinnenTtl_RoeptApiNietNogmaalsAan()
    {
        // Arrange
        var momenten = new[] { MaakMoment(AfvalType.PAPIER) };
        _mockInnerApi.Setup(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026))
            .ReturnsAsync(momenten);
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);
        await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026);

        // Act
        var resultaat = (await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026)).ToList();

        // Assert
        resultaat.Should().HaveCount(1);
        resultaat[0].Type.Should().Be(AfvalType.PAPIER);
        _mockInnerApi.Verify(
            x => x.HaalKalenderOpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task HaalKalenderOpAsync_CacheVerlopen_RoeptApiNogmaalsAan()
    {
        // Arrange
        var tijdstip = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var momenten = new[] { MaakMoment() };
        _mockInnerApi.Setup(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026))
            .ReturnsAsync(momenten);
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad, () => tijdstip);
        await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026);

        var verlopen = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad,
            () => tijdstip.AddHours(25));

        // Act
        await verlopen.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026);

        // Assert
        _mockInnerApi.Verify(
            x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026),
            Times.Exactly(2));
    }

    [Fact]
    public async Task HaalKalenderOpAsync_CacheBevatAlleAfvalTypen_WordenCorrectHersteld()
    {
        // Arrange
        var momenten = new[]
        {
            new AfvalOphaalMoment(AfvalType.GRIJS,        new DateTime(2026, 1, 1), "Restafval", "1234AB", "10"),
            new AfvalOphaalMoment(AfvalType.GROEN,        new DateTime(2026, 1, 2), "GFT", "1234AB", "10"),
            new AfvalOphaalMoment(AfvalType.PAPIER,       new DateTime(2026, 1, 3), "Papier", "1234AB", "10"),
            new AfvalOphaalMoment(AfvalType.VERPAKKINGEN, new DateTime(2026, 1, 4), "Plastic", "1234AB", "10"),
            new AfvalOphaalMoment(AfvalType.KERSTBOOM,    new DateTime(2026, 1, 5), "Kerstboom", "1234AB", "10"),
        };
        _mockInnerApi.Setup(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026))
            .ReturnsAsync(momenten);
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);
        await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026);

        // Act — lees uit cache
        var resultaat = (await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026))
            .OrderBy(m => m.Datum).ToList();

        // Assert
        resultaat.Select(m => m.Type).Should().ContainInOrder(
            AfvalType.GRIJS, AfvalType.GROEN, AfvalType.PAPIER, AfvalType.VERPAKKINGEN, AfvalType.KERSTBOOM);
    }

    [Fact]
    public async Task HaalKalenderOpAsync_VerschillendeAdressen_GebruikenAparteCachebestanden()
    {
        // Arrange
        var momentenAdres1 = new[] { new AfvalOphaalMoment(AfvalType.GRIJS, DateTime.Today, "Test1", "1234AB", "10") };
        var momentenAdres2 = new[] { new AfvalOphaalMoment(AfvalType.GROEN, DateTime.Today, "Test2", "5678CD", "20") };

        _mockInnerApi.Setup(x => x.HaalKalenderOpAsync("id-1", "1234AB", "10", 2026)).ReturnsAsync(momentenAdres1);
        _mockInnerApi.Setup(x => x.HaalKalenderOpAsync("id-2", "5678CD", "20", 2026)).ReturnsAsync(momentenAdres2);
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);

        // Act
        var r1 = (await sut.HaalKalenderOpAsync("id-1", "1234AB", "10", 2026)).ToList();
        var r2 = (await sut.HaalKalenderOpAsync("id-2", "5678CD", "20", 2026)).ToList();

        // Assert — beide uit API (nog geen cache) en correct gescheiden
        r1[0].Type.Should().Be(AfvalType.GRIJS);
        r2[0].Type.Should().Be(AfvalType.GROEN);
    }

    [Fact]
    public async Task HaalUniekAdresIdOpAsync_MetForceerVernieuwen_RoeptApiOpnieuwAanZelfsBinnenTtl()
    {
        // Arrange
        _mockInnerApi.Setup(x => x.HaalUniekAdresIdOpAsync("1234AB", "10", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync("uniek-123");
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);
        await sut.HaalUniekAdresIdOpAsync("1234AB", "10");

        // Act
        var resultaat = await sut.HaalUniekAdresIdOpAsync("1234AB", "10", forceerVernieuwen: true);

        // Assert
        resultaat.Should().Be("uniek-123");
        _mockInnerApi.Verify(x => x.HaalUniekAdresIdOpAsync("1234AB", "10", It.IsAny<string>(), false), Times.Once);
        _mockInnerApi.Verify(x => x.HaalUniekAdresIdOpAsync("1234AB", "10", It.IsAny<string>(), true), Times.Once);
    }

    [Fact]
    public async Task HaalKalenderOpAsync_MetForceerVernieuwen_RoeptApiOpnieuwAanZelfsBinnenTtl()
    {
        // Arrange
        var momenten = new[] { MaakMoment() };
        _mockInnerApi.Setup(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026, It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(momenten);
        var sut = new CacherendeAfvalApi(_mockInnerApi.Object, _tijdelijkePad);
        await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026);

        // Act
        await sut.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026, forceerVernieuwen: true);

        // Assert
        _mockInnerApi.Verify(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026, It.IsAny<string>(), false), Times.Once);
        _mockInnerApi.Verify(x => x.HaalKalenderOpAsync("uniek-123", "1234AB", "10", 2026, It.IsAny<string>(), true), Times.Once);
    }
}
