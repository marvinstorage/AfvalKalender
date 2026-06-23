using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Domain.ValueObjects;
using AfvalKalender.Infrastructure.Sync;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AfvalKalender.Infrastructure.Tests.Sync;

public class WebDavSyncAdapterTests
{
    private readonly Mock<IIcsExporter> _mockIcsExporter = new();

    [Fact]
    public async Task SynchroniseerAsync_MetGeldigeParameters_VerstuurtPutRequestMetBasicAuth()
    {
        // Arrange
        var momenten = new List<AfvalOphaalMoment>
        {
            new(AfvalType.GRIJS, DateTime.Today, "Test", "1234AB", "10")
        };

        _mockIcsExporter.Setup(x => x.ExporteerAsync(momenten, It.IsAny<string>(), 13))
            .Callback<IEnumerable<AfvalOphaalMoment>, string, int>((m, path, hr) =>
            {
                File.WriteAllText(path, "BEGIN:VCALENDAR\nEND:VCALENDAR");
            })
            .Returns(Task.CompletedTask);

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri == new Uri("https://dav.test.org/calendar.ics") &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Scheme == "Basic" &&
                    req.Content != null),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var sut = new WebDavSyncAdapter(httpClient, _mockIcsExporter.Object);

        // Act
        Func<Task> act = () => sut.SynchroniseerAsync(
            momenten, 
            new SyncConfiguratie(SyncProvider.WebDav, "https://dav.test.org/calendar.ics", "user", "pass"), 
            13);

        // Assert
        await act.Should().NotThrowAsync();
        _mockIcsExporter.Verify(x => x.ExporteerAsync(momenten, It.IsAny<string>(), 13), Times.Once);
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SynchroniseerAsync_MetLegeUrl_ZouArgumentExceptionMoetenGooien()
    {
        // Arrange
        var httpClient = new HttpClient();
        var sut = new WebDavSyncAdapter(httpClient, _mockIcsExporter.Object);

        // Act
        Func<Task> act = () => sut.SynchroniseerAsync(
            new List<AfvalOphaalMoment>(), 
            new SyncConfiguratie(SyncProvider.WebDav, "", "user", "pass"), 
            13);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*WebDAV URL*");
    }

    [Fact]
    public async Task SynchroniseerAsync_WanneerHttpFoutOptreedt_ZouMoetenFalen()
    {
        // Arrange
        var momenten = new List<AfvalOphaalMoment>();
        _mockIcsExporter.Setup(x => x.ExporteerAsync(momenten, It.IsAny<string>(), 13))
            .Callback<IEnumerable<AfvalOphaalMoment>, string, int>((m, path, hr) =>
            {
                File.WriteAllText(path, "TEST CONTENT");
            })
            .Returns(Task.CompletedTask);

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var sut = new WebDavSyncAdapter(httpClient, _mockIcsExporter.Object);

        // Act
        Func<Task> act = () => sut.SynchroniseerAsync(
            momenten, 
            new SyncConfiguratie(SyncProvider.WebDav, "https://dav.test.org/calendar.ics", "wrong", "wrong"), 
            13);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
