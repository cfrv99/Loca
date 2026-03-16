using FluentAssertions;
using Loca.Domain.Common;

namespace Loca.Tests.Unit.Domain;

public class ResultTests
{
    [Fact]
    public void Should_CreateSuccessResult()
    {
        var result = Result<string>.Success("test");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Should_CreateFailureResult()
    {
        var result = Result<string>.Failure("ERR_CODE", "Error message");
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("ERR_CODE");
        result.Error!.Message.Should().Be("Error message");
    }

    [Fact]
    public void Should_Match_OnSuccess()
    {
        var result = Result<int>.Success(42);
        var output = result.Match(
            val => $"Value: {val}",
            err => $"Error: {err.Code}"
        );
        output.Should().Be("Value: 42");
    }

    [Fact]
    public void Should_Match_OnFailure()
    {
        var result = Result<int>.Failure("NOT_FOUND", "Not found");
        var output = result.Match(
            val => $"Value: {val}",
            err => $"Error: {err.Code}"
        );
        output.Should().Be("Error: NOT_FOUND");
    }
}
