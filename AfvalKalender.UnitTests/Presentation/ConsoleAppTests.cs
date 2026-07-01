using AfvalKalender.Application.Commands;
using AfvalKalender.ConsoleUI;
using AfvalKalender.Domain.Entities;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace AfvalKalender.UnitTests.Presentation;

public class ConsoleAppTests
{
    [Fact]
    public void Constructor_Should_Initialize_Without_Errors()
    {
        // Arrange
        var mockHandler = new Mock<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>();

        // Act
        var app = new ConsoleApp(mockHandler.Object);

        // Assert
        app.Should().NotBeNull();
    }
}
