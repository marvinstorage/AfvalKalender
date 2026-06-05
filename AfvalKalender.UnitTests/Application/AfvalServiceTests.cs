using AfvalKalender.Application.Services;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;
using Moq;
using Xunit;

namespace AfvalKalender.UnitTests.Application;

public class AfvalServiceTests
{
    [Fact]
    public async Task VerwerkKalenderAsync_ZouJuisteVolgordeMoetenAanhouden()
    {
        // Arrange
        var mockApi = new Mock<IAfvalApi>();
        var mockRepo = new Mock<IAfvalRepository>();
        var mockIcs = new Mock<IIcsExporter>();

        var postcode = "1234AB";
        var huisnummer = "10";
        var jaar = 2026;
        var uniekId = "unique-123";
        var momenten = new List<AfvalOphaalMoment> 
        { 
            new AfvalOphaalMoment(AfvalType.GRIJS, DateTime.Now, "Test", postcode, huisnummer) 
        };

        mockApi.Setup(x => x.HaalUniekAdresIdOpAsync(postcode, huisnummer)).ReturnsAsync(uniekId);
        mockApi.Setup(x => x.HaalKalenderOpAsync(uniekId, postcode, huisnummer, jaar)).ReturnsAsync(momenten);
        mockRepo.Setup(x => x.HaalOpVoorAdresEnJaarAsync(postcode, huisnummer, jaar)).ReturnsAsync(momenten);

        var service = new AfvalService(mockApi.Object, mockRepo.Object, mockIcs.Object);

        // Act
        await service.VerwerkKalenderAsync(postcode, huisnummer, jaar, 13, "test.ics");

        // Assert
        mockApi.Verify(x => x.HaalUniekAdresIdOpAsync(postcode, huisnummer), Times.Once);
        mockApi.Verify(x => x.HaalKalenderOpAsync(uniekId, postcode, huisnummer, jaar), Times.Once);
        mockRepo.Verify(x => x.SlaOpOfUpdateAsync(momenten), Times.Once);
        mockIcs.Verify(x => x.ExporteerAsync(momenten, "test.ics", 13), Times.Once);
    }
}
