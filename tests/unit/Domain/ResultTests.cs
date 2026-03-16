using FluentAssertions;
using Loca.Domain.Common;

namespace Loca.Tests.Unit.Domain;

public class ResultTests
{
    [Fact]
    public void Should_BeSuccess_When_CreatedWithValue()
    {
        var result = Result<string>.Success("test");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Should_BeFailure_When_CreatedWithError()
    {
        var result = Result<string>.Failure("ERR_CODE", "Something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("ERR_CODE");
        result.Error.Message.Should().Be("Something went wrong");
    }

    [Fact]
    public void Should_MatchSuccess_When_ResultIsSuccess()
    {
        var result = Result<int>.Success(42);

        var output = result.Match(
            value => $"Got {value}",
            error => $"Error: {error.Code}"
        );

        output.Should().Be("Got 42");
    }

    [Fact]
    public void Should_MatchFailure_When_ResultIsFailure()
    {
        var result = Result<int>.Failure("NOT_FOUND", "Not found");

        var output = result.Match(
            value => $"Got {value}",
            error => $"Error: {error.Code}"
        );

        output.Should().Be("Error: NOT_FOUND");
    }
}
