using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class ExceptionFlattenerTests
{
    [Fact]
    public void Flatten_SingleException_ReturnsOneItem()
    {
        var ex = new InvalidOperationException("test error");

        var result = ExceptionFlattener.Flatten(ex);

        result.Should().HaveCount(1);
        result[0].Message.Should().Be("test error");
        result[0].Type.Should().Contain("InvalidOperationException");
    }

    [Fact]
    public void Flatten_InnerException_ReturnsTwoItems()
    {
        var inner = new ArgumentException("inner error");
        var outer = new InvalidOperationException("outer error", inner);

        var result = ExceptionFlattener.Flatten(outer);

        result.Should().HaveCount(2);
        result[0].Message.Should().Be("outer error");
        result[1].Message.Should().Be("inner error");
    }

    [Fact]
    public void Flatten_DeepChain_ReturnsAllItems()
    {
        var ex = new InvalidOperationException("level 3",
            new ArgumentException("level 2",
                new NotSupportedException("level 1")));

        var result = ExceptionFlattener.Flatten(ex);

        result.Should().HaveCount(3);
        result[0].Message.Should().Be("level 3");
        result[1].Message.Should().Be("level 2");
        result[2].Message.Should().Be("level 1");
    }

    [Fact]
    public void Flatten_AggregateException_ReturnsAllInner()
    {
        var ex = new AggregateException(
            new InvalidOperationException("error 1"),
            new ArgumentException("error 2"));

        var result = ExceptionFlattener.Flatten(ex);

        result.Should().HaveCount(3); // AggregateException + 2 inner
        result[0].Type.Should().Contain("AggregateException");
        result[1].Message.Should().Be("error 1");
        result[2].Message.Should().Be("error 2");
    }

    [Fact]
    public void Flatten_NullException_ReturnsEmpty()
    {
        var result = ExceptionFlattener.Flatten(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Unwrap_SingleException_ReturnsMessage()
    {
        var ex = new InvalidOperationException("test error");

        ExceptionFlattener.Unwrap(ex).Should().Be("test error");
    }

    [Fact]
    public void Unwrap_DeepChain_ReturnsDeepestMessage()
    {
        var ex = new InvalidOperationException("outer",
            new ArgumentException("inner",
                new NotSupportedException("deepest")));

        ExceptionFlattener.Unwrap(ex).Should().Be("deepest");
    }

    [Fact]
    public void LogFlattened_CallsLogger()
    {
        var logger = new Mock<ILogger>();
        var ex = new InvalidOperationException("test error");

        ExceptionFlattener.LogFlattened(logger.Object, ex);

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
